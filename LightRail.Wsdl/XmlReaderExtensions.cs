using System.Xml;

namespace LightRail.Wsdl;

public static class XmlReaderExtensions
{
    public static string GetNameAttribute(this XmlReader reader) => reader.GetAttribute("name");
    public static string GetTypeAttribute(this XmlReader reader) => reader.GetAttribute("type");
}

