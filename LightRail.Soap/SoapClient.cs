using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace LightRail.Soap;

public record SoapMessageConfiguration(SoapVersion SoapVersion)
{
    public string MediaType =>
        SoapVersion == SoapVersion.Soap11
            ? "text/xml"
            : "application/soap+xml";

    public XNamespace Schema =>
        SoapVersion == SoapVersion.Soap11
            ? "http://schemas.xmlsoap.org/soap/envelope/"
            : "http://www.w3.org/2003/05/soap-envelope";
}

public enum SoapVersion
{
    Soap11 = 11,
    Soap12 = 12
}

public interface ISoapClient
{
    /// <summary>
    /// Posts an asynchronous message.
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="soapVersion">The preferred SOAP version.</param>
    /// <param name="bodies">The body of the SOAP message.</param>
    /// <param name="headers">The header of the SOAP message.</param>
    /// <param name="action">The SOAPAction of the SOAP message.</param>
    Task<HttpResponseMessage> PostAsync(Uri endpoint, SoapVersion soapVersion, IEnumerable<XElement> bodies, IEnumerable<XElement>? headers = null, string? action = null, CancellationToken cancellationToken = default(CancellationToken));
}

public class SoapClient : ISoapClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SoapClient" /> class.
    /// </summary>
    /// <param name="IHttpClientFactory">Microsoft.Extensions.Http HttpClientFactory</param>
    public SoapClient(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    public SoapClient()
        => _httpClientFactory = DefaultHttpClientFactory();

    /// <inheritdoc />
    public async Task<HttpResponseMessage> PostAsync(
        Uri endpoint,
        SoapVersion soapVersion,
        IEnumerable<XElement> bodies,
        IEnumerable<XElement>? headers = null,
        string? action = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (endpoint == null)
            throw new ArgumentNullException(nameof(endpoint));

        if (bodies == null)
            throw new ArgumentNullException(nameof(bodies));

        if (!bodies.Any())
            throw new ArgumentException("Bodies element cannot be empty", nameof(bodies));

        // Get configuration based on version
        var messageConfiguration = new SoapMessageConfiguration(soapVersion);

        // Get the envelope
        var envelope = GetEnvelope(messageConfiguration);

        // Add headers
        if (headers != null && headers.Any())
            envelope.Add(new XElement(messageConfiguration.Schema + "Header", headers));

        // Add bodies
        envelope.Add(new XElement(messageConfiguration.Schema + "Body", bodies));

        // Get HTTP content
        var content = new StringContent(envelope.ToString(), Encoding.UTF8, messageConfiguration.MediaType);

        // Add SOAP action if any
        if (action != null)
        {
            content.Headers.Add("SOAPAction", action);

            if (messageConfiguration.SoapVersion == SoapVersion.Soap12)
                content.Headers.ContentType!.Parameters.Add(
                    new NameValueHeaderValue("ActionParameter", $"\"{action}\""));
        }

        string contentRaw = await content.ReadAsStringAsync(cancellationToken);
        
        // Execute call
        var httpClient = _httpClientFactory.CreateClient(nameof(SoapClient));
        var response = await httpClient.PostAsync(endpoint, content, cancellationToken);

        if (response.Content.Headers.ContentType?.MediaType != messageConfiguration.MediaType)
            throw new Exception("Response is not xml");    
        
        //var responseContent = await response.Content.ReadAsStringAsync();
        
        return response;
    }

    #region Private Methods

    private static XElement GetEnvelope(SoapMessageConfiguration soapMessageConfiguration)
    {
        return new
            XElement(
                soapMessageConfiguration.Schema + "Envelope",
                new XAttribute(
                    XNamespace.Xmlns + "soapenv",
                    soapMessageConfiguration.Schema.NamespaceName));
    }

    private static IHttpClientFactory DefaultHttpClientFactory()
    {
        var serviceProvider = new ServiceCollection();

        serviceProvider
            .AddHttpClient(nameof(SoapClient))
            .ConfigurePrimaryHttpMessageHandler(e =>
                new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
                });

        return serviceProvider.BuildServiceProvider().GetService<IHttpClientFactory>()!;
    }

    #endregion Private Methods
}