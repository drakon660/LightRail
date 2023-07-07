using System.Xml.Linq;
using System.Xml.Serialization;
using FastMember;
using LightRail.Reflection;
using Member = FastMember.Member;

namespace LightRail.Soap;

public class SoapEnvelopeBuilder
{
    private readonly IReadOnlyDictionary<string, (string Name, string Namespace)> _soapAttributes;
    private const string SoapSchema = "http://schemas.xmlsoap.org/soap/envelope/";
    private static XNamespace XSoapSchema => SoapSchema;
    private XNamespace _xOperationSchema;

    private HashSet<string> Namespaces => _soapAttributes.Where(x => x.Value.Namespace != null)
        .Select(x => x.Value.Namespace).ToHashSet();

    private XElement _body;

    public SoapEnvelopeBuilder(IReadOnlyDictionary<string, (string Name, string Namespace)> soapAttributes)
    {
        _soapAttributes = soapAttributes;
    }

    //TODO refactor
    public string GetPrefix(string url)
    {
        ReadOnlySpan<char> urlSpan = url.AsSpan();
        ReadOnlySpan<char> letters;

        if (urlSpan.StartsWith("https://"))
        {
            letters = urlSpan.Slice("https://".Length, 3);
        }
        else if (urlSpan.StartsWith("http://"))
        {
            letters = urlSpan.Slice("http://".Length, 3);
        }
        else
        {
            throw new ArgumentException("Invalid URL format.");
        }

        return letters.ToString();
    }

    //TODO refactor
    public string GetSuffix(string url)
    {
        //todo last segment is empty /  in tempuri
        var segments = new Uri(url).Segments;

        if (segments.Length == 1)
        {
            return GetPrefix(url);
        }
        
        var lastSegment = segments.Last();
        return lastSegment.AsSpan().Slice(0, 3).ToString().ToLower();
    }

    protected XElement BuildBody<TSoapMessage>(string operationName, TSoapMessage message)
    {
        XElement operation = new XElement(_xOperationSchema + operationName);

        var accessor = TypeAccessor.Create(typeof(TSoapMessage));

        List<XElement> xElements = new();

        foreach (var member in accessor.GetMembers())
        {
            var value = accessor[message, member.Name];
            if (value is null)
                continue;

            string name = member.Name;

            // char[] charArray = name.ToCharArray();
            // Span<char> charSpan = charArray;
            // charSpan[0] = char.ToLower(charSpan[0]);
            // name = new string(charArray);
            
            string key = $"{typeof(TSoapMessage)}_{name}";
            
            if (_soapAttributes.TryGetValue(key, out var nameOrNamespace))
                name = nameOrNamespace.Name;

            var element = GetValue(name, member.Type, value, _xOperationSchema);
            xElements.Add(element);
        }

        operation.Add(xElements);

        return operation;
    }

    public XElement GetEnvelope<TSoapMessage>(XNamespace operationSchema, string operationName, TSoapMessage message)
    {
        _xOperationSchema = operationSchema;

        _body = new XElement((XNamespace)SoapSchema + "Body");

        XElement operation = BuildBody(operationName, message);

        var envelope = new XElement(XSoapSchema + "Envelope", new XAttribute(
            XNamespace.Xmlns + "soapenv", XSoapSchema.NamespaceName));

        envelope.Add(new XAttribute(XNamespace.Xmlns + GetSuffix(_xOperationSchema.NamespaceName),
            _xOperationSchema.NamespaceName));
        
        foreach (var namespacesWithPrefix in Namespaces)
        {
            XNamespace additionalNamespace = namespacesWithPrefix;
            envelope.Add(new XAttribute(XNamespace.Xmlns + GetSuffix(additionalNamespace.NamespaceName),
                additionalNamespace.NamespaceName));
        }

        _body.Add(operation);

        envelope.Add(new XElement((XNamespace)SoapSchema + "Header"));
        envelope.Add(_body);

        return envelope;
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

            if (_soapAttributes.TryGetValue(key, out var nameOrNamespace))
                xNamespace = nameOrNamespace.Namespace;

            var childElement = GetValue(childName, member.Type, accessor[obj, member.Name],
                xNamespace);

            element.Add(childElement);
        }

        return element;
    }
}