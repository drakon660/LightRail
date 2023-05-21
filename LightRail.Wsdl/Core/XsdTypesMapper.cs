namespace LightRail.Wsdl.Core;

public class XsdTypesMapper
{
    public static IReadOnlyDictionary<string, Type> XsdTypes = new Dictionary<string, Type>()
    {
        { "xs:byte", typeof(sbyte) },
        { "xs:boolean", typeof(bool) },
        { "xs:date", typeof(DateTime) },
        { "xs:dateTime", typeof(DateTime) },
        { "xs:decimal", typeof(decimal) },
        { "xs:double", typeof(double) },
        { "xs:duration", typeof(TimeSpan) },
        { "xs:float", typeof(float) },
        { "xs:gDay", typeof(DateTimeOffset) },
        { "xs:gMonth", typeof(DateTimeOffset) },
        { "xs:gMonthDay", typeof(DateTimeOffset) },
        { "xs:gYear", typeof(DateTimeOffset) },
        { "xs:gYearMonth", typeof(DateTimeOffset) },
        { "xs:hexBinary", typeof(byte[]) },
        { "xs:int", typeof(int) },
        { "xs:integer", typeof(decimal) },
        { "xs:long", typeof(long) },
        { "xs:string", typeof(string) },
        { "xs:time", typeof(DateTime) },
        { "xs:unsignedByte", typeof(byte) },
        { "xs:unsignedInt", typeof(uint) },
        { "xs:unsignedLong", typeof(ulong) },
        { "xs:unsignedShort", typeof(ushort) },
    };

    public static bool IsSimpleType(string typeValue) => XsdTypes.ContainsKey(typeValue);
}