using System;

namespace Sovos.SvcBus.Common.Model.Capability
{
    [Flags]
    public enum AcaXmitStatus
    {
        PreQueued = -1,
        Queued = 0,
        IrsProcessing = 1,
        Complete = 2,
        Cancelled = 3,
        NeedsReview = 4,
        Importing = 5,
        Imported = 6
    }

    [Flags]
    public enum AcaXmitLogCodes
    {
        Create = 0,
        Submit = 1,
        CheckStatus = 2,
        Download = 3,
        StatusReset = 4,
        NotifyQueued = 5,
        Import = 6
    }

    [Flags]
    public enum AcaXmtPauseCommands
    {
        SUBMIT = 0,
        RETRIEVE = 1
    }

    [Flags]
    public enum AcaXmitTimerType
    {
        Submit = 1,
        SubmitRetry = 2,
        Retrieve = 3,
        RetrieveRetry = 4,
        Unlock = 5,
        QueueSubmit = 6,
        Import = 7
    }
}