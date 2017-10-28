namespace Sovos.SvcBus.Common.Model.LongRunningJob
{
    [SvcBusBuildable]
    public abstract class SvcRequest
    {
        [SvcBusSerializable]
        public int StepId { get; set; }
    }
}
