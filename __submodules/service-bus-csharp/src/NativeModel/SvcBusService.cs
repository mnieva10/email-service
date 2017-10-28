using System;
using System.Runtime.InteropServices;

namespace Sovos.SvcBus
{
  public class Service
  {
    public IntPtr Handle { get; protected set; }

    protected Service() {}

    public string Name
    {
      get
      {
        var result = Marshal.PtrToStringAnsi(NativeMethods.SvcBusService_getName(Handle));
        GC.KeepAlive(this);
        return result;
      }
    }

    public svc_bus_queue_mode_t Mode
    {
      get
      {
        var result = (svc_bus_queue_mode_t)NativeMethods.SvcBusService_getMode(Handle);
        GC.KeepAlive(this);
        return result;
      }
    }

    public uint VolatileSize
    {
      get
      {
        var result = NativeMethods.SvcBusService_getvolatileSize(Handle);
        GC.KeepAlive(this);
        return result;
      }
    }

    public uint ResponsePipeSize
    {
      get
      {
        var result = NativeMethods.SvcBusService_getresponsePipeSize(Handle);
        GC.KeepAlive(this);
        return result;
      }
    }

    protected void RegisterHandle()
    {
      Utils.UnmanagedObjectTracker.Register(GetType(), Handle, 
                                            Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusService_destroy) + 
                                            Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusService_dealloc));  
    }

    ~Service()
    {
      if (Handle == IntPtr.Zero)
        return;
      Utils.UnmanagedObjectTracker.Unregister(GetType(), Handle);
      Handle = IntPtr.Zero;
    }
  }

  [Serializable]
  public class ServiceException : SvcBusException
  {
    public ServiceException(string msg, int code, string sourceMsg) : base(msg, code, sourceMsg) { }

    public new service_err ErrorCode
    {
      get { return (service_err)base.ErrorCode; }
    }
  }

  public class _Service : Service
  {
    public _Service(string name, svc_bus_queue_mode_t mode, uint volatileSize = 0, uint responsePipeSize = 0)
    {
      Handle = NativeMethods.SvcBusService_alloc();
      var inited = NativeMethods.SvcBusService_init(Handle, name, (int) mode, volatileSize, responsePipeSize);
      if (inited == NativeMethods.SERVICE_BUS_OK)
      {
        RegisterHandle();
        return;
      }

      var errCode = NativeMethods.SvcBusService_getlastErrorCode(Handle);
      var errMsg = Marshal.PtrToStringAnsi(NativeMethods.SvcBusService_getlastErrorMsg(Handle));
      NativeMethods.SvcBusService_dealloc(Handle);
      Handle = IntPtr.Zero;
      throw new ServiceException("constructor", errCode, errMsg);
    }

    public _Service(IntPtr handle)
    {
      Handle = handle;
      RegisterHandle();
    }
  }

  [Serializable]
  public class ServicePersistenceException : SvcBusException
  {
    public ServicePersistenceException(string msg, int code, string sourceMsg) : base(msg, code, sourceMsg) { }

    public new service_persistence_err ErrorCode
    {
      get
      {
        return (service_persistence_err)base.ErrorCode;
      }
    }
  }

  public class ServicePersistence
  {
    public IntPtr Handle { get; protected set; }

    protected void RaiseException(string msg)
    {
      var errCode = NativeMethods.SvcBusServicePersistence_getlastErrorCode(Handle);
      var errMsg = Marshal.PtrToStringAnsi(NativeMethods.SvcBusServicePersistence_getlastErrorMsg(Handle));
      GC.KeepAlive(this);
      throw new ServicePersistenceException(msg, errCode, errMsg);
    }

    public Service Load(string serviceName)
    {
      var serviceHandle = NativeMethods.SvcBusService_alloc();
      if (NativeMethods.SvcBusServicePersistence_load(Handle, serviceHandle, serviceName) != NativeMethods.SERVICE_BUS_OK)
      {
        NativeMethods.SvcBusService_dealloc(serviceHandle);
        RaiseException("Load");
      }
      GC.KeepAlive(this);
      return Builder.newService(serviceHandle);
    }

    public void Save(Service service)
    {
      if (NativeMethods.SvcBusServicePersistence_save(Handle, service.Handle) != NativeMethods.SERVICE_BUS_OK)
        RaiseException("Save");
      GC.KeepAlive(service);
      GC.KeepAlive(this);
    }

    public bool Exists(string serviceName)
    {
      var result = NativeMethods.SvcBusServicePersistence_exists(Handle, serviceName) == NativeMethods.SERVICE_BUS_OK;
      GC.KeepAlive(this);
      return result;
    }

    public void Remove(string serviceName)
    {
      if (NativeMethods.SvcBusServicePersistence_remove(Handle, serviceName) != NativeMethods.SERVICE_BUS_OK)
        RaiseException("Remove");
      GC.KeepAlive(this);
    }

    ~ServicePersistence()
    {
      if (Handle == IntPtr.Zero)
        return;
      Utils.UnmanagedObjectTracker.Unregister(GetType(), Handle);
      Handle = IntPtr.Zero;
    }
  }

  internal class _ServicePersistence : ServicePersistence
  {
    public _ServicePersistence(PipeService ps)
    {
      Handle = NativeMethods.SvcBusServicePersistence_alloc();
      if (NativeMethods.SvcBusServicePersistence_init(Handle, ps.Handle) == NativeMethods.SERVICE_BUS_OK)
      {
        Utils.UnmanagedObjectTracker.Register(GetType(), Handle, Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusServicePersistence_destroy) +
                                              Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusServicePersistence_dealloc), 
                                              new SvcBusHandles
                                                {
                                                  { ps.GetType(), ps.Handle }
                                                });
        GC.KeepAlive(ps);
        return;
      }

      var errCode = NativeMethods.SvcBusServicePersistence_getlastErrorCode(Handle);
      var errMsg = Marshal.PtrToStringAnsi(NativeMethods.SvcBusServicePersistence_getlastErrorMsg(Handle));
      NativeMethods.SvcBusServicePersistence_dealloc(Handle);
      Handle = IntPtr.Zero;
      throw new ServicePersistenceException("constructor", errCode, errMsg);
    }
  }
}
