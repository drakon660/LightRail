namespace LightRail.Wsdl.Core;

public class Element
{
    public string Name { get; }
    public bool IsNillable { get; }
    public string Type { get; }
    public string TypeWithoutNamespace => Type.Contains(':') ? Type.Split(':').Last() : Type;
    
    public List<Element> Children { get; } = new();
    
    public void AddChild(Element child)
    {
        Children.Add(child);
    }
    public void AddChildren(IEnumerable<Element> children)
    {
        Children.AddRange(children);
    }
    
    public bool IsSimpleType => XsdTypesMapper.IsSimpleType(Type);

    private Element(string name, bool isNillable, string type)
    {
        Name = name;
        IsNillable = isNillable;
        Type = type;
    }

    public bool HasTypeAttribute => !string.IsNullOrEmpty(Type);
    public bool HasName => !string.IsNullOrEmpty(Name);
    
    public static Element Create(string name, bool isNillable, string type) => new (name,isNillable, type);
}