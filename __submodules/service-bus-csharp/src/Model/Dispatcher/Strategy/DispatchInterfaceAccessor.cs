using System;

namespace Sovos.SvcBus
{
  internal abstract class DispatchInterfaceAccessor : IDisposable
  {
    protected Type DispatchInterfaceClass { get; set; }
    protected readonly object syncLock = new object();

    protected DispatchInterfaceAccessor(Type dispatchInterfaceClass)
    {
      DispatchInterfaceClass = dispatchInterfaceClass;
    }

    ~DispatchInterfaceAccessor()
    {
      Dispose(false);
    }

    public abstract DispatchInterface AcquireDispatchInterface(object userData);
    public abstract void ReleaseDispatchInterface(DispatchInterface dispatchInterface);

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    public abstract void Dispose(bool disposing);
  }
}