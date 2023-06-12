namespace LightRail.Wsdl;

public class OperationParam
{
    public string Name { get; }

    private readonly List<OperationParam> _children;
    public IReadOnlyList<OperationParam> Children => _children;
    
    public string Type { get; }

    private OperationParam(string name, string type)
    {
        Name = name;
        Type = type;

        _children = new();
    }

    private OperationParam(string name, string type, List<OperationParam> children) : this(name,type)
    {
        _children = children;
    }

    public void AddChild(OperationParam child) => _children.Add(child);
    
    public static OperationParam Create(string name, string type, List<OperationParam> children) =>
        new OperationParam(name, type, children);
    
    public static OperationParam Create(string name, string type) =>
        new OperationParam(name, type);
}