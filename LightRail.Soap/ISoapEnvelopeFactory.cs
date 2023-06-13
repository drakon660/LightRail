using System.Xml.Linq;

namespace LightRail.Soap;

public interface ISoapEnvelopeFactory
{
    StringContent Create(IEnumerable<XElement> headers, IEnumerable<XElement> bodies, string action);
}