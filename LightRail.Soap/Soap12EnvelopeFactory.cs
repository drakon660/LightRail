using System.Xml.Linq;

namespace LightRail.Soap;

public class Soap12EnvelopeFactory : ISoapEnvelopeFactory
{
     
     // public StringContent Create(IEnumerable<XElement> headers, IEnumerable<XElement> bodies, string action)
     // {
     //     XElement envelope = new
     //         XElement(
     //             XSchema + "Envelope",
     //             new XAttribute(
     //                 XNamespace.Xmlns + "soapenv",
     //                 XSchema.NamespaceName));
     //     
     //     
     //     if (headers != null && headers.Any())
     //         envelope.Add(new XElement(XSchema + "Header", headers));
     //
     //     envelope.Add(new XElement(XSchema + "Body", bodies));
     //
     //     var content = new StringContent(envelope.ToString(), Encoding.UTF8, MediaType);
     //     
     //     if (action != null)
     //     {
     //         content.Headers.Add("SOAPAction", action);
     //
     //         // if (_soapMessageConfiguration.SoapVersion == SoapVersion.Soap12)
     //         //     content.Headers.ContentType!.Parameters.Add(
     //         //         new NameValueHeaderValue("ActionParameter", $"\"{action}\""));
     //     }
     //
     //     return content;
     // }

     public StringContent Create(IEnumerable<XElement> headers, IEnumerable<XElement> bodies, string action)
     {
          throw new NotImplementedException();
     }

     public StringContent Create(string headers, string bodies, string action)
     {
          throw new NotImplementedException();
     }

     public StringContent Create(XElement envelope, string action)
     {
          throw new NotImplementedException();
     }
}