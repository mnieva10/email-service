using System;

namespace Sovos.SvcBus
{
  using SvcBusConsumerPool = IntPtr;
  using SvcBusGenericErr = IntPtr;
  using SvcBusConsumer = IntPtr;
  using SvcBusConsumerPoolDictionary = IntPtr;

  [Serializable]
  public class ConsumerPoolException : SvcBusException
  {
    public ConsumerPoolException(string msg, int code, string sourceMsg) : base(msg, code, sourceMsg) { }

    public new consumer_pool_err ErrorCode
    {
      get { return (consumer_pool_err)base.ErrorCode; }
    }
  }

  public class ConsumerPool : IDisposable
  {
    public SvcBusConsumerPool Handle { get; protected set; }
    private ILogger _logger = new SimpleConsoleLogger();
    public ILogger Logger
    {
      get
      {
        return _logger;
      }
      set
      {
        if (value != null)
          _logger = value;
      }
    }

    public GenericErr GenericErr
    {
      get
      {
        var result = new GenericErr(NativeMethods.SvcBusConsumerPool_getErr(Handle));
        GC.KeepAlive(this);
        return result;
      }
    }

    public Consumer Acquire()
    {
      var err = Builder.newGenericErr();
      var consumerHandle = NativeMethods.SvcBusConsumerPool_acquire(Handle, err.Handle);
      if (consumerHandle == SvcBusConsumer.Zero)
        throw new ConsumerPoolException("acquire", err.ErrCode, err.ErrMsg);

      var consumer = Builder.newConsumer(this, consumerHandle);
      consumer.Logger = Logger;

      return consumer;
    }

    public void Release(Consumer consumer)
    {
      NativeMethods.SvcBusConsumerPool_release(Handle, consumer.Handle);
      consumer.NotifyReleased();
      GC.KeepAlive(this);
    }

    public uint AutoStartHeartbeatTimer
    {
      set
      {
        NativeMethods.SvcBusConsumerPool_autoStartHeartbeatTimer(Handle, value);
        GC.KeepAlive(this);
      }
    }

    public bool CopyConsumers
    {
      set
      {
        NativeMethods.SvcBusConsumerPool_setCopy(Handle, Convert.ToByte(value));
        GC.KeepAlive(this);
      }
    }

    ~ConsumerPool()
    {
      Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (Handle == SvcBusConsumerPool.Zero) 
        return;
      Utils.UnmanagedObjectTracker.Unregister(GetType(), Handle);
      Handle = SvcBusConsumerPool.Zero;
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public void startAsyncWorkPool(uint minThreads, uint maxThreads)
    {
      if (NativeMethods.SvcBusConsumerPool_startAsyncWorkPool(Handle, minThreads, maxThreads) !=
          NativeMethods.SERVICE_BUS_OK)
        throw new ConsumerPoolException("startAsyncWorkPool", NativeMethods.SERVICE_BUS_ERROR, "Failed to initialize PipeService's ConsumerPool threadpool");
      GC.KeepAlive(this);
    }

    public void stopAsyncWorkPool()
    {
      NativeMethods.SvcBusConsumerPool_stopAsyncWorkPool(Handle);
      GC.KeepAlive(this);
    }

    public void configureConnectionPoolCleanupTimer(uint timerIntervalMillis, uint maxIdleMillis)
    {
      if (
        NativeMethods.SvcBusConsumerPool_configureConsumerPoolCleanupTimer(Handle, timerIntervalMillis, maxIdleMillis) !=
        NativeMethods.SERVICE_BUS_OK)
        throw new ConsumerPoolException("configureConnectionPoolCleanupTimer", NativeMethods.SERVICE_BUS_ERROR, "Failed to configure consumer pool cleaner timer");
      GC.KeepAlive(this);
    }
  }

  internal class _ConsumerPool : ConsumerPool
  {
    public _ConsumerPool(PipeService ps, string name, Service service, int options = 0)
    {
      Handle = NativeMethods.SvcBusConsumerPool_alloc();

      if (NativeMethods.SvcBusConsumerPool_init(Handle, ps.Handle, name, service.Handle, options) != NativeMethods.SERVICE_BUS_OK)
      {
        var e = new ConsumerPoolException("init", GenericErr.ErrCode, GenericErr.ErrMsg);
        NativeMethods.SvcBusConsumerPool_dealloc(Handle);
        Handle = SvcBusConsumerPool.Zero;
        throw e;
      }
      Utils.UnmanagedObjectTracker.Register(GetType(), Handle, Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusConsumerPool_destroy) +
                                            Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusConsumerPool_dealloc),
                                            new SvcBusHandles
                                            {
                                              { ps.GetType(), ps.Handle }, 
                                              { service.GetType(), service.Handle }
                                            });
    }

    public _ConsumerPool(SvcBusConsumerPool consumerPoolHandle)
    {
      Handle = consumerPoolHandle;
      Utils.UnmanagedObjectTracker.Register(GetType(), Handle);
    }
  }

  [Serializable]
  public class ConsumerPoolDictionaryException : SvcBusException
  {
    public ConsumerPoolDictionaryException(string msg, int code, string sourceMsg) : base(msg, code, sourceMsg) { }

    public new consumer_pool_dictionary_err ErrorCode
    {
      get
      {
        return (consumer_pool_dictionary_err)base.ErrorCode;
      }
    }
  }

  public class ConsumerPoolDictionary : IDisposable
  {
    public SvcBusConsumerPoolDictionary Handle { get; protected set; }

    public GenericErr GenericErr
    {
      get
      {
        var result = new GenericErr(NativeMethods.SvcBusConsumerPoolDictionary_getErr(Handle));
        GC.KeepAlive(this);
        return result;
      }
    }

    protected ConsumerPoolDictionary() { }

    public ConsumerPool GetConsumerPool(string connectionString, string serviceName, string consumerName, svc_bus_consumer_options_t options)
    {
      var err = Builder.newGenericErr();
      var poolHandle = NativeMethods.SvcBusConsumerPoolDictionary_getConsumerPool(Handle, connectionString, serviceName, 
                                                                                  consumerName, (int)options, err.Handle);
      if (poolHandle == SvcBusConsumer.Zero)
        throw new ConsumerPoolException("getConsumerPool", err.ErrCode, err.ErrMsg);
      GC.KeepAlive(this);
      return Builder.newConsumerPool(poolHandle);
    }

    ~ConsumerPoolDictionary()
    {
      Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (Handle == SvcBusConsumerPool.Zero) 
        return;
      NativeMethods.SvcBusConsumerPoolDictionary_destroy(Handle);
      NativeMethods.SvcBusConsumerPoolDictionary_dealloc(Handle);
      Handle = SvcBusConsumerPool.Zero;
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
  }

  internal class _ConsumerPoolDictionary : ConsumerPoolDictionary
  {
    public _ConsumerPoolDictionary()
    {
      Handle = NativeMethods.SvcBusConsumerPoolDictionary_alloc();
      if (NativeMethods.SvcBusConsumerPoolDictionary_init(Handle) != NativeMethods.SERVICE_BUS_OK)
      {
        var e = new ConsumerPoolDictionaryException("init", GenericErr.ErrCode, GenericErr.ErrMsg);
        NativeMethods.SvcBusConsumerPoolDictionary_dealloc(Handle);
        Handle = SvcBusConsumerPool.Zero;
        throw e;
      }
    }
  }
}