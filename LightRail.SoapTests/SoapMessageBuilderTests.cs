using System.Xml.Linq;
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
        
        XElementSoapBuilder xmlSerializer = new XElementSoapBuilder();

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
            
            XElement envelope = new
            XElement(
                XSchema + "Envelope",
                new XAttribute(
                    XNamespace.Xmlns + "soapenv",
                    XSchema.NamespaceName), new XAttribute(
                    XNamespace.Xmlns + "tem",
                    gml.NamespaceName));
        
        XElement root = new XElement(gml + "GetValues", new XAttribute(XNamespace.Xmlns + "tem", gml.NamespaceName));

        root.Add(new XElement(gml + "coord","some content" ));
       
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