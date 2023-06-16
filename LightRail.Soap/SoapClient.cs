using System.Net;
using System.Reflection;
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
    
    public async Task<HttpResponseMessage> PostAsync(
        Uri endpoint,
        SoapVersion soapVersion,
        IEnumerable<XElement> bodies,
        IEnumerable<XElement> headers = null,
        string action = null,
        CancellationToken cancellationToken = default)
    {
        if (endpoint == null)
            throw new ArgumentNullException(nameof(endpoint));

        if (bodies == null)
            throw new ArgumentNullException(nameof(bodies));

        if (!bodies.Any())
            throw new ArgumentException("Bodies element cannot be empty", nameof(bodies));

        ISoapEnvelopeFactory soapFactory = EnvelopeFactories.Get(soapVersion);

        var content = soapFactory.Create(headers, bodies, action);
        
        // Execute call
        var httpClient = _httpClientFactory.CreateClient(nameof(SoapClient));
        var response = await httpClient.PostAsync(endpoint, content, cancellationToken);
        
        //result builder
        return response;
    }
    
    public async Task<HttpResponseMessage> PostAsync(
        Uri endpoint,
        SoapVersion soapVersion,
        string bodies,
        string headers = null,
        string action = null,
        CancellationToken cancellationToken = default)
    {
        if (endpoint == null)
            throw new ArgumentNullException(nameof(endpoint));

        if (bodies == null)
            throw new ArgumentNullException(nameof(bodies));

        if (!bodies.Any())
            throw new ArgumentException("Bodies element cannot be empty", nameof(bodies));

        ISoapEnvelopeFactory soapFactory = EnvelopeFactories.Get(soapVersion);

        var content = soapFactory.Create(headers, bodies, action);
        
        // Execute call
        var httpClient = _httpClientFactory.CreateClient(nameof(SoapClient));
        var response = await httpClient.PostAsync(endpoint, content, cancellationToken);
        
        //result builder
        return response;
    }

    // public async Task<HttpResponseMessage> PostAsync<T>(
    //     Uri endpoint,
    //     SoapVersion soapVersion,
    //     T message,
    //     string action = null,
    //     CancellationToken cancellationToken = default) where T: ISoapMessage
    // {
    //
    //         
    // }

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