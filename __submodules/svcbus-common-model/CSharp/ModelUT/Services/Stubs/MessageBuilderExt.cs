using System;
using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.MicroserviceConsole;
using Sovos.SvcBus.Common.Model.MicroserviceConsole.Capability;

namespace ModelUT.Services.Stubs
{
    public class MessageBuilderExt : MessageBuilder
    {
        public new void BuildObjectMap(IDictionary<string, object> objMap, List<CmdParameter> properties)
        {
            base.BuildObjectMap(objMap, properties);
        }

        public new List<CmdParameter> ParseArguments(List<string> arguments)
        {
            return base.ParseArguments(arguments);
        }

        public new string ParseParameter(List<string> parameters, string key)
        {
            return base.ParseParameter(parameters, key);
        }

        public new Type ParseObjectType(List<string> arguments)
        {
            return base.ParseObjectType(arguments);
        }
    }
}
