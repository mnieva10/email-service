using System;
using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class Resource
    {
        private readonly byte[] _defaultParameters;
        private readonly string _customParametersStr;
        private readonly byte[] _customParameters;

        [SvcBusSerializable]
        public string ResourceName { get; set; }
        [SvcBusSerializable]
        public string Script { get; set; }
        [SvcBusSerializable]
        public string ParsedScript { get; set; }
        [SvcBusSerializable]
        public DateTime DateChanged { get; set; }
        [SvcBusSerializable]
        public MacroParameters MacroParameters { get; set; }

        public string DefaultVarsDefs { get { return _defaultParameters != null ? System.Text.Encoding.UTF8.GetString(_defaultParameters) : string.Empty; } }
        public string CustomVarsDefs { get { return string.IsNullOrEmpty(_customParametersStr) ? (_customParameters != null ? System.Text.Encoding.UTF8.GetString(_customParameters) : string.Empty) : _customParametersStr; } }

        public Resource()
        {
            MacroParameters = new MacroParameters
                {
                    Vars = new Dictionary<string, string>(),
                    Defs = new Dictionary<string, string>()
                };
        }

        public Resource(string resourceName) : this()
        {
            ResourceName = resourceName;
        }

        public Resource(string resourceName, DateTime dateChanged) : this(resourceName)
        {
            DateChanged = dateChanged;
        }

        public Resource(string resourceName, string script, byte[] defaultParameters) : this(resourceName)
        {
            Script = script;
            _defaultParameters = defaultParameters;
        }

        public Resource(string resourceName, byte[] customParameters, string customParametersStr) : this(resourceName)
        {
            _customParameters = customParameters;
            _customParametersStr = customParametersStr;
        }
    }
}