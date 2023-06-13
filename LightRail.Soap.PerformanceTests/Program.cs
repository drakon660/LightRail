// See https://aka.ms/new-console-template for more information

//Console.WriteLine("Hello, World!");

using BenchmarkDotNet.Running;
using LightRail.Soap.PerformanceTests;

BenchmarkRunner.Run<SoapClientBenchmark>();