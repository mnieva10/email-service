using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sovos.SvcBus
{
  using SvcBusResponder = IntPtr;
  using bson = IntPtr;

  [Serializable]
  public class ResponderException : SvcBusException
  {
    public ResponderException(string msg, int code, string sourceMsg) : base(msg, code, sourceMsg) { }

    public new svc_bus_responder_err_t ErrorCode
    {
      get
      {
        return (svc_bus_responder_err_t)base.ErrorCode;
      }
    }
  }

  public class Responder
  {
    public SvcBusResponder Handle { get; protected set; }

    protected Responder() { }

    protected void checkResponderResult(int result, string msg)
    {
      if (result != NativeMethods.SERVICE_BUS_OK)
      {
        var e = new ResponderException(msg, NativeMethods.SvcBusResponder_getlastErrorCode(Handle),
          Marshal.PtrToStringAnsi(NativeMethods.SvcBusResponder_getlastErrorMsg(Handle)));
        GC.KeepAlive(this);
        throw e;
      }
    }

    ~Responder()
    {
      if (Handle == SvcBusResponder.Zero)
        return;
      Utils.UnmanagedObjectTracker.Unregister(GetType(), Handle);
      Handle = SvcBusResponder.Zero;
    }

    public void send(string responsePipeName, Message message)
    {
      checkResponderResult(NativeMethods.SvcBusResponder_send(Handle, responsePipeName, message.Handle), "send");
      GC.KeepAlive(message);
      GC.KeepAlive(this);
    }

    public static void ResponseFromException(Exception e, Bson bson)
    {
      bson.append(DispatcherConstants.Exception, e.GetType().Name);
      bson.append(DispatcherConstants.ExceptionMessage, e.Message);
      var st = new StackTrace(e, true);
      // Get the top stack frame
      var frame = st.GetFrame(0);
      // Get the line number from the stack frame
      var stackInfo = frame != null ? string.Format("Stack information: {0}. line {1}", frame.GetFileName(), frame.GetFileLineNumber()) : "no stack info";
      bson.append(DispatcherConstants.StackInfo, stackInfo);
      if (!(e is SvcBusException)) return;
      bson.append(DispatcherConstants.ExceptionErrorCode, ((SvcBusException)e).ErrorCode);
      bson.append(DispatcherConstants.ExceptionSourceMessage, ((SvcBusException)e).SourceMessage);
    }

    public static Message ResponseFromException(Oid msgId, Exception e)
    {
      var result = msgId != null ? Builder.newMessage(msgId) : Builder.newMessage(Message.InitMode.inited);
      ResponseFromException(e, result.bson);
      return result;
    }
  }

  internal class _Responder : Responder
  {
    public _Responder(PipeService ps)
    {
      Handle = NativeMethods.SvcBusResponder_alloc();
      checkResponderResult(NativeMethods.SvcBusResponder_init(Handle, ps.Handle), "constructor");
      Utils.UnmanagedObjectTracker.Register(GetType(), Handle, Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusResponder_destroy) +
                                            Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusResponder_dealloc),
                                            new SvcBusHandles { { ps.GetType(), ps.Handle } });
      GC.KeepAlive(ps);
    }

    public _Responder(SvcBusResponder responder)
    {
      Handle = responder;
      Utils.UnmanagedObjectTracker.Register(GetType(), Handle);
    }
  }
}
