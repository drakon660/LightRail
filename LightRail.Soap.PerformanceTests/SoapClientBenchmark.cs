using System.Xml.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace LightRail.Soap.PerformanceTests;

[MemoryDiagnoser]
//[Config(typeof(Config))]
public class SoapClientBenchmark
{
    private LightRail.WcfClient.NothingInputServiceClient _wcfClient;
    private SoapClient _soapClient;
    private SoapClient _soapClient2;

    private const string WcfServiceUrl = "http://localhost:8667/sample-45830D75-D6F6-420F-B22F-D721E354C6A5.svc";
    
    [GlobalSetup]
    public void Setup()
    {
        _wcfClient = LightRail.WcfClient.NothingInputServiceFactory.Create(WcfServiceUrl);
        _soapClient = new SoapClient();
        _soapClient2 = new SoapClient();
    }

   
    // [Benchmark]
    // public void WcfClientFactory()
    // {
    //    
    // }

    //[Benchmark]
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

    //[Benchmark]
    public async Task SoapClientRawBody()
    {
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
            await _soapClient.PostAsync(
                new Uri("http://localhost:8667/sample-45830D75-D6F6-420F-B22F-D721E354C6A5.svc"),
                SoapVersion.Soap11,
                action:"http://tempuri.org/INothingInputService/GetValues",
                bodies: bodies, headers:headers );
    }

    // [Benchmark]
    // public async Task SoapClientGeneric()
    // {
    //     string n = "http://tempuri.org/";
    //     string operation = "GetValues";
    //     string action = "http://tempuri.org/INothingInputService/GetValues";
    //     
    //     SoapEnvelopeBuilder xmlSerializer = new SoapEnvelopeBuilder();
    //     var message = new SoapMessage();
    //     message.Input = new Input(12, "dupa");
    //     message.ComplexInput = new ComplexInput(32, new Query(3, 6));
    //
    //     var result = xmlSerializer.BuildEnvelope(n, );
    //     
    //     var actual =
    //         await _soapClient.PostAsync(
    //             new Uri("http://localhost:8667/sample-45830D75-D6F6-420F-B22F-D721E354C6A5.svc"),
    //             SoapVersion.Soap11,
    //             action:"http://tempuri.org/INothingInputService/GetValues",
    //             bodies: new [] { result } );
    // }
    
    //[Benchmark]
    public async Task WcfClient()
    {
        await _wcfClient.GetValuesAsync(new LightRail.WcfClient.Input(){Id = 15, Query = "Sample Query"}, new LightRail.WcfClient.ComplexInput()
        {
            Id = 25,
            Query = new LightRail.WcfClient.ComplexQuery()
            {
                From = 0,
                Size = 100
            }
        });
    }
    
    private class Config : ManualConfig
    {
        public Config()
        {
            AddJob(new Job(Job.Dry.WithGcServer(true).WithRuntime(CoreRuntime.Core70))
            {
                Environment = { Jit = Jit.Default, Platform = Platform.X64 },
                Run = { LaunchCount = 3, WarmupCount = 1, IterationCount = 5 },
                Accuracy = { MaxRelativeError = 0.01 },
            });
            // AddJob(new Job(Job.Dry.WithGcServer(true).WithRuntime(CoreRuntime.Core60))
            // {
            //     Environment = { Jit = Jit.Default, Platform = Platform.X64 },
            //     Run = { LaunchCount = 1, WarmupCount = 1, IterationCount = 1 },
            //     Accuracy = { MaxRelativeError = 0.01 },
            // });
        }
    }
    
    public record SoapMessage : ISoapMessage
    {
        public Input Input { get; set; }
        public ComplexInput ComplexInput { get; set; }
    }
    
    public record Input(int Id, string Query);
    public record ComplexInput(int Id, Query Query);
    public record Query(int From, int Size);
}

