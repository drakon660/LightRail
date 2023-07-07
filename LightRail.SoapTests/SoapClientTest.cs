using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using FluentAssertions;
using LightRail.Reflection;
using LightRail.Soap;
using LightRail.Soap.Contracts;

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
                action: "http://tempuri.org/INothingService/GetNothing",
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
            new(ns.GetName("value1"), "cos"),
            new(ns.GetName("value2"), 1),
        };

        methodBody.Add(values);

        var actual =
            await soapClient.PostAsync(
                new Uri("https://lightrail-2.azurewebsites.net/nothing.svc"),
                SoapVersion.Soap11,
                action: "http://tempuri.org/INothingService/GetNothingWithSimpleInput",
                body: methodBody);

        Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
    }

    [Fact]
    public async Task Check_SoapClient_PostAsync_With_Raw_Data()
    {
        var soapClient = new SoapClient();

        string bodies = """
                                       <tem:GetValues>
                                        <tem:input>
                        
                                        <Id>1</Id>
                        
                                        <Query>dwa</Query>
                                        </tem:input>
                        
                                 <tem:complexInput>
                        
                                <Id>1</Id>

                        <Query>
                           
                           <From>1</From>
                           
                           <Size>2</Size>
                        </Query>
                                </tem:complexInput>
                            </tem:GetValues>

                        """;
        string headers = "";

        var actual =
            await soapClient.PostAsync(
                new Uri("http://localhost:8667/sample-45830D75-D6F6-420F-B22F-D721E354C6A5.svc"),
                SoapVersion.Soap11,
                action: "http://tempuri.org/INothingInputService/GetValues",
                bodies: bodies, headers: headers);

        Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
    }

    [Fact]
    public async Task Check_If_Instance_Is_Created_Properly_With_GetValues_Generic_Version()
    {
        XNamespace tempuri = "http://tempuri.org/";

        IReadOnlyDictionary<string, SoapAttributeAttribute> attributes =
            ReflectionUtils.GetCustomAttributes<SoapAttributeAttribute>(typeof(Soap.Contracts.DifferentSoapMessage));

        var attr = attributes.ToDictionary(x => x.Key, y => (Name:y.Value.AttributeName, y.Value.Namespace));
        
        SoapEnvelopeBuilder envelopeBuilder = new(attr);
        
        DifferentSoapMessage soapMessage = new DifferentSoapMessage()
        {
            Input = new () { Item = new Item { Value = "222"} },
        };

        SoapClient soapClient = new SoapClient(envelopeBuilder);

        HttpResponseMessage? actual =
            await soapClient.PostAsync(new Uri("http://localhost:8667/sample-45830D75-D6F6-420F-B22F-D721E354C6A5.svc"),
                SoapVersion.Soap11, soapMessage, "Shoot", "http://tempuri.org/IDifferentService/Shoot");
        
        string value = await actual.Content.ReadAsStringAsync();
    }
}