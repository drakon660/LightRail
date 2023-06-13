using System.Collections.Concurrent;

namespace LightRail.Soap;

public static class EnvelopeFactories
{
    static class Cached
    {
        internal static readonly ConcurrentDictionary<SoapVersion, ISoapEnvelopeFactory> 
            Instance = new ();
    }

    public static ISoapEnvelopeFactory Get(SoapVersion soapVersion) => Cached.Instance[soapVersion];
    
    static EnvelopeFactories()
    {
        Cached.Instance.GetOrAdd(SoapVersion.Soap11, (_) => new Soap11EnvelopeFactory());
        Cached.Instance.GetOrAdd(SoapVersion.Soap12, (_) => new Soap12EnvelopeFactory());
    }
}