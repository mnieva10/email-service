using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Sovos.SvcBus
{
  [Serializable]
  public class PipeServiceException : SvcBusException
  {
    public PipeServiceException(string msg, int code, string sourceMsg) : base(msg, code, sourceMsg) {}

    public new svc_bus_pipeservice_err_t ErrorCode
    {
      get
      {
        return (svc_bus_pipeservice_err_t)base.ErrorCode;
      }
    }
  }

  public class PipeService: IDisposable
  {
    protected static readonly AutoResetEvent ZeroPipeServiceAliveEvent = new AutoResetEvent(true);
    protected static int _instanceCountReachable;
    protected static int _instanceCountAlive;
    private StatsCollector _statsCollector;
    public IntPtr Handle { get; set; }

    protected static void DeallocHandle(IntPtr handle)
    {
      NativeMethods.SvcBusPipeService_destroy(handle);
      NativeMethods.SvcBusPipeService_dealloc(handle);
      if (Interlocked.Decrement(ref _instanceCountAlive) == 0)
        ZeroPipeServiceAliveEvent.Set();
    }

    public static void WaitForZeroPipeServicesAlive() {
        ZeroPipeServiceAliveEvent.WaitOne();
    }

    protected PipeService(){}

    protected void checkPipeServiceResult(int result, string msg)
    {
      if (result != NativeMethods.SERVICE_BUS_OK)
      {
        var e = new PipeServiceException(msg, NativeMethods.SvcBusPipeService_getlastErrorCode(Handle),
          Marshal.PtrToStringAnsi(NativeMethods.SvcBusPipeService_getlastErrorMsg(Handle)));
        GC.KeepAlive(this);
        throw e;
      }
    }

    public void startAsyncWorkPool(uint minThreads, uint maxThreads)
    {
      var res = NativeMethods.SvcBusPipeService_startAsyncWorkPool(Handle, minThreads, maxThreads);
      checkPipeServiceResult(res, "startAsyncWorkPool");
      GC.KeepAlive(this);
    }

    public void stopAsyncWorkPool()
    {
      NativeMethods.SvcBusPipeService_stopAsyncWorkPool(Handle);
      GC.KeepAlive(this);
    }

    public void configureConnectionPoolCleanupTimer(uint timerIntervalMillis, uint maxIdleMillis)
    {
      NativeMethods.SvcBusPipeService_configureConnectionPoolCleanupTimer(Handle, timerIntervalMillis, maxIdleMillis);
      GC.KeepAlive(this);
    }

    ~PipeService()
    {
      Dispose(false);
    }

    private void UnregisterHandle()
    {
      Utils.UnmanagedObjectTracker.Unregister(GetType(), Handle);
      Handle = IntPtr.Zero;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (Handle == IntPtr.Zero) 
        return;
      if (Interlocked.Decrement(ref _instanceCountReachable) > 0 || Utils.RunningOnTestFramework())
      {
        UnregisterHandle();
        return;
      }
      Utils.UnmanagedObjectTracker.StopAgent();
      try
      {
        DeallocHandle(Handle);
      }
      finally
      {
        Handle = IntPtr.Zero;
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public void releaseDeadResponsePipes(int heartbeatMs)
    {
      checkPipeServiceResult(NativeMethods.SvcBusPipeService_releaseDeadResponsePipes(Handle, heartbeatMs), "releaseDeadResponsePipes");
      GC.KeepAlive(this);
    }

    public void dropServicePipe(Service svc)
    {
      checkPipeServiceResult(NativeMethods.SvcBusPipeService_dropServicePipe(Handle, svc.Handle), "dropServicePipe");
      GC.KeepAlive(svc);
      GC.KeepAlive(this);
    }

    public void dropPipe(string pipeName)
    {
      checkPipeServiceResult(NativeMethods.SvcBusPipeService_dropPipe(Handle, pipeName), "dropPipe");
      GC.KeepAlive(this);
    }

    public StatsCollector statsCollector
    {
      get
      {
        if (_statsCollector == null)
          _statsCollector = new StatsCollector(NativeMethods.SvcBusPipeService_getStatsCollector(Handle));
        GC.KeepAlive(this);
        return _statsCollector;
      }
    }
  }

  public class _PipeService : PipeService
  {
    public _PipeService(string connectionString)
    {
      Handle = NativeMethods.SvcBusPipeService_alloc();
      if (NativeMethods.SvcBusPipeService_init(Handle, connectionString) == NativeMethods.SERVICE_BUS_OK)
      {
        Utils.UnmanagedObjectTracker.Register(GetType(), Handle, DeallocHandle);
        ZeroPipeServiceAliveEvent.Reset();
        Interlocked.Increment(ref _instanceCountReachable);
        Interlocked.Increment(ref _instanceCountAlive);
        return;
      }

      var errCode = NativeMethods.SvcBusPipeService_getlastErrorCode(Handle);
      var errMsg = Marshal.PtrToStringAnsi(NativeMethods.SvcBusPipeService_getlastErrorMsg(Handle));
      NativeMethods.SvcBusPipeService_dealloc(Handle);
      Handle = IntPtr.Zero;
      throw new PipeServiceException("constructor", errCode, errMsg);
    }
  }
}
