using System.Xml.Linq;
using FastMember;
using LightRail.Reflection;

namespace LightRail.Soap;

public class XElementSoapBuilder
{
    public XElement CreateEnvelope(string soapSchema, string schema, IDictionary<string,string> namespacesWithPrefixes)
    {
        XNamespace XSoapSchema = soapSchema;
        XNamespace XSchema = schema;
        
        XElement envelope = new
            XElement(
                XSoapSchema + "Envelope",
                new XAttribute(
                    XNamespace.Xmlns + "soapenv",
                    XSoapSchema.NamespaceName), new XAttribute(
                    XNamespace.Xmlns + "tem",
                    XSchema.NamespaceName));
        
        //namespacesWithPrefixes.Select(x=> new XAttribute(XNamespace.Xmlns + x.Value,))
        
        envelope.Add();
        
        return envelope;
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