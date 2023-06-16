using System.Collections.Generic;
using System.Text;

namespace LightRail.Reflection
{
    public sealed class Method
    {
        public string Name { get; set; }

        private IList<Parameter> _properties;

        public IList<Parameter> Parameters => _properties ??= new List<Parameter>();

        public Instance ReturnValue { get; set; }
    }
}