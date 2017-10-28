using System;
using System.Collections.Generic;

namespace Sovos.SvcBus
{
  internal class DispatchInterfaceMultipleInstanceAccessor : DispatchInterfaceAccessor
  {
    readonly LinkedList<DispatchInterface> _dispatchInterfaces;

    public DispatchInterfaceMultipleInstanceAccessor(Type dispatchInterfaceClass)
      : base(dispatchInterfaceClass)
    {
      _dispatchInterfaces = new LinkedList<DispatchInterface>();
    }

    public override DispatchInterface AcquireDispatchInterface(object userData)
    {
      lock (syncLock)
      {
        // not using try..finally in purpose here for performance reasons...
        // we will rely that getting element from list and removing node won't throw an exception
        if (_dispatchInterfaces.Count == 0)
          return (DispatchInterface)Activator.CreateInstance(DispatchInterfaceClass, userData);
        var res = _dispatchInterfaces.First.Value;
        _dispatchInterfaces.RemoveFirst();
        return res;
      }
    }

    public override void ReleaseDispatchInterface(DispatchInterface dispatchInterface)
    {
      // not using try..finally in purpose here for performance reasons...
      // we will rely that inserting back into a linked list will not throw an exception
      lock (syncLock)
      {
        _dispatchInterfaces.AddFirst(dispatchInterface);
      }
    }

    public override void Dispose(bool disposing)
    {
      if (!disposing) 
        return;
      lock (syncLock)
      {
        foreach (var dispatchInterface in _dispatchInterfaces)
        {
          var disposableDispatchInterface = dispatchInterface as IDisposable;
          if (disposableDispatchInterface != null)
          {
            disposableDispatchInterface.Dispose();
          }
        }
      }
    }
  }
}