namespace Sovos.SvcBus.Common.Model.LongRunningJob
{
    [SvcBusBuildable]
    public class LrjRequest : SvcRequest
    {
        [SvcBusSerializable]
        public int JobId { get; set; }

        [SvcBusSerializable]
        public int ParentId { get; set; }

        [SvcBusSerializable]
        public int ResumeAfterStepId { get; set; }

        [SvcBusSerializable]
        public string Username { get; set; }

        [SvcBusSerializable]
        public string Schema { get; set; }

        [SvcBusSerializable]
        public string SegHash { get; set; }
        
        public LrjRequest() { }

        //public LrjRequest(int jobId, int resumeAfterStepId)
        //{
        //    JobId = jobId;
        //    ResumeAfterStepId = resumeAfterStepId;
        //}
    }
}
