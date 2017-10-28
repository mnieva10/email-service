using Sovos.SvcBus;

namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class SqlExecuteResult
    {
        [SvcBusSerializable]
        public bool Success { get; set; }

        [SvcBusSerializable]
        public int RowsAffected { get; set; }

        [SvcBusSerializable]
        public string ErrorMessage { get; set; }
    }
}
