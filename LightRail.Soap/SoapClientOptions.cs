namespace LightRail.Soap;

public record SoapClientOptions
{
    public string Namespace { get; init; }
    public string Endpoint { get; init; }
}