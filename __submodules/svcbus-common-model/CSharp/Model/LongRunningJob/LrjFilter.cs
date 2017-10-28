namespace Sovos.SvcBus.Common.Model.LongRunningJob
{
    [SvcBusBuildable]
    public class LrjFilter
    {
        [SvcBusSerializable]
        public string SegHash { get; set; }

        [SvcBusSerializable]
        public string Username { get; set; }

        [SvcBusSerializable]
        public bool? IsExpired { get; set; }

        [SvcBusSerializable]
        public JobStatus? JobStatus { get; set; }

        public LrjFilter() { }
    }
}
