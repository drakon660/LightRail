using System.Xml.Linq;

namespace LightRail.Soap;

public interface ISoapClient
{
    Task<HttpResponseMessage> PostAsync(Uri endpoint, SoapVersion soapVersion, IEnumerable<XElement> bodies,
        IEnumerable<XElement> headers = null, string action = null,
        CancellationToken cancellationToken = default);
    
    Task<HttpResponseMessage> PostAsync<TSoapMessage>(TSoapMessage soapMessage, string operationName, string action = null, CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> PostAsync(
        Uri endpoint,
        SoapVersion soapVersion,
        string bodies,
        string headers = null,
        string action = null,
        CancellationToken cancellationToken = default);
}