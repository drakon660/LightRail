using System.Collections.ObjectModel;
using FluentAssertions;
using LightRail.Wsdl;
using LightRail.Wsdl.Core;

namespace LightRail.WsdlTests;

public class WsdleReaderTests
{
    [Fact]
    public void Check_If_WsdlReader_Is_Reading_Tree()
    {
        var sampleXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<note>
  <to>Tove</to>
  <from>Jani</from>
  <heading>Reminder</heading>
  <body>Don't forget me this weekend!</body>
</note>";

        var wsdlReader = new WsdlReader();
        wsdlReader.Read(sampleXml);

        wsdlReader.Root.Should().NotBeNull();
    }

    [Fact]
    public void Check_If_Wsdl_Tree_Has_Values()
    {
        var sampleXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<note>
  <to id=""15"">Tove</to>
  <from>Jani</from>
  <heading>Reminder</heading>
  <body>Don't forget me this weekend!</body>
</note>";

        var wsdlReader = new WsdlReader();
        wsdlReader.Read(sampleXml);

        wsdlReader.Root.Data.Should().NotBeNull();
        wsdlReader.Root.Data.Should()
            .BeEquivalentTo(NodeElement.Create("note", "note", new Dictionary<string, string>()));

        wsdlReader.Root.FirstChild.Data.Should()
            .BeEquivalentTo(NodeElement.Create("to", "to", new Dictionary<string, string>
            {
                { "id", "15" }
            }));

        wsdlReader.Root.HasMultipleChildren.Should().BeTrue();
        wsdlReader.Root.Children.Count.Should().Be(4);
    }

    [Fact]
    public void Check_If_WsdlReader_Is_Resolving_Soap_Operation_Namespace()
    {
        var sampleXml = """
                            <wsdl:definitions xmlns:wsdl="http://schemas.xmlsoap.org/wsdl/" xmlns:soap="http://schemas.xmlsoap.org/wsdl/soap12/"
                            xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:tds="http://www.onvif.org/ver10/device/wsdl" targetNamespace="http://www.onvif.org/ver10/device/wsdl"></wsdl:definitions>
                           """;

        var wsdlReader = new WsdlReader();
        wsdlReader.Read(sampleXml);
        wsdlReader.ResolveAll();

        wsdlReader.TargetNamespace.Should().Be("http://www.onvif.org/ver10/device/wsdl");
        wsdlReader.SoapActionNamespace.Should().Be("tds");
        wsdlReader.ServiceName.Should().BeEmpty();
    }

    [Fact]
    public void Check_If_WsdlReader_Is_Resolving_Wsdl_Message()
    {
        var wsdlReader = new WsdlReader();
        using var file = File.OpenRead("wcf.xsd");
        wsdlReader.Read(file);
        wsdlReader.ResolveAll();

        wsdlReader.TargetNamespace.Should().Be("http://tempuri.org/");
        wsdlReader.SoapActionNamespace.Should().Be("tns");
        wsdlReader.ServiceName.Should().Be("NothingService");
        wsdlReader.Messages.Count.Should().Be(4);
        wsdlReader.Messages.Should().BeEquivalentTo(new Dictionary<string, IReadOnlyList<Part>>
        {
            {
                "INothingService_GetNothingValues_InputMessage", new Collection<Part>
                {
                    new("parameters", "tns:GetNothingValues")
                }
            },
            {
                "INothingService_GetNothingValues_OutputMessage", new Collection<Part>
                {
                    new("parameters", "tns:GetNothingValuesResponse")
                }
            },
            {
                "INothingService_GetNothingWithQuery_InputMessage", new Collection<Part>
                {
                    new("parameters", "tns:GetNothingWithQuery")
                }
            },
            {
                "INothingService_GetNothingWithQuery_OutputMessage", new Collection<Part>
                {
                    new("parameters", "tns:GetNothingWithQueryResponse")
                }
            }
        });
    }

    [Fact]
    public void Check_If_WsdlReader_Is_Resolving_Operations_From_PortType()
    {
        var wsdlReader = new WsdlReader();
        using var file = File.OpenRead("wcf.xsd");
        wsdlReader.Read(file);
        wsdlReader.ResolveAll();

        var getNothingValues = Operation.Create("GetNothingValues");
        getNothingValues.SetInput("http://tempuri.org/INothingService/GetNothingValues","tns:INothingService_GetNothingValues_InputMessage");
        getNothingValues.SetOutput("http://tempuri.org/INothingService/GetNothingValuesResponse","tns:INothingService_GetNothingValues_OutputMessage");
        
        var getNothingWithQuery = Operation.Create("GetNothingWithQuery");
        getNothingWithQuery.SetInput("http://tempuri.org/INothingService/GetNothingWithQuery","tns:INothingService_GetNothingWithQuery_InputMessage");
        getNothingWithQuery.SetOutput("http://tempuri.org/INothingService/GetNothingWithQueryResponse","tns:INothingService_GetNothingWithQuery_OutputMessage");
            
        wsdlReader.Operations.Should().BeEquivalentTo(new Dictionary<string, Operation>
        {
            { "GetNothingValues", getNothingValues },
            { "GetNothingWithQuery", getNothingWithQuery }
        });
    }
}