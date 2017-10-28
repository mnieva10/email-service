namespace Sovos.SvcBus.Common.Model.Capability
{
    [SvcBusBuildable]
    public class TxwEdiRenderRequest
    {
        [SvcBusSerializable]
        public string SegHash { get; set; }

        [SvcBusSerializable]
        public string Schema { get; set; }

        [SvcBusSerializable]
        public string RfmConnectionString { get; set; }

        [SvcBusSerializable]
        public string ClientReturnIndicator { get; set; }

        [SvcBusSerializable]
        public string FilingData { get; set; }
    }
}
