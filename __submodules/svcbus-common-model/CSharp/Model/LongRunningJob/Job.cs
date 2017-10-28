using System;
using System.Collections.Generic;
using System.Linq;

namespace Sovos.SvcBus.Common.Model.LongRunningJob
{
    [SvcBusBuildable]
    public class Job : BaseJob
    {
        [SvcBusSerializable]
        public List<BaseJob> Steps { get; set; }

        public Job()
        {
            Steps = new List<BaseJob>();
        }

        public Job(int id, int parentId, int stepId, int status, 
            DateTime createdOn, DateTime modifiedOn, DateTime expiresOn, 
            string serviceName, string request, string command)
            : base(id, parentId, stepId, status, createdOn, modifiedOn, expiresOn, serviceName, request, command)
        {
            Steps = new List<BaseJob>();
        }

        public override void AddJob(BaseJob job)
        {
            var found = Steps.Find(s => s.Id == job.Id);
            if (found == null)
                Steps.Add(job);
        }

        public override void RemoveJob(int id)
        {
            var found = Steps.Find(s => s.Id == id);
            if (found != null)
                Steps.Remove(found);
        }

        public override BaseJob GetJob(int id)
        {
            var found = Steps.Find(s => s.Id == id);
            return found ?? Steps.Select(step => step.GetJob(id)).FirstOrDefault(found1 => found1 != null);
        }

        public override bool IsExpired()
        {
            return DateTime.Compare(DateTime.Now, ExpiresOn) >= 0;
        }

        //SM: not sure we need it due to services stateless nature
        //public override void ResetStatus()
        //{
        //    foreach (var step in Steps)
        //        _status = (JobStatus) Math.Min((int) _status, (int) step.Status);
        //}
    }
}
