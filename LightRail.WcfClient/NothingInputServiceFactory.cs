using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace LightRail.WcfClient;

public class NothingInputServiceFactory
{
    public static NothingInputServiceClient Create(string url)
    {
        return new NothingInputServiceClient(GetBindingByUrl(url), new EndpointAddress(url));
    }
    
    public static Binding GetBindingByUrl(string url)
    {
        BasicHttpBinding binding = new BasicHttpBinding();
        binding.MaxBufferSize = int.MaxValue;
        binding.ReaderQuotas = XmlDictionaryReaderQuotas.Max;
        binding.MaxReceivedMessageSize = int.MaxValue;
        binding.AllowCookies = true;
        binding.Security.Mode = BasicHttpSecurityMode.None;
        
        if(url.Contains("https"))
            binding.Security.Mode = BasicHttpSecurityMode.Transport;
        
        return binding;
    }
}