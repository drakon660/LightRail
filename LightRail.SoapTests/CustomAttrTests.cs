using System.Reflection;
using System.Xml.Serialization;
using LightRail.Soap.Contracts;

namespace LightRail.SoapTests;

public class CustomAttrTests
{
    [Fact]
    public void Check_Custom_Attributes()
    {
        var attributes = typeof(SoapMessage).GetProperties()
             .Select(x=>(x.Name, Attribute: x.GetCustomAttribute(typeof(SoapAttributeAttribute))))
             .ToDictionary((x)=>x.Name, y => y.Attribute);
        
        var attribute =
            typeof(SoapMessage).GetProperty(nameof(SoapMessage.Input));

        var attribute1 =
            typeof(SoapMessage).GetProperty(nameof(SoapMessage.ComplexInput));

        // var attribute2 =
        //     typeof(Soap.Contracts.ComplexInput).GetProperty(nameof(SoapMessage.ComplexInput));
        //
        //
        
        var soapAttribute = attribute.GetCustomAttribute(typeof(SoapAttributeAttribute));
        var soapAttribute1 = attribute1.GetCustomAttribute(typeof(SoapAttributeAttribute));
        
    }
}