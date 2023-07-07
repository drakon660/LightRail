using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using LightRail.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LightRail.Soap;

public static class SoapClientExtensions
{
    public static Task<HttpResponseMessage> PostAsync(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        XElement body,
        XElement? header = null,
        string? action = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.PostAsync(
            endpoint,
            soapVersion,
            new[] { body },
            header != null ? new[] { header } : default(IEnumerable<XElement>),
            action,
            cancellationToken);
    }

    public static Task<HttpResponseMessage> PostAsync(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        IEnumerable<XElement> bodies,
        XElement header,
        string? action = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.PostAsync(
            endpoint,
            soapVersion,
            bodies,
            new[] { header },
            action,
            cancellationToken);
    }

    public static Task<HttpResponseMessage> PostAsync(
        this ISoapClient client,
        Uri endpoint,
        SoapVersion soapVersion,
        XElement body,
        IEnumerable<XElement> headers,
        string? action = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.PostAsync(
            endpoint,
            soapVersion,
            new[] { body },
            headers,
            action,
            cancellationToken);
    }
    
    public static IServiceCollection AddSoapClient(this IServiceCollection serviceCollection,
        IConfiguration configurationSection, string name,  params Assembly[] assemblies)
    {
        var types = assemblies.SelectMany(x=>x.DefinedTypes);

        foreach (var type in types.Where(x=>x.IsClass))
        {
            var dictionary = ReflectionUtils.GetCustomAttributes<SoapAttributeAttribute>(type);
        }
        
        serviceCollection.Configure<SoapClientOptions>(_=> configurationSection.GetSection("SoapClients"));
        serviceCollection.AddHttpClient(name, httpClient => { });
        
        //TODO add something that will take all attributes
        serviceCollection.AddSingleton<SoapEnvelopeBuilder>();
        
        serviceCollection.AddTransient<ISoapClient, SoapClient>();

        return serviceCollection;
    }
}