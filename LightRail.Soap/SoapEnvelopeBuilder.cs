using System.Xml.Linq;
using System.Xml.Serialization;
using FastMember;
using LightRail.Reflection;
using Member = FastMember.Member;

namespace LightRail.Soap;

public class SoapEnvelopeBuilder
{
    private const string SoapSchema = "http://schemas.xmlsoap.org/soap/envelope/";
    private static XNamespace XSoapSchema => SoapSchema;
    private XNamespace _xOperationSchema;
    private string _operationPrefix = "tem";

    private XElement _envelope;

    private static Func<Member, SoapAttributeAttribute> GetSoapAttribute = (member)
        => (SoapAttributeAttribute)member.GetAttribute(typeof(SoapAttributeAttribute), false);

    public void BuildEnvelope(string operationSchema, IDictionary<string, string> namespacesWithPrefixes)
    {
        _xOperationSchema = operationSchema;

        _envelope = new XElement(XSoapSchema + "Envelope", new XAttribute(
            XNamespace.Xmlns + "soapenv", XSoapSchema.NamespaceName));

        _envelope.Add(new XAttribute(
            XNamespace.Xmlns + _operationPrefix,
            _xOperationSchema.NamespaceName));

        foreach (var namespacesWithPrefix in namespacesWithPrefixes)
        {
            XNamespace additionalNamespace = namespacesWithPrefix.Key;
            _envelope.Add(new XAttribute(XNamespace.Xmlns + namespacesWithPrefix.Value,
                additionalNamespace.NamespaceName));
        }
    }

    public void BuildHeader(XElement headers = null)
    {
        throw new NotImplementedException("not needed right now");
    }

    public void BuildBody<TSoapMessage>(string operationName, TSoapMessage message)
    {
        XElement operation = new XElement(_xOperationSchema + operationName);
            //new XAttribute(XNamespace.Xmlns + _operationPrefix, _xOperationSchema.NamespaceName));

        var accessor = TypeAccessor.Create(typeof(TSoapMessage));

        List<XElement> xElements = new List<XElement>();

        foreach (var member in accessor.GetMembers())
        {
            var value = accessor[message, member.Name];
            if (value is null)
                continue;

            var soapAttribute = GetSoapAttribute(member);

            string name = !string.IsNullOrEmpty(soapAttribute.AttributeName)
                ? soapAttribute.AttributeName
                : member.Name;

            var element = GetValue(name, member.Type, value, _xOperationSchema);

            if (element is not null)
                xElements.Add(element);
        }

        operation.Add(xElements);

        _envelope.Add(operation);
    }

    public XElement GetEnvelope()
    {
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
                var soapAttribute = GetSoapAttribute(member);

                var childElement = GetValue(member.Name, member.Type, accessor[obj, member.Name],
                    soapAttribute?.Namespace);
                element.Add(childElement);
            }
        }

        return element;
    }
}