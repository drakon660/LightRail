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

    public string FindAttribute(params string[] names)
    {
        foreach (var name in names)
        {
            var value = FindAttribute(name);
            if (!string.IsNullOrEmpty(value))
                return value;
        }

        return string.Empty;
    }

    public string FindNameAttribute() => FindAttribute("name");
    public string FindAttributeValue(string value) => Attributes.SingleOrDefault(x=>x.Value == value).Key ?? string.Empty;
    public string FindTypeAttribute() => FindAttribute("type");
    public bool FindNillableAttribute() => FindAttribute<bool>("nillable");
    public bool Is(string name) => LocalName == name;
    
    private static T ConvertStringTo<T>(string input)
    {
        return (T)Convert.ChangeType(input, typeof(T));
    }

    private T FindAttribute<T>(string name) => Attributes.TryGetValue(name, out var value) ? ConvertStringTo<T>(value) : default;
}