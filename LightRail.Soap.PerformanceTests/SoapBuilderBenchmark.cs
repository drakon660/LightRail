using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using LightRail.Reflection;
using LightRail.Soap.Contracts;

namespace LightRail.Soap.PerformanceTests;

[MemoryDiagnoser]
//[Config(typeof(Config))]
public class SoapBuilderBenchmark
{
    private SoapEnvelopeBuilder _envelopeBuilder;
    private const string SoapSchema = "http://schemas.xmlsoap.org/soap/envelope/";
    XNamespace _tempuri = "http://tempuri.org/";
    [GlobalSetup]
    public void Setup()
    {
        var attributes =
            ReflectionUtils.GetCustomAttributes<SoapAttributeAttribute>(typeof(Soap.Contracts.SoapMessage));
        var attr = attributes.ToDictionary(x => x.Key, y => (Name:y.Value.AttributeName, y.Value.Namespace));
        _envelopeBuilder = new(attr);
    }

    [Benchmark]
    public void XElement_Raw()
    {
        XNamespace SoapSchema = "http://schemas.xmlsoap.org/soap/envelope/";
        
        var ns = XNamespace.Get("http://tempuri.org/");
        var tem = XNamespace.Get("http://schemas.datacontract.org/2004/07/Interstate.SoapTestService");
        
        var envelope = new XElement(SoapSchema + "Envelope", new XAttribute(
            XNamespace.Xmlns + "soapenv", SoapSchema.NamespaceName));

        envelope.Add(new XAttribute(
            XNamespace.Xmlns + "tns",
            ns.NamespaceName));
        
        envelope.Add(new XAttribute(
            XNamespace.Xmlns + "tem",
            tem.NamespaceName));
        
        var methodBody = new XElement(ns.GetName("GetValues"));

        List<XElement> values = new List<XElement>()
        {
            new (ns.GetName("input"), new [] 
            {
                new XElement(tem.GetName("Id"),1),
                new XElement(tem.GetName("Query"),"test"),
            }),
            new (ns.GetName("complexInput"), new []
            {
                new XElement(tem.GetName("Id"),1),
                // new XElement(tem.GetName("Query"), new []
                // {
                //     new XElement(tem.GetName("From"),1),
                //     new XElement(tem.GetName("Size"),12)
                // }),
            }),
        };
        var body = new XElement((XNamespace)SoapSchema + "Body");
        var header = new XElement((XNamespace)SoapSchema + "Header");
        methodBody.Add(values);
        body.Add(methodBody);
        envelope.Add(header);
        envelope.Add(body);
    }

    [Benchmark]
    public void SoapBuilder()
    {
        var envelope = _envelopeBuilder.GetEnvelope(_tempuri, "GetValues", new SoapMessage
        {
            Input = new Input { Id = 1, Query = "dupa" },
            ComplexInput = new ComplexInput { Id = 1 }
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
        }
    }
}