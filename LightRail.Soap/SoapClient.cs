using System.Net;
using System.Xml.Linq;
using Ardalis.Result;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LightRail.Soap;

public class SoapClient : ISoapClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SoapEnvelopeBuilder _soapEnvelopeBuilder;

    private readonly XNamespace _namespace;
    private readonly Uri _endpoint;
    
    public SoapClient(string @namespace)
    {
        _namespace = @namespace;

        _httpClientFactory = DefaultHttpClientFactory();
    }
    
    public SoapClient(string @namespace, string endpoint)
    {
        _namespace = @namespace;
        _endpoint = new Uri(endpoint);

        _httpClientFactory = DefaultHttpClientFactory();
    }
    
    public SoapClient(string @namespace, string endpoint, IHttpClientFactory httpClientFactory)
    {
        _namespace = @namespace;
        _endpoint = new Uri(endpoint);
        
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }
    
    public SoapClient(string @namespace, string endpoint, SoapEnvelopeBuilder soapEnvelopeBuilder, IHttpClientFactory httpClientFactory)
    {
        _namespace = @namespace;
        _endpoint = new Uri(endpoint);
        
        _soapEnvelopeBuilder = soapEnvelopeBuilder ?? throw new ArgumentNullException(nameof(soapEnvelopeBuilder));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }
    
    public SoapClient(IOptions<SoapClientOptions> options, SoapEnvelopeBuilder soapEnvelopeBuilder, IHttpClientFactory httpClientFactory)
    {
        _namespace = options.Value.Namespace;
        _endpoint = new Uri(options.Value.Endpoint);
        
        _soapEnvelopeBuilder = soapEnvelopeBuilder ?? throw new ArgumentNullException(nameof(soapEnvelopeBuilder));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }
    
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

    public async Task<HttpResponseMessage> PostAsync<TSoapMessage>(TSoapMessage soapMessage, string operationName, string action = null,
        CancellationToken cancellationToken = default)
    {
        var envelope = _soapEnvelopeBuilder.GetEnvelope(_namespace, operationName, soapMessage);

        return await PostAsync(_endpoint, envelope, action, cancellationToken);
    }

    public async Task<HttpResponseMessage> PostAsync(
        Uri endpoint,
        XElement envelope,
        string action = null,
        CancellationToken cancellationToken = default)
    {
        if (endpoint == null)
            throw new ArgumentNullException(nameof(endpoint));

        if (envelope == null)
            throw new ArgumentNullException(nameof(envelope));
        
        ISoapEnvelopeFactory soapFactory = EnvelopeFactories.Get(soapVersion:SoapVersion.Soap11);

        var content = soapFactory.Create(envelope, action);
        
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

    public async Task<HttpResponseMessage> PostAsync<T>(
        Uri endpoint,
        SoapVersion soapVersion,
        T message,
        string operationName,
        string action = null,
        CancellationToken cancellationToken = default) where T : class
    {
        XNamespace tempuri = "http://tempuri.org/";

        var envelope = _soapEnvelopeBuilder.GetEnvelope(tempuri, operationName, message);

        return await PostAsync(endpoint, envelope, action, cancellationToken);
    }

    public static IHttpClientFactory DefaultHttpClientFactory()
    {
        var serviceProvider = new ServiceCollection();

        serviceProvider
            .AddHttpClient(nameof(SoapClient))
            .ConfigurePrimaryHttpMessageHandler(e =>
                new SocketsHttpHandler()
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