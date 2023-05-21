using LightRail.Wsdl.Core;

namespace LightRail.Wsdl;

public static class ElementExtensions
{
    public static Element ToXsdElement(this NodeElement element)
    {
        var name = element.FindNameAttribute();
        var type = element.FindTypeAttribute();
        var isNillable = element.FindNillableAttribute();

        return Element.Create(name, isNillable, type);
    }
}