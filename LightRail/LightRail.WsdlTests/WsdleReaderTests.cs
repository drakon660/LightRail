using System.Text;
using FluentAssertions;
using FluentAssertions.Equivalency;
using LightRail.Wsdl;

namespace LightRail.WsdlTests;

public class WsdleReaderTests
{
    [Fact]
    public void Check_If_WsdlReader_Is_Reading_Tree()
    {
        string sampleXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
        string sampleXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
            .BeEquivalentTo(NodeElement.Create("to", "to", new Dictionary<string, string>()
            {
                { "id", "15" }
            }));

        wsdlReader.Root.HasMultipleChildren.Should().BeTrue();
        wsdlReader.Root.Children.Count.Should().Be(4);
    }

    [Fact]
    public void Check_If_WsdlReader_Is_Resolving_Soap_Operation_Namespace()
    {
        string sampleXml = """
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
}