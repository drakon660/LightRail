using System.Reflection;
using System.Xml.Serialization;
using BenchmarkDotNet.Attributes;
using LightRail.Soap.Contracts;

namespace LightRail.Soap.PerformanceTests;

public class GetCustomAttributeBenchmark
{
    [Benchmark]
    public void CustomAttributes()
    {
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