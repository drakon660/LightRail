namespace LightRail.Soap;

public interface ISoapMessage
{
    public string Namespace { get; }
    public string OperationName { get; }
    public string Action { get; }
}