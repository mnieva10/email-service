using System.Collections.Generic;

namespace Sovos.SvcBus.Common.Model.Capability.Macroparsing.Request
{
    [SvcBusBuildable]
    public class RequestScript
    {
        [SvcBusSerializable]
        public string Script { get; set; }

        [SvcBusSerializable]
        public string Text { get; set; }

        [SvcBusSerializable]
        public string Dialect { get; set; }

        [SvcBusSerializable]
        public Dictionary<string, int> Defs { get; set; }

        [SvcBusSerializable]
        public Dictionary<string, string> Vars { get; set; }
    }
}
