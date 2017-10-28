using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Sovos.SvcBus
{
  internal class ProducerJob : IDisposable, IReplier
  {
    volatile bool _shouldStop;
    private readonly WinApiThreadPool.ThreadPool _threadPool;
    private readonly ConcurrentQueue<RespondJob> _respondQueue;
    private readonly AutoResetEvent _responseJobInQueue;
    private readonly Queue<Message> _yieldedMessages = new Queue<Message>();
    private readonly Dispatcher _dispatcher;
    public bool ReplySingleThreadedMode { get; set; }
    public ManualResetEvent DoneEvent { get; private set; }
    public DispatchMode DispatchInterfaceMode { get; set; }
    public Type DispatchInterfaceClass { get; set; }
    public Producer Producer { get; set; }
    public PipeService PipeService { get; set; }
    public IRequestProcessingStrategy RequestProcessingStrategy { get; set; }
    public IResponseProcessingStrategy ResponseProcessingStrategy { get; set; }
    public Service Service { get; set; }
    private readonly DefaultProducerJobSignaler _producerJobsSignaler;
    private readonly ConcurrentDictionary<Oid, Oid> _asyncReplyMap = new ConcurrentDictionary<Oid, Oid>(); 
    public ILogger Logger { get; set; }

    public ProducerJob(Dispatcher dispatcher,
      uint minThreadCount,
      uint maxThreadCount)
    {
      _dispatcher = dispatcher;
      _shouldStop = false;
      DoneEvent = new ManualResetEvent(false);
      _producerJobsSignaler = new DefaultProducerJobSignaler();
      _threadPool = new WinApiThreadPool.ThreadPool(minThreadCount, maxThreadCount);
      _respondQueue = new ConcurrentQueue<RespondJob>();
      _responseJobInQueue = new AutoResetEvent(false);
      _producerJobsSignaler.MaxJobsCount = Convert.ToInt32(maxThreadCount);
      ReplySingleThreadedMode = true;
      Logger = new SimpleConsoleLogger();
    }

    private Message TakeYielded()
    {
      while (_yieldedMessages.Count > 0)
      {
        var msg = _yieldedMessages.Dequeue();
        if (RequestProcessingStrategy.TakeMessage(msg))
          return msg;
      }
      return null;
    }

    public void send(string responsePipeName, Message msg)
    {
      Oid reportId;
      if (_asyncReplyMap.TryRemove(msg.messageId, out reportId))
        PipeService.statsCollector.ReportCompleteByTimeElapsed(reportId);
      if (ReplySingleThreadedMode)
      {
        _respondQueue.Enqueue(new RespondJob() {responsePipe = responsePipeName, message = msg});
        _responseJobInQueue.Set();
      }
      else
        Producer.responder.send(responsePipeName, msg);
    }

    public void send(string responsePipeName, object message,
      Oid msgId, IResponseProcessingStrategy responseProcessingStrategy, bool isBroadcast)
    {
      var response = Builder.newMessage(msgId);
      response.Broadcast = isBroadcast;

      var serializer = Builder.newBsonSerializer(message.GetType(), message, response.bson);
      serializer.Serialize(DispatcherConstants.Response);
      if (responseProcessingStrategy != null) 
        responseProcessingStrategy.ProcessResponse(response);
      send(responsePipeName, response);
    }

    public void RegisterAsyncReply(Oid messageId, Oid statReportId)
    {
      _asyncReplyMap.TryAdd(messageId, statReportId);
    }

    public void ResponseFromException(Exception e, Bson bson)
    {
      Responder.ResponseFromException(e, bson);
    }

    public Message ResponseFromException(Oid msgId, Exception e)
    {
      return Responder.ResponseFromException(msgId, e);
    }

    private void ProcessRespondQueue()
    {
      while (!_shouldStop)
      {
        RespondJob respondJob;
        if (!_respondQueue.TryDequeue(out respondJob))
        {
          _responseJobInQueue.WaitOne();
          continue;
        }
        Producer.responder.send(respondJob.responsePipe, respondJob.message);
      }
    }

    private static readonly WinApiThreadPool.ThreadWorkDelegate DispatchJobDelegate = DispatchJobCallback;
    private static void DispatchJobCallback(object userdata)
    {
      var dispatchJob = (DispatchJob)userdata;
      dispatchJob.Run(null);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public void Run(object userdata)
    {
      const uint cycleTimeout = 3500;
      if (ReplySingleThreadedMode)
      {
        var processRespondQueue = new Thread(ProcessRespondQueue);
        processRespondQueue.Start();
      }
      using (var scope = new Scope())
      {
        scope.add(Producer);
        Producer.RequestTimeout = cycleTimeout;
        RequestProcessingStrategy.Producer = Producer;

        var type = (DispatchInterfaceMode == DispatchMode.SingletonDispatchInterface)
          ? typeof(DispatchInterfaceSingletonAccessor)
          : typeof(DispatchInterfaceMultipleInstanceAccessor);

        using (var dispIntAccessor = (DispatchInterfaceAccessor)Activator.CreateInstance(type, DispatchInterfaceClass))
        {

          try
          {
            while (!_shouldStop)
            {
              try
              {
                Message request;
                while (_producerJobsSignaler.WaitUntilWorkerAvailable(YieldTimeout) &&
                       !_shouldStop &&
                       (request = TakeYielded()) != null)
                {
                  var job = PrepareDispathJob(userdata, request, dispIntAccessor, _producerJobsSignaler);

                  _producerJobsSignaler.NotifyNewJobCreated();
                  _threadPool.QueueUserWorkItem(DispatchJobDelegate, job);
                }

                if (_shouldStop)
                  break;

                request = RequestProcessingStrategy.GetMessage();
                if (request == null) 
                  continue;
                var it = request.bson.find(DispatcherConstants.Priority);
                if (it != null && it.bsonType == bson_type.BSON_BOOL && (bool) it &&
                    RequestProcessingStrategy.TakeMessage(request))
                {
                  var job = PrepareDispathJob(userdata, Builder.newMessage(request), dispIntAccessor, null);
                  ThreadPool.QueueUserWorkItem(state => job.Run(null));
                }
                else
                  _yieldedMessages.Enqueue(Builder.newMessage(request));

              }
              catch (ProducerException e)
              {
                // Only accepted exception is SERVICE_BUS_PRODUCER_WAIT_EXCEEDED
                // Any other exception will kill the ProducerJob
                if (e.ErrorCode != svc_bus_producer_err_t.SERVICE_BUS_PRODUCER_WAIT_EXCEEDED)
                {
                  Logger.WriteLogEntry(this, e, "Service bus error other than TIMEOUT_EXCEEDED thrown in ProducerJob.Run method", LogLevel.Fatal);
                  throw;
                }
              }
            }
          }
          finally
          {
            DoneEvent.Set();
            _dispatcher.NotifyProducerJobDone();
          }
        }
      }
      DoneEvent.Set();
    }

    private DispatchJob PrepareDispathJob(object userdata, Message request, DispatchInterfaceAccessor dispIntAccessor, 
                                          IProducerJobSignaler signalizer)
    {
      RequestProcessingStrategy.Process(request);

      return new DispatchJob(userdata, signalizer, RequestProcessingStrategy, PipeService, Service)
      {
        Msg = request,
        ResponseProcessingStrategy = ResponseProcessingStrategy,
        DispatchInterfaceAccessor = dispIntAccessor,
        Replier = !string.IsNullOrEmpty(request.responsePipeName) ? this : null,
        Logger = Logger
      };
    }

    public void Cancel()
    {
      _shouldStop = true;
      _responseJobInQueue.Set();
    }

    ~ProducerJob()
    {
      Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing) return;
      Cancel();
      _threadPool.Dispose();
      _producerJobsSignaler.Dispose();
      _responseJobInQueue.Dispose();
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public int YieldTimeout { get; set; }
  }
}