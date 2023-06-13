using System.Xml.Linq;

namespace LightRail.Soap;

public interface ISoapClient
{
    Task<HttpResponseMessage> PostAsync(Uri endpoint, SoapVersion soapVersion, IEnumerable<XElement> bodies,
        IEnumerable<XElement>? headers = null, string? action = null,
        CancellationToken cancellationToken = default);
}