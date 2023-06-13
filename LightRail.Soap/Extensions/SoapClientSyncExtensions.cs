using System.Xml.Linq;

namespace LightRail.Soap;

public static class SoapClientSyncExtensions
{
    public static HttpResponseMessage Post(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        XElement body,
        XElement? header = null,
        string? action = null)
            => ResolveTask(() =>
                client.PostAsync(endpoint, soapVersion, body, header, action));

    
    public static HttpResponseMessage Post(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        IEnumerable<XElement> bodies,
        XElement header,
        string? action = null)
            => ResolveTask(() =>
                client.PostAsync(endpoint, soapVersion, bodies, header, action));
    
    public static HttpResponseMessage Post(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        XElement body,
        IEnumerable<XElement> headers,
        string? action = null)
            => ResolveTask(() =>
                client.PostAsync(endpoint, soapVersion, body, headers, action));

    #region Private Methods

    private static HttpResponseMessage ResolveTask(Func<Task<HttpResponseMessage>> fn)
        => Task.Run(fn).Result;

    #endregion Private Methods
}
