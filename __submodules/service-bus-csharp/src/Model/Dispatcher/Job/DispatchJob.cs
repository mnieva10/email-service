using System;
using System.Reflection;

namespace Sovos.SvcBus
{
  internal class DispatchJob
  {
    readonly object _userData;
    private readonly IProducerJobSignaler _producerJobsignaler;
    private readonly IRequestProcessingStrategy _requestProcessingStrategy;
    private readonly PipeService _ps;
    private readonly Service _service;
    
    public DispatchInterfaceAccessor DispatchInterfaceAccessor { get; set; }
    public Message Msg { get; set; }
    public IResponseProcessingStrategy ResponseProcessingStrategy { get; set; }
    public IReplier Replier { get; set; }
    public ILogger Logger { get; set; }

    public DispatchJob(object userData,
      IProducerJobSignaler producerJobsignalizer,
      IRequestProcessingStrategy requestProcessingStrategy,
      PipeService ps,
      Service service)
    {
      _userData = userData;
      _producerJobsignaler = producerJobsignalizer;
      _requestProcessingStrategy = requestProcessingStrategy;
      _ps = ps;
      _service = service;
      Logger = new SimpleConsoleLogger();
    }

    public void Run(object context)
    {
      try
      {
        using (var scope = new Scope())
        {
          try
          {
            if (string.IsNullOrEmpty(Msg.command))
              throw new CommandNotFoundException();

            var dispatchInterface = DispatchInterfaceAccessor.AcquireDispatchInterface(_userData);
            dispatchInterface.ResponseProcessingStrategy = ResponseProcessingStrategy;
            var mi = dispatchInterface.GetType().GetMethod(Msg.command);
            if (mi == null)
              throw new MethodNotFoundException(Msg.command);

            try
            {
              CallDispatchInterfaceMethod(mi, dispatchInterface, scope);
            }
            catch (TargetInvocationException e)
            {
              if (e.InnerException != null)
                throw e.InnerException;
              throw;
            }
            finally
            {
              DispatchInterfaceAccessor.ReleaseDispatchInterface(dispatchInterface);
            }
          }
          catch (Exception e)
          {
            try
            {
              _ps.statsCollector.SendReport(string.Format("svcbus.produce_exception,service={0},msg_id={1},command={2}",
                _service.Name, Msg.messageId, Msg.command), string.Format("exception_class=\"{0}\"", e.GetType().Name));
            }
            finally
            {
              if (Replier != null && !string.IsNullOrEmpty(Msg.responsePipeName))
              {
                Logger.WriteLogEntry(this, e,
                  "Error thrown in DispatcherJob.Run method which is serialized back to caller", LogLevel.Warning);
                var exceptionMessage = Responder.ResponseFromException(Msg.messageId, e);
                Replier.send(Msg.responsePipeName, exceptionMessage);
              }
              else
                Logger.WriteLogEntry(this, e,
                  "Error thrown in DispatcherJob.Run method, but there's not caller to serialize error back",
                  LogLevel.Error);
            }
          }
        }
      }
      finally
      {
        try
        {
          _requestProcessingStrategy.RequestComplete(Msg);
        }
        catch (Exception ex)
        {
          Logger.WriteLogEntry(this, ex, "Exception caught from _requestProcessingStrategy.OnRequestComplete()", LogLevel.Error);
        }
        // We will use try finally here because we want to make sure we signal job termination regardless of any exception happening
        if (_producerJobsignaler != null)
          _producerJobsignaler.SignalFinishedJob();
      }
    }

    private void CallDispatchInterfaceMethod(MethodInfo mi, object dispatchInterface, Scope scope)
    {
      object res;
      /* Method that expect a response async receive responder as second parameter */
      var parameters = mi.GetParameters().Length == 2 ? new object[] { Msg, Replier } : new object[] { Msg };
      var reportId = _ps.statsCollector.ReportInit(string.Format("svcbus.produce_elapsed_time,service={0},msg_id={1},command={2}",
        _service.Name, Msg.messageId, mi.Name));
      try
      {
        res = mi.Invoke(dispatchInterface, parameters);
        if (res != null || Msg.responsePipeName == "")
          _ps.statsCollector.ReportCompleteByTimeElapsed(reportId);
        else
          Replier.RegisterAsyncReply(Msg.messageId, reportId);
      }
      catch(Exception)
      {
        _ps.statsCollector.CancelReport(reportId);
        throw;
      }
      if (res == null) return;
      var message = res as Message;
      if (message != null)
      {
        if (string.IsNullOrEmpty(Msg.responsePipeName) || Replier == null)
          return;
        ResponseProcessingStrategy.ProcessResponse(message);
        Replier.send(Msg.responsePipeName, message);
      }
      else
      {
        if (string.IsNullOrEmpty(Msg.responsePipeName) || Replier == null)
          return;
        Replier.send(Msg.responsePipeName, res, Msg.messageId, ResponseProcessingStrategy,
          Msg.Broadcast);
      }
    }

    public bool FirstDisposePassDone { get; set; }
  }
}