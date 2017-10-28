using System;

namespace Sovos.SvcBus.Common.Model.LongRunningJob
{
    [SvcBusBuildable]
    public class LrjProgress
    {
        [SvcBusSerializable]
        public int JobId { get; set; }
        [SvcBusSerializable]
        public DateTime ReportedOn { get; set; }
        [SvcBusSerializable]
        public string ProgressStatus { get; set; }
        [SvcBusSerializable]
        public int StepId { get; set; }

        public LrjProgress() {}

        public LrjProgress(int jobId, string progressStatus, int stepId)
        {
            JobId = jobId;
            ProgressStatus = progressStatus;
            StepId = stepId;
        }

        public LrjProgress(int jobId, DateTime reportedOn, string progressStatus, int stepId)
            : this(jobId, progressStatus, stepId)
        {
            ReportedOn = reportedOn;
        }
    }
}
