using System.Text;
using System.Xml.Linq;

namespace LightRail.Soap;

public class Soap11EnvelopeFactory : ISoapEnvelopeFactory
{
    private const string Schema = "http://schemas.xmlsoap.org/soap/envelope/";
    private const string MediaType = "text/xml";
    private const string SOAPAction = "SOAPAction";
    private static readonly XNamespace XSchema = Schema;

    private const string Envelope =
        "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\" >{0}<soapenv:Body>{1}</soapenv:Body></soapenv:Envelope>";

    
    public StringContent Create(IEnumerable<XElement> headers, IEnumerable<XElement> bodies, string action)
    {
        XElement envelope = new
            XElement(
                XSchema + "Envelope",
                new XAttribute(
                    XNamespace.Xmlns + "soapenv",
                    XSchema.NamespaceName));
        
        
        if (headers != null && headers.Any())
            envelope.Add(new XElement(XSchema + "Header", headers));

        envelope.Add(new XElement(XSchema + "Body", bodies));

        var content = new StringContent(envelope.ToString(), Encoding.UTF8, MediaType);
        
        if (action != null)
            content.Headers.Add(SOAPAction, action);

        return content;
    }
    
    public StringContent Create(string headers, string bodies, string action)
    {
        if (string.IsNullOrEmpty(headers))
            headers = "<soapenv:Header/>";
        
        string envelope = string.Format(Envelope, headers, bodies);
        
        var content = new StringContent(envelope, Encoding.UTF8, MediaType);
        
        if (action != null)
            content.Headers.Add(SOAPAction, action);

        return content;
    }
}