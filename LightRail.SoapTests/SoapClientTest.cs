using System.Net;
using System.Xml;
using System.Xml.Linq;
using LightRail.Soap;

namespace LightRail.SoapTests;

public class SoapClientTest
{
    [Fact]
    public async Task Check_If_Instance_Is_Created_Properly_With_GetNothing()
    {
        var soapClient = new SoapClient();
        var ns = XNamespace.Get("http://tempuri.org/");

        var actual =
            await soapClient.PostAsync(
                new Uri("https://lightrail-2.azurewebsites.net/nothing.svc"),
                SoapVersion.Soap11,
                action:" http://tempuri.org/INothingService/GetNothing",
                body: new XElement(ns.GetName("GetNothing")));

        Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
    }
    
    [Fact]
    public async Task Check_If_Instance_Is_Created_Properly_With_GetNothingWithSimpleInput()
    {
        var soapClient = new SoapClient();
        var ns = XNamespace.Get("http://tempuri.org/");

        var methodBody = new XElement(ns.GetName("GetNothingWithSimpleInput"));

        List<XElement> values = new List<XElement>()
        {
            new (ns.GetName("value1"),"cos"),
            new (ns.GetName("value2"),1),
        };
        
        methodBody.Add(values);
        
        var actual =
            await soapClient.PostAsync(
                new Uri("https://lightrail-2.azurewebsites.net/nothing.svc"),
                SoapVersion.Soap11,
                action:"http://tempuri.org/INothingService/GetNothingWithSimpleInput",
                body: methodBody);

        Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
    }
}