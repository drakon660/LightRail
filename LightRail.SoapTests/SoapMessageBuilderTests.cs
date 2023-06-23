using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using FluentAssertions;
using LightRail.Soap;

namespace LightRail.SoapTests;

public class SoapMessageBuilderTests
{
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
                    gml.NamespaceName), new XAttribute(XNamespace.Xmlns + "tns", tns.NamespaceName));


        XElement operation =
            new XElement(gml + "GetValues", new XAttribute(XNamespace.Xmlns + "tem", gml.NamespaceName));

        var input1 = new XElement(gml + "coord", "some content");

        var input1_1 = new XElement(tns + "input1_1", "ciul");
        input1.Add(input1_1);

        var input2 = new XElement(gml + "duport", "some content");

        operation.Add(input1, input2);

        envelope.Add(operation);
    }

    [Fact]
    public void Test_SoapMessageBuildr_Create_Soap_Envelope()
    {
        XNamespace tempuri = "http://tempuri.org/";
        SoapEnvelopeBuilder envelopeBuilder = new();
        envelopeBuilder.BuildEnvelope(tempuri.ToString(), new Dictionary<string, string>()
        {
            { "http://schemas.datacontract.org/2004/07/Interstate.SoapTestService", "tns" }
        });

        envelopeBuilder.BuildBody("GetValues", new SoapMessage
        {
            Input = new Input { Id = 1, Query = "dupa" },
            //ComplexInput = new ComplexInput(1, new Query(23,23))
        });

        var envelope = envelopeBuilder.GetEnvelope();

        string expectedSoap = """ 
                                  
                                   <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/" xmlns:tns="http://schemas.datacontract.org/2004/07/Interstate.SoapTestService">
                                  <tem:GetValues>
                                  <tem:input>
                                    <tns:Id>1</tns:Id>
                  
                                    <tns:Query>dupa</tns:Query>
                
                                    </tem:input>
          
                                    </tem:GetValues>
    
                                    </soapenv:Envelope>        
                                 """;

        string result = RemoveWhitespace(envelope.ToString());
        expectedSoap = RemoveWhitespace(expectedSoap);
        result.Should().BeEquivalentTo(expectedSoap);
        envelope.Should().NotBeNull();
    }
    
    [Fact]
    public void Test_SoapMessageBuildr_Create_Soap_Envelope_Better()
    {
        XNamespace tempuri = "http://tempuri.org/";
        SoapEnvelopeBuilder2 envelopeBuilder = new();
        
        var envelope = envelopeBuilder.GetEnvelope(tempuri.ToString(), "GetValues", new SoapMessage
        {
            Input = new Input { Id = 1, Query = "dupa" },
            //ComplexInput = new ComplexInput(1, new Query(23,23))
        });

        string expectedSoap = """                                   
                                   <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/" xmlns:tns="http://schemas.datacontract.org/2004/07/Interstate.SoapTestService">
                                  <tem:GetValues>
                                  <tem:input>
                                    <tns:Id>1</tns:Id>
                  
                                    <tns:Query>dupa</tns:Query>
                
                                    </tem:input>
          
                                    </tem:GetValues>
    
                                    </soapenv:Envelope>        
                                 """;

        string result = RemoveWhitespace(envelope.ToString());
        expectedSoap = RemoveWhitespace(expectedSoap);
        result.Should().BeEquivalentTo(expectedSoap);
        envelope.Should().NotBeNull();
    }
   
    public static string RemoveWhitespace(string input)
    {
        return Regex.Replace(input, @"\s+", string.Empty);
    }
}

public record Reponse();

public record SoapMessage : ISoapMessage
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