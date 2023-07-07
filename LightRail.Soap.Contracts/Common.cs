using System.Xml.Linq;
using System.Xml.Serialization;
using LightRail.Reflection;

namespace LightRail.Soap.Contracts;

public static class Common
{
    public static XElement ProduceSoap()
    {
        XNamespace tempuri = "http://tempuri.org/";
        
        var attributes =
            ReflectionUtils.GetCustomAttributes<SoapAttributeAttribute>(typeof(Soap.Contracts.SoapMessage));
        var attr = attributes.ToDictionary(x => x.Key, y => (Name:y.Value.AttributeName, y.Value.Namespace));
        
        SoapEnvelopeBuilder envelopeBuilder = new(attr);
        
        var envelope = envelopeBuilder.GetEnvelope(tempuri.ToString(), "GetValues", new SoapMessage
        {
            Input = new Input { Id = 1, Query = "dupa" },
            ComplexInput = new ComplexInput { Id = 1 }
        });

        return envelope;
    }
}