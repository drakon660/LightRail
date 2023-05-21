using System.Net;
using System.Xml.Linq;
using LightRail.Soap;

namespace LightRail.SoapTests;

public class SoapClientTest
{
    [Fact]
    public async Task Check_If_Instance_Is_Created_Properly()
    {
        var soapClient = new SoapClient();
        var ns = XNamespace.Get("http://tempuri.org/");

        var actual =
            await soapClient.PostAsync(
                new Uri("http://lightrail-2.azurewebsites.net/nothing.svc"),
                SoapVersion.Soap12,
                action:"http://tempuri.org/INothingService/GetNothingValues",
                body: new XElement(ns.GetName("GetNothing2")));

        Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
    }
}