using System;
using System.Collections.Generic;

namespace Sovos.SvcBus.Common.Model.LongRunningJob
{
    [SvcBusBuildable]
    public abstract class BaseJob
    {
        [SvcBusSerializable]
        public int Id { get; set; }
        [SvcBusSerializable]
        public int ParentId { get; set; }
        [SvcBusSerializable]
        public int StepId { get; set; }
        [SvcBusSerializable]
        public DateTime CreatedOn { get; set; }
        [SvcBusSerializable]
        public DateTime ModifiedOn { get; set; }
        [SvcBusSerializable]
        public DateTime ExpiresOn { get; set; }
        [SvcBusSerializable]
        public string Command { get; set; }
        [SvcBusSerializable]
        public string ServiceName { get; set; }
        [SvcBusSerializable]
        public string Request { get; set; }
        [SvcBusSerializable]
        public int ResumeAfterStepId { get; set; }
        [SvcBusSerializable]
        public List<LrjProgress> ProgressList { get; set; }
        [SvcBusSerializable]
        public int Status { get; set; }

        protected BaseJob()
        {
            ProgressList = new List<LrjProgress>();
        }

        protected BaseJob(int id, int parentId, int stepId, int status, 
            DateTime createdOn, DateTime modifiedOn, DateTime expiresOn, 
            string serviceName, string request, string command)
        {
            Id = id;
            ParentId = parentId;
            StepId = stepId;
            Status = status;
            CreatedOn = createdOn;
            ModifiedOn = modifiedOn;
            ExpiresOn = expiresOn;
            ServiceName = serviceName;
            Request = request;
            Command = command;
        }

        public abstract void AddJob(BaseJob job);
        public abstract void RemoveJob(int id);
        public abstract BaseJob GetJob(int id);

        public abstract bool IsExpired();
    }
}
