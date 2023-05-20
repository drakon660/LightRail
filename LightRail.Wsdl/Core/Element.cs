namespace LightRail.Wsdl.Core;

public class Element
{
    public string Name { get; }
    public bool IsNillable { get; }
    public string Type { get; }

    private Element(string name, bool isNillable, string type)
    {
        Name = name;
        IsNillable = isNillable;
        Type = type;
    }
    
    public static Element Create(string name, bool isNillable, string type) => new Element(name,isNillable, type);
}