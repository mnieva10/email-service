using System;

namespace Sovos.SvcBus
{
  using SvcBusMutex = IntPtr;
  using bson = IntPtr;
  using System.Runtime.InteropServices;

  public class Mutex: IDisposable
  {
    public SvcBusMutex Handle { get; protected set; }

    protected Mutex() {}

    ~Mutex()
    {
      Dispose(false);
    }

    protected void RegisterHandle(PipeService ps)
    {
      Utils.UnmanagedObjectTracker.Register(GetType(), Handle, Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusMutex_destroy) +
                                            Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusMutex_dealloc),
                                            new SvcBusHandles { { ps.GetType(), ps.Handle } });  
      GC.KeepAlive(ps);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (Handle == SvcBusMutex.Zero) 
        return;
      Utils.UnmanagedObjectTracker.Unregister(GetType(), Handle);
      Handle = SvcBusMutex.Zero;
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected void RaiseException(string msg)
    {
      var e = new MutexException(msg, NativeMethods.SvcBusMutex_getlastErrorCode(Handle),
                                    Marshal.PtrToStringAnsi(NativeMethods.SvcBusMutex_getlastErrorMsg(Handle)));
      GC.KeepAlive(this);
      throw e;
    }

    private void CheckException(svc_bus_mutex_operation_result_t res, string msg)
    {
      if (res == svc_bus_mutex_operation_result_t.SERVICE_BUS_MUTEX_OPERATION_ERROR)
        RaiseException(msg);
    }

    private void CheckException(string msg)
    {
      if ((svc_bus_mutex_err_t)NativeMethods.SvcBusMutex_getlastErrorCode(Handle) != svc_bus_mutex_err_t.SERVICE_BUS_MUTEX_OK)
        RaiseException(msg);
    }

    public bool acquire()
    {
      var res = NativeMethods.SvcBusMutex_acquire(Handle);
      CheckException(res, "calling acquire()");
      GC.KeepAlive(this);
      return res == svc_bus_mutex_operation_result_t.SERVICE_BUS_MUTEX_OPERATION_SUCCESS;
    }

    public bool release()
    {
      var res = NativeMethods.SvcBusMutex_release(Handle);
      CheckException(res, "calling release()");
      GC.KeepAlive(this);
      return res == svc_bus_mutex_operation_result_t.SERVICE_BUS_MUTEX_OPERATION_SUCCESS;
    }

    public bool forceRelease(string computerName, string procName, int procId)
    {
      var res = NativeMethods.SvcBusMutex_forceRelease(Handle, computerName, procName, procId);
      CheckException(res, "calling forceRelease()");
      GC.KeepAlive(this);
      return res == svc_bus_mutex_operation_result_t.SERVICE_BUS_MUTEX_OPERATION_SUCCESS;
    }

    public void remove()
    {
      if (NativeMethods.SvcBusMutex_remove(Handle) != NativeMethods.SERVICE_BUS_OK)
        RaiseException("calling remove()");
      GC.KeepAlive(this);
    }

    public void lockedMutexQueryInit()
    {
      if (NativeMethods.SvcBusMutex_lockedMutexQueryInit(Handle) != NativeMethods.SERVICE_BUS_OK)
        RaiseException("calling lockedMutexQueryInit()");
      GC.KeepAlive(this);
    }

    public void lockedMutexQueryDone()
    {
      NativeMethods.SvcBusMutex_lockedMutexQueryDone(Handle);
      GC.KeepAlive(this);
    }

    public bool lockedMutexQueryNext(out Bson curr)
    {
      bson _curr;
      var res = NativeMethods.SvcBusMutex_lockedMutexQueryNext(Handle, out _curr);
      CheckException("calling lockedMutexQueryNext()");
      curr = res ? Builder.newBson(_curr) : null;
      GC.KeepAlive(this);
      return res;
    }
  }

  [Serializable]
  public class MutexException : SvcBusException
  {
    public MutexException(string msg, int code, string sourceMsg) : base(msg, code, sourceMsg) { }

    public new svc_bus_mutex_err_t ErrorCode
    {
      get { return (svc_bus_mutex_err_t)base.ErrorCode; }
    }
  }

  internal class _Mutex : Mutex
  {
    public _Mutex(PipeService ps, string name, string svcname)
    {
      Handle = NativeMethods.SvcBusMutex_alloc();
      if (NativeMethods.SvcBusMutex_init(Handle, ps.Handle, name, svcname) !=
          NativeMethods.SERVICE_BUS_OK)
        RaiseException("Failed to create named Mutex object");
      RegisterHandle(ps);
    }

    public _Mutex(PipeService ps)
    {
      Handle = NativeMethods.SvcBusMutex_alloc();
      if (NativeMethods.SvcBusMutex_init(Handle, ps.Handle, "", "") !=
          NativeMethods.SERVICE_BUS_OK)
        RaiseException("Failed to create anonymous Mutex object");
      RegisterHandle(ps);
    }
  }
}
