// See https://aka.ms/
// new-console-template for more information

using System.Reflection;
using LightRail.Soap;
using LightRail.Soap.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

var services = new ServiceCollection();

services.AddSoapClient(configuration,"SoapClient-1", assemblies: Assembly.GetAssembly(typeof(SoapMessage)));

var provider = services.BuildServiceProvider();

var soapClient = provider.GetService<ISoapClient>();

var soapMessage = new SoapMessage
{
    Input = new Input { Id = 1, Query = "dupa" },
    ComplexInput = new ComplexInput { Id = 1 }
};

//var result = await soapClient.PostAsync(soapMessage);

