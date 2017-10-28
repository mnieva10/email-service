using System;
using System.Runtime.InteropServices;

namespace Sovos.SvcBus
{
  using SvcBusMessage = IntPtr;
  using bson_oid_p = IntPtr;
  using bson = IntPtr;

  public class Message
  {
    public enum InitMode { blank, inited }
    /* The following fields are for caching proxy objects of master message members */
    private Oid __id;
    private Bson _bson;
    private Oid _messageId;
    protected bool _inScope;

    protected Message(){}

    public SvcBusMessage Handle { get; protected set; }

    public Oid _id {
      get
      {
        var result = __id ?? (__id = Builder.newOid(NativeMethods.SvcBusMessage_get__id(Handle)));
        GC.KeepAlive(this);
        return result;
      }
    }

    public Bson bson {
      get
      {
        var result = _bson ?? (_bson = Builder.newBson(NativeMethods.SvcBusMessage_getBson(Handle)));
        GC.KeepAlive(this);
        return result;
      }
    }

    public Oid messageId {
      get
      {
        var result = _messageId ?? (_messageId = Builder.newOid(NativeMethods.SvcBusMessage_get_messageId(Handle)));
        GC.KeepAlive(this);
        return result;
      }
    }

    /* Not worth caching responsePipeName, it should be used normally only once: to send a response back to consumer */
    public string responsePipeName {
      get
      {
        var result = Marshal.PtrToStringAnsi(NativeMethods.SvcBusMessage_get_responsePipeName(Handle));
        GC.KeepAlive(this);
        return result;
      }
    }

    public string command
    {
      get
      {
        var result = Marshal.PtrToStringAnsi(NativeMethods.SvcBusMessage_get_command(Handle));
        GC.KeepAlive(this);
        return result;
      }
    }

    public bool Broadcast
    {
      get
      {
        var result = Convert.ToBoolean(NativeMethods.SvcBusMessage_get_broadcast(Handle));
        GC.KeepAlive(this);
        return result;
      }
      set
      {
        NativeMethods.SvcBusMessage_set_broadcast(Handle, Convert.ToByte(value));
        GC.KeepAlive(this);
      }
    }

    public uint TtlSec
    {
      set
      {
        NativeMethods.SvcBusMessage_set_ttl(Handle, value);
        GC.KeepAlive(this);
      }
    }

    protected void RegisterHandle()
    {
      Utils.UnmanagedObjectTracker.Register(GetType(), Handle,
                                            Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusMessage_destroy) +
                                            Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusMessage_dealloc)); 
    }

    ~Message()
    {
      if (Handle == SvcBusMessage.Zero)
        return;
      Utils.UnmanagedObjectTracker.Unregister(GetType(), Handle);
      Handle = SvcBusMessage.Zero;
    }
  }

  internal class _Message: Message
  {
    public _Message(bson_oid_p _oid)
    {
      Handle = NativeMethods.SvcBusMessage_alloc();
      NativeMethods.SvcBusMessage_init(Handle, _oid);
      RegisterHandle();
    }

    public _Message(Oid _oid) : this(_oid.oid)
    {
      GC.KeepAlive(_oid);
    }

    public _Message(InitMode initMode)
    {
      Handle = NativeMethods.SvcBusMessage_alloc();
      if(initMode == InitMode.blank)
        NativeMethods.SvcBusMessage_init_Zero(Handle);
      else 
        NativeMethods.SvcBusMessage_init(Handle, IntPtr.Zero);
      RegisterHandle();
    }

    public _Message(_Message srcMessage)
    {
      Handle = NativeMethods.SvcBusMessage_alloc();
      NativeMethods.SvcBusMessage_init_copy(Handle, srcMessage.Handle);
      GC.KeepAlive(srcMessage);
      RegisterHandle();
    }

    public _Message() : this(InitMode.inited) {}
  }
}
