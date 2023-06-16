using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using FluentAssertions;
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
                action:"http://tempuri.org/INothingService/GetNothing",
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
    
    [Fact]
    public async Task Check_If_Instance_Is_Created_Properly_With_GetValues()
    {
        var soapClient = new SoapClient();

        string n = "http://tempuri.org/";
        string operation = "GetNothingValues";
        string action = "http://tempuri.org/INothingService/GetNothingValues";
        
        SoapEnvelopeBuilder xmlSerializer = new SoapEnvelopeBuilder();
        var message = new SoapMessage();
        message.Input = new Input(12, "dupa");
        message.Value1 = 12;
        
        var result = xmlSerializer.Initialize(n,operation,message);
        
        var actual =
            await soapClient.PostAsync(
                new Uri("https://lightrail-2.azurewebsites.net/nothing.svc"),
                SoapVersion.Soap11,
                action:"http://tempuri.org/INothingService/GetNothingValues",
                bodies: new [] { result } );

        string content = await actual.Content.ReadAsStringAsync();

        //var resultElement = XElement.Parse(content);
        
        //resultElement.Element()
        
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
                action:"http://tempuri.org/INothingInputService/GetValues",
                bodies: bodies, headers:headers );
        
        Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
    }
    //
    // [Fact]
    // public async Task Check_If_Instance_Is_Created_Properly_With_GetValues_Generic_Version()
    // {
    //     var soapClient = new SoapClient();
    //
    //     var soapMessage = new SoapMessage()
    //     {
    //         Input = new Input(1,"cos"),
    //         ComplexInput = new ComplexInput(12, new Query(1,2))
    //     };
    //     
    //     var actual =
    //         await soapClient.PostAsync<Reponse,SoapMessage>(
    //             "http://localhost:8667/sample-45830D75-D6F6-420F-B22F-D721E354C6A5.svc",
    //             SoapVersion.Soap11,
    //             message: soapMessage
    //         );
    // }

    public record SoapMessage : ISoapMessage
    {
        public Input Input { get; set; }
        public int Value1 { get; set; }
    }

    public record input(int Id, string Query);
    public record ComplexInput(int Id, Query Query);
    public record Query(int From, int Size);
}