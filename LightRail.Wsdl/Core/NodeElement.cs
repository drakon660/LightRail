namespace LightRail.Wsdl.Core;

public class NodeElement
{
    public string Name { get; }
    public string LocalName { get; }
    public IReadOnlyDictionary<string, string> Attributes { get; }

    private NodeElement(string name, string localName, IReadOnlyDictionary<string, string> attributes)
    {
        Name = name;
        LocalName = localName;
        Attributes = attributes;
    }

    public static NodeElement Create(string name, string localName, IReadOnlyDictionary<string, string> attributes) =>
        new (name, localName, attributes);
    
    public string FindAttribute(string name) => Attributes.TryGetValue(name, out var attributeValue) ? attributeValue : string.Empty;
    public string FindNameAttribute() => FindAttribute("name");
    public string FindAttributeValue(string value) => Attributes.SingleOrDefault(x=>x.Value == value).Key ?? string.Empty;
    public bool Is(string name) => LocalName == name;
}