using System;

namespace Sovos.SvcBus
{
  internal class DispatchInterfaceSingletonAccessor : DispatchInterfaceAccessor
  {
    DispatchInterface _dispatchInterface;

    public DispatchInterfaceSingletonAccessor(Type dispatchInterfaceClass)
      : base(dispatchInterfaceClass) { }

    public override DispatchInterface AcquireDispatchInterface(object userData)
    {
      lock (syncLock)
      {
        return _dispatchInterface ?? (_dispatchInterface = (DispatchInterface)Activator.CreateInstance(DispatchInterfaceClass, userData));
      }
    }

    public override void ReleaseDispatchInterface(DispatchInterface dispatchInterface) { }

    public override void Dispose(bool disposing)
    {
      if (!disposing) return;
      var disposableDispatchInterface = _dispatchInterface as IDisposable;
      if (disposableDispatchInterface != null)
        disposableDispatchInterface.Dispose();
    }
  }
}