using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using FluentAssertions;
using LightRail.Reflection;
using LightRail.Soap;
using LightRail.Soap.Contracts;

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
    public void Test_ReflectionUtils_GetCustomAttribute()
    {
        //todo dodac rekurencyjne zbieranie attrybutow
        var attributes =
            ReflectionUtils.GetCustomAttributes<SoapAttributeAttribute>(typeof(Soap.Contracts.SoapMessage));

        attributes.Count.Should().Be(5);
    }
    
    [Fact]
    public void Test_SoapMessageBuildr_Create_Soap_Envelope_Better()
    {
        XNamespace tempuri = "http://tempuri.org/";
        
        //todo dodac rekurencyjne zbieranie attrybutow
        var attributes =
            ReflectionUtils.GetCustomAttributes<SoapAttributeAttribute>(typeof(Soap.Contracts.SoapMessage));
        
        SoapEnvelopeBuilder2 envelopeBuilder = new(attributes);
        
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

    [Fact]
    public void RawXElment_Soap()
    {
        XNamespace SoapSchema = "http://schemas.xmlsoap.org/soap/envelope/";
        
        var ns = XNamespace.Get("http://tempuri.org/");
        
        var envelope = new XElement(SoapSchema + "Envelope", new XAttribute(
            XNamespace.Xmlns + "soapenv", SoapSchema.NamespaceName));

        envelope.Add(new XAttribute(
            XNamespace.Xmlns + "tns",
            ns.NamespaceName));
        
        var methodBody = new XElement(ns.GetName("GetValues"));

        List<XElement> values = new List<XElement>()
        {
            new (ns.GetName("input"), new [] 
            {
                new XElement("Id",1),
                new XElement("Query","test"),
            }),
            new (ns.GetName("complexInput"), new []
            {
                new XElement("Id",1),
                new XElement("Query", new []
                {
                    new XElement("From",1),
                    new XElement("Size",12)
                }),
            }),
        };
        
        methodBody.Add(values);
        envelope.Add(methodBody);
    }
}

