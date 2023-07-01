using System.Xml.Linq;
using System.Xml.Serialization;
using FastMember;
using LightRail.Reflection;
using Member = FastMember.Member;

namespace LightRail.Soap;

public class SoapEnvelopeBuilder2
{
    private readonly IReadOnlyDictionary<string, SoapAttributeAttribute> _soapAttributes;
    private const string SoapSchema = "http://schemas.xmlsoap.org/soap/envelope/";
    private static XNamespace XSoapSchema => SoapSchema;
    private XNamespace _xOperationSchema;
    private string _operationPrefix = "tem";

    private HashSet<string> Namespaces =>
        _soapAttributes?.Where(x =>x.Value.Namespace != null).Select(x => x.Value.Namespace).ToHashSet();

    private XElement _envelope;

    public SoapEnvelopeBuilder2(IReadOnlyDictionary<string, SoapAttributeAttribute> soapAttributes)
    {
        _soapAttributes = soapAttributes;
    }

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

            string name = member.Name;

            string key = $"{typeof(TSoapMessage).DeclaringType}_{member.Name}";

            if (_soapAttributes.TryGetValue(key, out var soapAttribute))
            {
                name = !string.IsNullOrEmpty(soapAttribute.AttributeName)
                    ? soapAttribute.AttributeName
                    : member.Name;
            }

            var element = GetValue(name, member.Type, value, _xOperationSchema);

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

        foreach (var namespacesWithPrefix in Namespaces)
        {
            XNamespace additionalNamespace = namespacesWithPrefix;
            _envelope.Add(new XAttribute(XNamespace.Xmlns + "tns",
                additionalNamespace.NamespaceName));
        }

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
            return element;
        }

        var accessor = TypeAccessor.Create(type);
        foreach (var member in accessor.GetMembers())
        {
            var childName = member.Name;

            string key = $"{type}_{member.Name}";

            XNamespace xNamespace = null;
            
            if (_soapAttributes.TryGetValue(key, out var soapAttribute))
            {
                childName = !string.IsNullOrEmpty(soapAttribute.AttributeName)
                    ? soapAttribute.AttributeName
                    : member.Name;

                xNamespace = soapAttribute.Namespace;
            }

            var childElement = GetValue(childName, member.Type, accessor[obj, member.Name],
                xNamespace);

            element.Add(childElement);
        }

        return element;
    }
}