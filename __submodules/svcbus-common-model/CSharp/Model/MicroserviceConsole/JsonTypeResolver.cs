using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Sovos.SvcBus.Common.Model.MicroserviceConsole
{
    public class JsonTypeResolver : SimpleTypeResolver
    {
        private readonly Dictionary<string, Type> _registeredTypes;

        public JsonTypeResolver(Dictionary<string, Type> registeredTypes)
        {
            _registeredTypes = registeredTypes;
        }

        public override Type ResolveType(string id)
        {
            return _registeredTypes[id];
        }
    }
}
