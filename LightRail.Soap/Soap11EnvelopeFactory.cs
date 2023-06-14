using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;

namespace LightRail.Soap;

public class Soap11EnvelopeFactory : ISoapEnvelopeFactory
{
    private readonly SoapMessageConfiguration _soapMessageConfiguration;
    private const string Schema = "http://schemas.xmlsoap.org/soap/envelope/";
    private const string MediaType = "text/xml";

    public Soap11EnvelopeFactory()
    {
        _soapMessageConfiguration = new SoapMessageConfiguration(SoapVersion.Soap11);
    }

    public StringContent Create(IEnumerable<XElement> headers, IEnumerable<XElement> bodies, string action)
    {
        XElement envelope = new
            XElement(
                _soapMessageConfiguration.Schema + "Envelope",
                new XAttribute(
                    XNamespace.Xmlns + "soapenv",
                    _soapMessageConfiguration.Schema.NamespaceName));
        
        
        if (headers != null && headers.Any())
            envelope.Add(new XElement(_soapMessageConfiguration.Schema + "Header", headers));

        envelope.Add(new XElement(_soapMessageConfiguration.Schema + "Body", bodies));

        var content = new StringContent(envelope.ToString(), Encoding.UTF8, MediaType);
        
        if (action != null)
        {
            content.Headers.Add("SOAPAction", action);

            if (_soapMessageConfiguration.SoapVersion == SoapVersion.Soap12)
                content.Headers.ContentType!.Parameters.Add(
                    new NameValueHeaderValue("ActionParameter", $"\"{action}\""));
        }

        return content;
    }
}