using System.Collections.Generic;
using Sovos.SvcBus;

namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class SqlExecuteResponse
    {
        [SvcBusSerializable]
        public bool IsSuccessful { get; set; }

        [SvcBusSerializable]
        public List<SqlExecuteResult> ResultSet { get; set; }
    }
}
