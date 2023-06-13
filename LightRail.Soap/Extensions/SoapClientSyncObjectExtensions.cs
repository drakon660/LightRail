using System.Xml.Linq;

namespace LightRail.Soap;

public static class SoapClientSyncObjectExtensions
{
    public static HttpResponseMessage Post(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        object body,
        object? header = null,
        IXElementSerializer? xElementSerializer = null,
        string? action = null)
    {
        if (xElementSerializer == null)
            xElementSerializer = new XElementSerializer();

        return client.Post(endpoint,
            soapVersion,
            xElementSerializer.Serialize(body),
            header != null ? new[] { xElementSerializer.Serialize(header) } : Enumerable.Empty<XElement>());
    }
    
    public static HttpResponseMessage Post(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        IEnumerable<object> bodies,
        object header,
        IXElementSerializer? xElementSerializer = null,
        string? action = null)
    {
        if (xElementSerializer == null)
            xElementSerializer = new XElementSerializer();

        return client.Post(endpoint,
            soapVersion,
            bodies.Select(e => xElementSerializer.Serialize(e)),
            xElementSerializer.Serialize(header),
            action);
    }
    
    public static HttpResponseMessage Post(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        object body,
        IEnumerable<object> headers,
        IXElementSerializer? xElementSerializer = null,
        string? action = null)
    {
        if (xElementSerializer == null)
            xElementSerializer = new XElementSerializer();

        return client.Post(endpoint,
            soapVersion,
            xElementSerializer.Serialize(body),
            headers.Select(e => xElementSerializer.Serialize(e)),
            action);
    }
}
