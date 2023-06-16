using System.Xml.Linq;
using FastMember;
using LightRail.Reflection;

namespace LightRail.Soap;

public class SoapEnvelopeBuilder
{
    private const string SoapSchema = "http://schemas.xmlsoap.org/soap/envelope/";
    private static XNamespace XSoapSchema => SoapSchema;
    private XNamespace _xOperationSchema;
    private string _operationPrefix = "tem";

    private XElement _envelope;

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
        XElement operation = new XElement(_xOperationSchema + operationName,
            new XAttribute(XNamespace.Xmlns + _operationPrefix, _xOperationSchema.NamespaceName));
        
        var accessor = TypeAccessor.Create(typeof(TSoapMessage));

        List<XElement> xElements = new List<XElement>();

        foreach (var member in accessor.GetMembers())
        {
            var value = accessor[message, member.Name];

            var element = GetValue(member.Name, member.Type, value);

            xElements.Add(element);
        }

        operation.Add(xElements);

        _envelope.Add(operation);
    }

    public XElement GetEnvelope()
    {
        return _envelope;
    }

    public XElement Initialize<TMessage>(string nameSpace, string operationName,
        TMessage message)
    {
        var ns = XNamespace.Get(nameSpace);

        var methodBody = new XElement(ns.GetName(operationName));
        XAttribute namespaceAttribute = new XAttribute(XNamespace.Xmlns + "tem", ns);
        methodBody.Add(namespaceAttribute);

        var accessor = TypeAccessor.Create(typeof(TMessage));

        List<XElement> xElements = new List<XElement>();

        foreach (var member in accessor.GetMembers())
        {
            var value = accessor[message, member.Name];

            var element = GetValue(member.Name, member.Type, value);
            element.Add(namespaceAttribute);

            xElements.Add(element);
        }

        methodBody.Add(xElements);

        return methodBody;
    }

    public XElement GetValue(string name, Type type, object obj)
    {
        var element = new XElement(name);

        if (ReflectionUtils.IsSimpleType(type))
        {
            element.Value = obj.ToString();
        }
        else
        {
            var accessor = TypeAccessor.Create(type);
            foreach (var member in accessor.GetMembers())
            {
                var childElement = GetValue(member.Name, member.Type, accessor[obj, member.Name]);
                element.Add(childElement);
            }
        }

        return element;
    }
}