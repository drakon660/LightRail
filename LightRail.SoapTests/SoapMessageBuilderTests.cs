using System.Xml.Linq;
using FluentAssertions;
using LightRail.Soap;

namespace LightRail.SoapTests;

public class SoapMessageBuilderTests
{
    [Fact]
    public void Test_Serializer()
    {
        string n = "http://tempuri.org/";
        string operation = "GetValues";
        string action = "http://tempuri.org/INothingInputService/GetValues";
        
        SoapEnvelopeBuilder xmlSerializer = new SoapEnvelopeBuilder();

        var message = new SoapMessage();
        message.Input = new Input(12, "dupa");
        message.ComplexInput = new ComplexInput(32, new Query(3, 6));

        var result = xmlSerializer.Initialize(n,operation,message);
    }

    [Fact]
    public void Test_XElement_Prefix()
    {
         string Schema = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace XSchema = Schema;
            XNamespace gml = "http://tempuri.org/";
            XNamespace tns = "http://schemas.datacontract.org/2004/07/Interstate.SoapTestService";
            
            XElement envelope = new
            XElement(
                XSchema + "Envelope",
                new XAttribute(
                    XNamespace.Xmlns + "soapenv",
                    XSchema.NamespaceName), new XAttribute(
                    XNamespace.Xmlns + "tem",
                    gml.NamespaceName), new XAttribute(XNamespace.Xmlns + "tns",tns.NamespaceName));
        
        XElement root = new XElement(gml + "GetValues", new XAttribute(XNamespace.Xmlns + "tem", gml.NamespaceName));

        root.Add(new XElement(tns + "coord","some content" ), new XAttribute(XNamespace.Xmlns + "tns", tns.NamespaceName));
       
    }
    [Fact]
    public void Test_SoapMessageBuildr_Create_Soap_Envelope()
    {
        // var tempUri = "http://tempuri.org/";
        //
        // XElementSoapBuilder xElementSoapBuilder = new();
        // var envelope = xElementSoapBuilder.CreateEnvelope(tempUri,new Dictionary<string, string>()
        // {
        //     {"http://schemas.datacontract.org/2004/07/Interstate.SoapTestService","tns"}
        // });
        //
        // XNamespace gml = "http://tempuri.org/";
        // XNamespace tns = "http://schemas.datacontract.org/2004/07/Interstate.SoapTestService";
        //
        // XElement operationName = new XElement(gml + "GetValues");
        //
        // envelope.Add(operationName);
        XNamespace tempuri = "http://tempuri.org/";
        SoapEnvelopeBuilder envelopeBuilder = new();
        envelopeBuilder.BuildEnvelope(tempuri.ToString(), new Dictionary<string, string>()
        {
            {"http://schemas.datacontract.org/2004/07/Interstate.SoapTestService","tns"}
        });

        envelopeBuilder.BuildBody("GetValues", new SoapMessage
        {
            Input = new Input(1,"dupa"),
            ComplexInput = new ComplexInput(1, new Query(23,23))
        });
        
        var envelope = envelopeBuilder.GetEnvelope();
        
        envelope.Should().NotBeNull();
    }
}

public record Reponse();

public record SoapMessage : ISoapMessage
{
    public Input Input { get; set; }
    public ComplexInput ComplexInput { get; set; }
}

public record Input(int Id, string Query);
public record ComplexInput(int Id, Query Query);
public record Query(int From, int Size);