using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Ardalis.Result;
using Microsoft.Extensions.DependencyInjection;

namespace LightRail.Soap;

public class SoapClient : ISoapClient
{
    private readonly IHttpClientFactory _httpClientFactory;

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
        CancellationToken cancellationToken = default)
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

        // Execute call
        var httpClient = _httpClientFactory.CreateClient(nameof(SoapClient));
        var response = await httpClient.PostAsync(endpoint, content, cancellationToken);

        //result builder
        return response;
    }

    // public async Task<TResponse> PostAsync<TResponse, TRequestMessage>(string endpoint, SoapVersion soapVersion,
    //     TRequestMessage message, string? action = null, CancellationToken cancellationToken = default)
    // {
    //     var uri = new Uri(endpoint);
    //
    //     var response = await PostAsync(uri, soapVersion, message, action, cancellationToken);
    //
    //     return response;
    // }

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

public class ResultFactory
{
    public static async Task<Result<string>> Create(HttpResponseMessage responseMessage, SoapVersion soapVersion)
    {
        var messageConfiguration = new SoapMessageConfiguration(soapVersion);

        if (responseMessage.Content.Headers.ContentType?.MediaType != messageConfiguration.MediaType)
            return Result.Error("Response is not xml");

        var responseContent = await responseMessage.Content.ReadAsStreamAsync();
        if (!responseMessage.IsSuccessStatusCode)
        {
            var (faultCode, faultString) = await ParseSoapFault(responseContent);
            return Result.Error();
        }

        using StreamReader reader = new StreamReader(responseContent);
            return Result.Success(await reader.ReadToEndAsync());
    }

    public static async Task<(string FaultCode, string FaultString)> ParseSoapFault(Stream responseContentStream,
        CancellationToken cancellationToken = default)
    {
        XElement envelope = await XElement.LoadAsync(responseContentStream, LoadOptions.None, default);

        XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace faultNs = "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher";

        XElement faultCodeElement =
            envelope.Descendants(soapNs + "Fault").Descendants(faultNs + "faultcode").FirstOrDefault();
        XElement faultStringElement =
            envelope.Descendants(soapNs + "Fault").Descendants(faultNs + "faultstring").FirstOrDefault();

        if (faultCodeElement != null && faultStringElement != null)
        {
            string faultCode = faultCodeElement.Value;
            string faultString = faultStringElement.Value;

            return (faultCode, faultString);
        }

        throw new Exception("response xml not recognized");
    }
}

public class MessageInitializer
{
    // public (IEnumerable<XElement> Body, IEnumerable<XElement> Header) Initialize<TMessage>(TMessage message)
    // {
    //     
    // }
}