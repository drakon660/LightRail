using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace LightRail.Soap;

public static class SoapClientExtensions
{
    public static Task<HttpResponseMessage> PostAsync(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        XElement body,
        XElement? header = null,
        string? action = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.PostAsync(
            endpoint,
            soapVersion,
            new[] { body },
            header != null ? new[] { header } : default(IEnumerable<XElement>),
            action,
            cancellationToken);
    }
    
    public static Task<HttpResponseMessage> PostAsync(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        IEnumerable<XElement> bodies,
        XElement header,
        string? action = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.PostAsync(
            endpoint,
            soapVersion,
            bodies,
            new[] { header },
            action,
            cancellationToken);
    }
    public static Task<HttpResponseMessage> PostAsync(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        XElement body,
        IEnumerable<XElement> headers,
        string? action = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.PostAsync(
            endpoint,
            soapVersion,
            new[] { body },
            headers,
            action,
            cancellationToken);
    }


    public static IServiceCollection AddSoapClient(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<SoapEnvelopeBuilder2>();
        
        return serviceCollection;
    }
}
