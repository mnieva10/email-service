using System.Collections.Generic;

namespace Sovos.SvcBus.Common.Model.Capability.Macroparsing.Response
{
    public class BaseResponseScript
    {
        [SvcBusSerializable]
        public string Exception { get; set; }

        [SvcBusSerializable]
        public bool Success { get; set; }

        [SvcBusSerializable]
        public List<string> ParsingErrors { get; set; }
    }
}
