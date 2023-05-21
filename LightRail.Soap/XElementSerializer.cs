﻿using System.Xml.Linq;
using System.Xml.Serialization;

namespace LightRail.Soap;

public interface IXElementSerializer
{
    /// <summary>
    /// Serializes the specified object to XElement
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    XElement Serialize(object obj);
}

public class XElementSerializer : IXElementSerializer
{
    /// <inheritdoc />
    public XElement Serialize(object obj)
    {
        var xs = new XmlSerializer(obj.GetType());

        var xDoc = new XDocument();

        using (var xw = xDoc.CreateWriter())
            xs.Serialize(xw, obj);

        return xDoc.Root!;
    }
}
