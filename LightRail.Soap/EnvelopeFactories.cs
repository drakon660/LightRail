using System.Collections.Concurrent;
using System.Xml.Linq;

namespace LightRail.Soap;

public static class EnvelopeFactories
{
    private const string SoapSchema = "http://schemas.xmlsoap.org/soap/envelope/";
    public static class Cached
    {
        internal static readonly ConcurrentDictionary<SoapVersion, ISoapEnvelopeFactory> 
            Instance = new ();
        
        internal static readonly ConcurrentDictionary<string, XElement> 
            XInstance = new ();

        public static XElement Header;
    }

    public static ISoapEnvelopeFactory Get(SoapVersion soapVersion) => Cached.Instance[soapVersion];
    public static XElement GetBody() => Cached.XInstance["Body"];
    public static XElement GetHeader() => Cached.XInstance["Header"];
    
    static EnvelopeFactories()
    {
        Cached.Instance.GetOrAdd(SoapVersion.Soap11, (_) => new Soap11EnvelopeFactory());
        Cached.Instance.GetOrAdd(SoapVersion.Soap12, (_) => new Soap12EnvelopeFactory());

        Cached.Header = new XElement((XNamespace)SoapSchema + "Header");
        
        //Cached.XInstance.GetOrAdd("Body", (_) => new XElement((XNamespace)SoapSchema + "Body"));
        //Cached.XInstance.GetOrAdd("Header", (_) => new XElement((XNamespace)SoapSchema + "Header"));
    }
}