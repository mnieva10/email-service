using System.Collections.Generic;

namespace Sovos.SvcBus.Common.Model.Capability.Macroparsing.Response
{
    [SvcBusBuildable]
    public class PlSqlResponseScript : BaseResponseScript
    {
        [SvcBusSerializable]
        public List<string> PlSqlBlocks { get; set; }
    }
}
