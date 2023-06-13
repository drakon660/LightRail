using System.Xml.Linq;

namespace LightRail.Soap;

public static class SoapClientObjectExtensions
{
    public static Task<HttpResponseMessage> PostAsync(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        object body,
        object? header = null,
        IXElementSerializer? xElementSerializer = null,
        string? action = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (xElementSerializer == null)
            xElementSerializer = new XElementSerializer();

        return client.PostAsync(endpoint,
            soapVersion,
            xElementSerializer.Serialize(body),
            header != null ? new[] { xElementSerializer.Serialize(header) } : Enumerable.Empty<XElement>(),
            action,
            cancellationToken);
    }
    
    public static Task<HttpResponseMessage> PostAsync(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        IEnumerable<object> bodies,
        object header,
        IXElementSerializer? xElementSerializer = null,
        string? action = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (xElementSerializer == null)
            xElementSerializer = new XElementSerializer();

        return client.PostAsync(endpoint,
            soapVersion,
            bodies.Select(e => xElementSerializer.Serialize(e)),
            header != null ? new[] { xElementSerializer.Serialize(header) } : Enumerable.Empty<XElement>(),
            action,
            cancellationToken);
    }
    
    public static Task<HttpResponseMessage> PostAsync(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        object body,
        IEnumerable<object> headers,
        IXElementSerializer? xElementSerializer = null,
        string? action = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (xElementSerializer == null)
            xElementSerializer = new XElementSerializer();

        return client.PostAsync(endpoint,
            soapVersion,
            xElementSerializer.Serialize(body),
            headers.Select(e => xElementSerializer.Serialize(e)),
            action,
            cancellationToken);
    }
}
