using System.Xml.Linq;
using FastMember;
using LightRail.Reflection;

namespace LightRail.Soap;

public class XElementSoapBuilder
{
    public (XElement Body, IEnumerable<XElement> Header) Initialize<TMessage>(TMessage message)
    {
        if (message is ISoapMessage soapMessage)
        {
            var ns = XNamespace.Get(soapMessage.Namespace);

            var methodBody = new XElement(ns.GetName(soapMessage.OperationName));

            var accessor = TypeAccessor.Create(typeof(TMessage));

            List<XElement> xElements = new List<XElement>();
            
            foreach (var member in accessor.GetMembers())
            {
                var element = new XElement(member.Name);

                if (ReflectionUtils.IsSimpleType(member.Type))
                {
                    var value = accessor[message, member.Name];
                    element.Value = value.ToString();
                }
                
                xElements.Add(element);
            }

            List<XElement> values = new List<XElement>()
            {
                new (ns.GetName("input"),new []
                {
                    new XElement("Id",1),
                    new XElement("Query","test"),
                }),
                new (ns.GetName("complexInput"),new []
                {
                    new XElement("Id",1),
                    new XElement("Query",new []
                    {
                        new XElement("From",1),
                        new XElement("Size",12)
                    }),
                }),
            };
        
            methodBody.Add(values);

            return (methodBody, null);
        }

        return (null, null);
    }

    // public XElement GetValue(string name, Type type, object obj)
    // {
    //     var element = new XElement(name);
    //
    //     if (ReflectionUtils.ISimpleType(type))
    //     {
    //         element.Value = obj.ToString();
    //     }
    // }
}