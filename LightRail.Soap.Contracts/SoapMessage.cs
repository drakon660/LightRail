using System.Xml.Serialization;

namespace LightRail.Soap.Contracts;

public record SoapMessage
{
    [SoapAttribute(AttributeName = "input")]
    public Input Input { get; set; }

    [SoapAttribute(AttributeName = "complexInput")]
    public ComplexInput ComplexInput { get; set; }
}

public record Input
{
    [SoapAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Interstate.SoapTestService")]
    public int Id { get; init; }

    [SoapAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Interstate.SoapTestService")]
    public string Query { get; init; }
};

public record ComplexInput(int Id, Query Query);

public record Query(int From, int Size);