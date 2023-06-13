using System.ServiceModel;
using System.Xml.Linq;
using BenchmarkDotNet.Attributes;
using LightRail.WcfClient;

namespace LightRail.Soap.PerformanceTests;

public class SoapClientBenchmark
{
    private NothingInputServiceClient _wcfClient;
    private SoapClient _soapClient;

    private const string WcfServiceUrl = "http://localhost:8667/sample-45830D75-D6F6-420F-B22F-D721E354C6A5.svc";
    
    [GlobalSetup]
    public void Setup()
    {
        _wcfClient = NothingInputServiceFactory.Create(WcfServiceUrl);
        _soapClient = new SoapClient();
    }

    [Benchmark]
    public async Task WcfClient()
    {
        await _wcfClient.GetValuesAsync(new Input(){Id = 15, Query = "Sample Query"}, new ComplexInput()
        {
            Id = 25,
            Query = new ComplexQuery()
            {
                From = 0,
                Size = 100
            }
        });
    }
    
    // [Benchmark]
    // public void WcfClientFactory()
    // {
    //    
    // }

    [Benchmark]
    public async Task SoapClient()
    {
        var ns = XNamespace.Get("http://tempuri.org/");

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
        
        var actual =
            await _soapClient.PostAsync(
                new Uri(WcfServiceUrl),
                SoapVersion.Soap11,
                action:"http://tempuri.org/INothingInputService/GetValues",
                bodies: new []{methodBody} );
    }
}