using System.Xml.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using LightRail.Soap.Contracts;

namespace LightRail.Soap.PerformanceTests;

[MemoryDiagnoser]
public class SoapBuilderBenchmark
{
    private SoapEnvelopeBuilder2 _envelopeBuilder;
    private const string SoapSchema = "http://schemas.xmlsoap.org/soap/envelope/";
    [GlobalSetup]
    public void Setup()
    {
        _envelopeBuilder = new();
    }

    [Benchmark]
    public void XElement_Raw()
    {
        XNamespace SoapSchema = "http://schemas.xmlsoap.org/soap/envelope/";
        
        var ns = XNamespace.Get("http://tempuri.org/");
        
        var envelope = new XElement(SoapSchema + "Envelope", new XAttribute(
            XNamespace.Xmlns + "soapenv", SoapSchema.NamespaceName));

        envelope.Add(new XAttribute(
            XNamespace.Xmlns + "tns",
            ns.NamespaceName));
        
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
        envelope.Add(methodBody);
    }

    [Benchmark]
    public void SoapBuilder()
    {
        XNamespace tempuri = "http://tempuri.org/";
        
        var envelope = _envelopeBuilder.GetEnvelope(tempuri, "GetValues", new SoapMessage
        {
            Input = new Input { Id = 1, Query = "dupa" },
            ComplexInput = new ComplexInput(1, new Query(23,23))
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