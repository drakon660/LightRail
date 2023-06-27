using System.Xml.Linq;
using System.Xml.Serialization;
using FastMember;
using LightRail.Reflection;
using Member = FastMember.Member;

namespace LightRail.Soap;

public class SoapEnvelopeBuilder2
{
    private const string SoapSchema = "http://schemas.xmlsoap.org/soap/envelope/";
    private static XNamespace XSoapSchema => SoapSchema;
    private XNamespace _xOperationSchema;
    private string _operationPrefix = "tem";

    private readonly HashSet<string> _namespaces = new();

    private XElement _envelope;

    private static Func<Member, SoapAttributeAttribute> _getSoapAttribute = (member)
        => (SoapAttributeAttribute)member.GetAttribute(typeof(SoapAttributeAttribute), false);

    public void BuildEnvelope(XNamespace operationSchema)
    {
        _xOperationSchema = operationSchema;

        _envelope = new XElement(XSoapSchema + "Envelope", new XAttribute(
            XNamespace.Xmlns + "soapenv", XSoapSchema.NamespaceName));

        _envelope.Add(new XAttribute(
            XNamespace.Xmlns + _operationPrefix,
            _xOperationSchema.NamespaceName));
    }

    public void BuildHeader(XElement headers = null)
    {
        throw new NotImplementedException("not needed right now");
    }

    protected XElement BuildBody<TSoapMessage>(string operationName, TSoapMessage message)
    {
        XElement operation = new XElement(_xOperationSchema + operationName);

        var accessor = TypeAccessor.Create(typeof(TSoapMessage));

        List<XElement> xElements = new List<XElement>();

        foreach (var member in accessor.GetMembers())
        {
            var value = accessor[message, member.Name];
            if (value is null)
                continue;
        
            // var soapAttribute = _getSoapAttribute(member);
            //
            // string name = !string.IsNullOrEmpty(soapAttribute?.AttributeName)
            //     ? soapAttribute.AttributeName
            //     : member.Name;
        
            var element = GetValue(member.Name, member.Type, value, _xOperationSchema);
        
            if (element is not null)
                xElements.Add(element);
        }

        operation.Add(xElements);


        return operation;
    }

    public XElement GetEnvelope<TSoapMessage>(XNamespace operationSchema, string operationName, TSoapMessage message)
    {
        BuildEnvelope(operationSchema);

        XElement operation = BuildBody(operationName, message);

        // foreach (var namespacesWithPrefix in _namespaces)
        // {
        //     XNamespace additionalNamespace = namespacesWithPrefix;
        //     _envelope.Add(new XAttribute(XNamespace.Xmlns + "tns",
        //         additionalNamespace.NamespaceName));
        // }

        _envelope.Add(operation);

        return _envelope;
    }

    public XElement GetValue(string name, Type type, object obj, XNamespace ns = null)
    {
        if (obj is null)
            return null;

        var element = ns == null ? new XElement(name) : new XElement(ns + name);

        if (ReflectionUtils.IsSimpleType(type))
        {
            element.Value = obj.ToString();
        }
        else
        {
            var accessor = TypeAccessor.Create(type);
            foreach (var member in accessor.GetMembers())
            {
                // var soapAttribute = _getSoapAttribute(member);
                //
                // if (soapAttribute is not null)
                //     _namespaces.Add(soapAttribute.Namespace);

                var childElement = GetValue(member.Name, member.Type, accessor[obj, member.Name],
                   null);

                element.Add(childElement);
            }
        }

        return element;
    }
}