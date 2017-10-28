namespace Sovos.SvcBus.Common.Model.LongRunningJob
{
    public enum JobStatus
    {
        Cancelled = 0,
        Paused = 1,
        NeedsReview = 2,
        Failed = 3,
        New = 4,
        Complete = 5,
        InProgress = 6
    }
}