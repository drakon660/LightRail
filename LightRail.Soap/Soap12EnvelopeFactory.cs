using System.Xml.Linq;

namespace LightRail.Soap;

public class Soap12EnvelopeFactory : ISoapEnvelopeFactory
{
    public StringContent Create(IEnumerable<XElement> headers, IEnumerable<XElement> bodies, string action)
    {
        throw new NotImplementedException();
    }
}