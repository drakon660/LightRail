using System.Collections.Generic;

namespace LightRail.Reflection;

public sealed class Class : Instance
{
    private IList<Property> _properties;

    public IList<Property> Properties => _properties ??= new List<Property>();
}