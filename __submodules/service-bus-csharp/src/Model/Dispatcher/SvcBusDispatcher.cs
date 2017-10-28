using System;

namespace Sovos.SvcBus
{
  public enum DispatchMode
  {
    SingletonDispatchInterface,
    MultiInstanceDispatchInterface
  }

  public class Dispatcher : IDisposable
  {
    public const int DEFAULT_DISPATCHER_YIELD_TIMEOUT = 4 * 1000;
    private readonly string _name;
    private readonly Service _service;
    private readonly PipeService _pipeService;
    private ProducerJob _producerJob;
    private readonly Type _dispatchInterfaceClass;
    private IRequestProcessingStrategy _requestProcessingStrategy;
    private IResponseProcessingStrategy _responseProcessingStrategy;
    private readonly Scope _scope;
    private readonly WinApiThreadPool.ThreadPool _producerPool;
    public bool ReplySingleThreadedMode { get; set; }
    public ILogger Logger { get; set; }

    public Dispatcher(string name, Service svc, PipeService ps, Type dispatchInterfaceClass)
    {
      _scope = new Scope();
      _producerPool = new WinApiThreadPool.ThreadPool(1, 1);
      var persistence = Builder.newServicePersistence(ps);
      try
      {
        _service = persistence.Load(svc.Name);
      }
      catch (ServicePersistenceException e)
      {
        if (e.ErrorCode != service_persistence_err.SERVICE_NOT_FOUND)
          throw;
        persistence.Save(svc);
        _service = svc;
      }
      _name = name;
      _pipeService = ps;
      _dispatchInterfaceClass = dispatchInterfaceClass;
      YieldTimeout = DEFAULT_DISPATCHER_YIELD_TIMEOUT;
      ReplySingleThreadedMode = true;
    }

    ~Dispatcher()
    {
      Dispose(false);
    }
    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing || _disposed) return;
      try
      {
        if (_producerJob != null)
        {
          _producerJob.Cancel();
          _producerJob.DoneEvent.WaitOne();
          _producerJob.Dispose();
        }
        _scope.Dispose();
        _producerPool.Dispose();
      }
      finally
      {
        _disposed = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public void Start(uint threadCount, DispatchMode dispatchInterfaceMode, object userData = null)
    {
      if (_producerJob != null)
        return; // Already started

      if (Logger == null)
        throw new Exception("Logger required to be initialzied");
      if (threadCount < 1)
        throw new ThreadCountException();
      if (_service == null)
        throw new ServiceIsNullException();
      if (_pipeService == null)
        throw new PipeServiceIsNullException();
      if (_name == String.Empty)
        throw new ServiceNameException();

      _pipeService.releaseDeadResponsePipes(NativeMethods.SERVICE_BUS_DEFAULT_HEARTBEAT_LIMIT_MS);

      _producerJob = new ProducerJob(this, 1, threadCount);
      try
      {
        _producerJob.PipeService = _pipeService;
        _producerJob.Producer = Builder.newProducer(_pipeService, _name, _service);
        _producerJob.DispatchInterfaceClass = _dispatchInterfaceClass;
        _producerJob.DispatchInterfaceMode = dispatchInterfaceMode;
        _producerJob.RequestProcessingStrategy = RequestProcessingStrategy;
        _producerJob.Service = _service;
        _producerJob.ResponseProcessingStrategy = ResponseProcessingStrategy;
        _producerJob.YieldTimeout = YieldTimeout;
        _producerJob.ReplySingleThreadedMode = ReplySingleThreadedMode;
        _producerJob.Logger = Logger;
      }
      catch
      {
        _producerJob.Dispose();
        throw;
      }
      _producerPool.QueueUserWorkItem(_producerJob.Run, userData);
      ProducerJobRunning = true;
    }

    internal void NotifyProducerJobDone()
    {
      ProducerJobRunning = false;
    }

    public bool ProducerJobRunning { get; private set; }

    public IRequestProcessingStrategy RequestProcessingStrategy
    {
      get { return _requestProcessingStrategy ?? (_requestProcessingStrategy = new ChainableRequestProcessingStrategy()); }
      set { _requestProcessingStrategy = value; }
    }
    public IResponseProcessingStrategy ResponseProcessingStrategy
    {
      get { return _responseProcessingStrategy ?? (_responseProcessingStrategy = new DefaultResponseProcessingStrategy()); }
      set { _responseProcessingStrategy = value; }
    }

    public PipeService pipeService
    {
      get
      {
        return _pipeService;
      }
    }

    public Service service
    {
      get
      {
        return _service;
      }
    }

    public int YieldTimeout { get; set; }
  }
}