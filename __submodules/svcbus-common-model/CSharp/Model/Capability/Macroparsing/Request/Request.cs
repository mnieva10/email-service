using System.Collections.Generic;

namespace Sovos.SvcBus.Common.Model.Capability.Macroparsing.Request
{
    [SvcBusBuildable]
    public class Request
    {
        [SvcBusSerializable]
        public string Schema { get; set; }

        [SvcBusSerializable]
        public string UserName { get; set; }

        [SvcBusSerializable]
        public List<RequestScript> Scripts { get; set; }
    }
}
