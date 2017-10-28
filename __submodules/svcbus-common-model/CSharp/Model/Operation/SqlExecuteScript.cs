using System.Collections.Generic;
using Sovos.SvcBus;

namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class SqlExecuteScript
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
