using System;
using System.Runtime.InteropServices;

namespace Sovos.SvcBus
{
  using SvcBusProducer = IntPtr;
  using SvcBusResponder = IntPtr;
  using bson = IntPtr;

  [Serializable]
  public class ProducerException : SvcBusException
  {
    public ProducerException(string msg, int code, string sourceMsg) : base(msg, code, sourceMsg) {}

    new public svc_bus_producer_err_t ErrorCode
    {
      get
      {
        return (svc_bus_producer_err_t)base.ErrorCode;
      }
    }
  }

  [Serializable]
  public class PeekObjectNotFoundException : Exception {}

  public interface IProducer
  {
    Message wait();
    bool take(Message msg);
    Message waitAndTake();
  }

  public class Producer : IDisposable, IProducer
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
    protected SvcBusProducer _producer;
    private Responder _responder;

    protected Producer(){}

    public uint RequestTimeout {
      set 
      {
        checkProducerResult(NativeMethods.SvcBusProducer_setRequestPipeTimeout(_producer, value), "set_RequestTimeout");
        GC.KeepAlive(this);
      }
    }

    protected void checkProducerResult(int result, string msg)
    {
      if (result != NativeMethods.SERVICE_BUS_OK)
      {
        raiseException(msg);
      }
    }

    private void raiseException(string msg)
    {
      var e = new ProducerException(msg, NativeMethods.SvcBusProducer_getlastErrorCode(_producer), Marshal.PtrToStringAnsi(NativeMethods.SvcBusProducer_getlastErrorMsg(_producer)));
      GC.KeepAlive(this);
      throw e;
    }

    ~Producer()
    {
      Dispose(false); 
    }

    protected virtual void Dispose(bool disposing)
    {
      if (_producer == SvcBusProducer.Zero) return;
      Utils.UnmanagedObjectTracker.Unregister(GetType(), _producer);
      _producer = SvcBusProducer.Zero;
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public Responder responder
    {
      get {
        var result = _responder ?? (_responder = new _Responder(NativeMethods.SvcBusProducer_getResponder(_producer)));
        GC.KeepAlive(this);
        return result;
      }
    }

    public bool take(Message msg)
    {
      var result = NativeMethods.SvcBusProducer_take(_producer, msg.Handle) == NativeMethods.SERVICE_BUS_OK;
      GC.KeepAlive(msg);
      GC.KeepAlive(this);
      return result;
    }

    public bool takeBy_id(Oid _id)
    {
      var result = NativeMethods.SvcBusProducer_take_by__id(_producer, _id.oid) == NativeMethods.SERVICE_BUS_OK;
      GC.KeepAlive(_id);
      GC.KeepAlive(this);
      return result;
    }

    public bool poke(Oid id, Bson data)
    {
      int res = NativeMethods.SvcBusProducer_poke(_producer, id.oid, data.Handle);
      GC.KeepAlive(id);
      GC.KeepAlive(data);
      if (res == NativeMethods.SERVICE_BUS_ERROR) // error occured
        raiseException("poke");
      GC.KeepAlive(this);
      return res == NativeMethods.SERVICE_BUS_OK; // success
    }

    public void remove(Oid id)
    {
      var res = NativeMethods.SvcBusProducer_remove(_producer, id.oid);
      GC.KeepAlive(id);
      if (res == NativeMethods.SERVICE_BUS_ERROR) // error occured
        raiseException("remove");
      GC.KeepAlive(this);
    }

    public Message wait()
    {
      var result = Builder.newMessage(Message.InitMode.blank);
      checkProducerResult(NativeMethods.SvcBusProducer_wait(_producer, result.Handle), "wait");
      GC.KeepAlive(this);
      return result;
    }

    public Bson peek(Oid _id)
    {
      var b = Builder.newBson(Bson.DisposalMethod.CallBsonFree);
      try
      {
        checkProducerResult(NativeMethods.SvcBusProducer_peek(_producer, b.Handle, _id.oid), "peek");
        GC.KeepAlive(_id);
        GC.KeepAlive(this);
      }
      catch (ProducerException e)
      {
        if (e.ErrorCode != 0) throw;
        throw new PeekObjectNotFoundException();
      }
      return b;
    }

    public Message waitAndTake()
    {
      var result = Builder.newMessage(Message.InitMode.blank);
      checkProducerResult(NativeMethods.SvcBusProducer_waitAndTake(_producer, result.Handle), "waitAndTake");
      GC.KeepAlive(this);
      return result;
    }
  }

  public class _Producer : Producer
  {
    public _Producer(PipeService ps, string name, Service svc)
    {
      _producer = NativeMethods.SvcBusProducer_alloc();
      checkProducerResult(NativeMethods.SvcBusProducer_init(_producer, ps.Handle, name, svc.Handle), "constructor");
      Utils.UnmanagedObjectTracker.Register(GetType(), _producer, Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusProducer_destroy) +
                                            Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusProducer_dealloc),
                                            new SvcBusHandles
                                            {
                                              { ps.GetType(), ps.Handle },
                                              { svc.GetType(), svc.Handle }
                                            });
      GC.KeepAlive(svc);
      GC.KeepAlive(ps);
    }
  }
}
