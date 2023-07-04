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
    public void Full_XElement()
    {
        XNamespace SoapSchema = "http://schemas.xmlsoap.org/soap/envelope/";
        
        var ns = XNamespace.Get("http://tempuri.org/");
        var tem = XNamespace.Get("http://schemas.datacontract.org/2004/07/Interstate.SoapTestService");
        
        var envelope = new XElement(SoapSchema + "Envelope", new XAttribute(
            XNamespace.Xmlns + "soapenv", SoapSchema.NamespaceName));

        envelope.Add(new XAttribute(
            XNamespace.Xmlns + "tns",
            ns.NamespaceName));
        
        envelope.Add(new XAttribute(
            XNamespace.Xmlns + "tem",
            tem.NamespaceName));
        
        var methodBody = new XElement(ns.GetName("GetValues"));

        List<XElement> values = new List<XElement>()
        {
            new (ns.GetName("input"), new [] 
            {
                new XElement(tem.GetName("Id"),1),
                new XElement(tem.GetName("Query"),"test"),
            }),
            new (ns.GetName("complexInput"), new []
            {
                new XElement(tem.GetName("Id"),1),
                new XElement(tem.GetName("Query"), new []
                {
                    new XElement(tem.GetName("From"),1),
                    new XElement(tem.GetName("Size"),12)
                }),
            }),
        };
        var body = new XElement((XNamespace)SoapSchema + "Body");
        var header = new XElement((XNamespace)SoapSchema + "Header");
        methodBody.Add(values);
        body.Add(methodBody);
        envelope.Add(header);
        envelope.Add(body);

        string env = envelope.ToString();
    }
    
    [Fact]
    public void Test_ReflectionUtils_GetCustomAttribute()
    {
        var attributes =
            ReflectionUtils.GetCustomAttributes<SoapAttributeAttribute>(typeof(Soap.Contracts.SoapMessage));

        attributes.Count.Should().Be(5);
    }
    
    [Fact]
    public void Test_SoapMessageBuildr_Create_Soap_Envelope_Better()
    {
        XNamespace tempuri = "http://tempuri.org/";
        
        var attributes =
            ReflectionUtils.GetCustomAttributes<SoapAttributeAttribute>(typeof(Soap.Contracts.SoapMessage));
        var attr = attributes.ToDictionary(x => x.Key, y => (Name:y.Value.AttributeName, y.Value.Namespace));
        SoapEnvelopeBuilder envelopeBuilder = new(attr);
        
        var envelope = envelopeBuilder.GetEnvelope(tempuri.ToString(), "GetValues", new SoapMessage
        {
            Input = new Input { Id = 1, Query = "dupa" },
            //ComplexInput = new ComplexInput{Id = 1}
        });

        string expectedSoap = """                                   
                                   <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/" xmlns:Int="http://schemas.datacontract.org/2004/07/Interstate.SoapTestService">
                                    <soapenv:Header/>
                                     <soapenv:Body>

                                  <tem:GetValues>
                                  <tem:input>
                                    <Int:Id>1</Int:Id>
                  
                                    <Int:Query>dupa</Int:Query>
                
                                    </tem:input>
          
                                    </tem:GetValues>
                                     </soapenv:Body>

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
    public void RawXElement_Soap()
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

