using System;
using System.Web.Script.Serialization;
using Sovos.SvcBus.Common.Model.Exceptions;
using Sovos.SvcBus.Common.Model.MicroserviceConsole.Capability;
using Sovos.SvcBus.Common.Model.Services;
using Sovos.SvcBus.Common.Model.Services.Interfaces;

namespace Sovos.SvcBus.Common.Model.LongRunningJob
{
    public abstract class JobHandler
    {
        protected JobHandler Successor { get; set; }
        protected SvcRequest SvcRequest { get; set; }
        protected bool IsProgressReportable { get; set; }

        protected IRemoteMicroservice LrjMngrMicroservice { get; set; }
        protected ILogger Logger { get; set; }

        public abstract void HandleRequest(LrjRequest request);

        protected JobHandler(IRemoteMicroservice lrjMngrMicroservice, SvcRequest svcRequest, ILogger logger, bool isProgressReportable = true)
        {
            if (lrjMngrMicroservice == null || logger == null)
                throw new InvalidParametersException();

            LrjMngrMicroservice = lrjMngrMicroservice;
            SvcRequest = svcRequest;
            Logger = logger;
            
            IsProgressReportable = isProgressReportable;
        }

        public JobHandler SetSuccessor(JobHandler successor)
        {
            Successor = successor;
            return successor;
        }

        public void ReportStatusChange(int jobId, JobStatus status)
        {
            LrjMngrMicroservice.SendCommand("ChangeJobStatus", new {JobId = jobId, Status = (int)status});
            Logger.WriteLogEntry(LogLevel.Debug, string.Format("ReportStatusChange for job #{2} as {1} from '{0}'.", GetType(), status, jobId));
        }

        public void ReportProgressChange(LrjProgress progress)
        {
            LrjMngrMicroservice.SendCommand("ChangeJobProgress", progress);
            Logger.WriteLogEntry(LogLevel.Debug, string.Format("ReportProgressChange from '{0}' job #{1} step #{2}.", GetType(), progress.JobId, progress.StepId));
        }

        public virtual void ContinueHandling(LrjRequest request, string message = "")
        {
            if (IsProgressReportable)
                ReportProgressChange(new LrjProgress { JobId = request.JobId, StepId = SvcRequest.StepId, ProgressStatus = string.Format("Completed. {0}", message) });

            if (Successor == null)
            {
                ReportStatusChange(request.JobId, JobStatus.Complete);
                Logger.WriteLogEntry(LogLevel.Debug, string.Format("{0}: {1} No successor to handle.", GetType(), message));
                return;
            } 
            
            Logger.WriteLogEntry(LogLevel.Debug, string.Format("{0}: {1} Handle successor '{2}'.", GetType(), message, Successor.GetType()));
            Successor.HandleRequest(request);
        }

        public virtual void Callback(Bson response, Message sourceMessage, Responder responder, object userdata)
        {
            Logger.WriteLogEntry(LogLevel.Debug, string.Format("Handle request callback from '{0}'. Bson Response: {1}.", GetType(), response != null ? response.ToJson() : "n/a"));
            var request = (LrjRequest)userdata;
            try
            {
                var resp = DeserializeBson<BsonResponse>(response);
                if (resp.exception == null)
                    ContinueHandling(request);
                else
                    Notify(new { Request = request, Response = response, Message = resp.exception.ToString(), ReportProgress = true});
            }
            catch (Exception ex)
            {
                Notify(new { Request = request, Response = response, ex.Message, ReportProgress = true });
            }
        }

        public virtual string Notify(dynamic args) //Request, Response, Message, ReportProgress
        {
            var errorRefCode = new CodeGenerator("WF", 5, 6).GenerateCode();

            var msg = string.Format("Error Ref Code: {4}; Handler: {0}; Request: {1}; Bson Response: {2}; Exception: {3}.",
                              GetType(), new JavaScriptSerializer().Serialize(SvcRequest),
                              args.Response != null ? args.Response.ToJson() : "n/a", args.Message, errorRefCode);
            Logger.WriteLogEntry(LogLevel.Error, msg);

            if (args.ReportProgress)
                ReportProgressChange(new LrjProgress { JobId = args.Request.JobId, StepId = args.Request.StepId, ProgressStatus = args.Message });
            ReportStatusChange(args.Request.JobId, JobStatus.Failed);

            return errorRefCode;
        }

        protected T DeserializeBson<T>(Bson responseBson)
        {
            return new JavaScriptSerializer().Deserialize<T>(responseBson.ToJson());
        }
    }
}
