using System.Collections.Generic;
using Sovos.SvcBus;

namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class SqlExecuteRequest
    {
        [SvcBusSerializable]
        public string Schema { get; set; }

        [SvcBusSerializable]
        public string UserName { get; set; }

        [SvcBusSerializable]
        public bool ParseAsPlSql { get; set; }

        [SvcBusSerializable]
        public List<SqlExecuteScript> Scripts { get; set; }

        [SvcBusSerializable]
        public bool IncludeDetails { get; set; }

        public SqlExecuteRequest()
        {
            IncludeDetails = true;
            ParseAsPlSql = false;
        }
    }
}
