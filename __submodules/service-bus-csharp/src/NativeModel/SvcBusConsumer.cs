using System;
using System.Runtime.InteropServices;

namespace Sovos.SvcBus
{
  using SvcBusConsumer = IntPtr;
  using bson = IntPtr;
  using bson_oid_p = IntPtr;
  using SvcBusConsumerPool = IntPtr;

  [Serializable]
  public class ConsumerException : SvcBusException
  {
    public ConsumerException(string msg, int code, string sourceMsg) : base(msg, code, sourceMsg) { }

    public new svc_bus_consumer_err_t ErrorCode
    {
      get
      {
        return (svc_bus_consumer_err_t)base.ErrorCode;
      }
    }
  }
  
  public interface IPooledConsumer
  {
    void NotifyReleased();
  }

  public class Consumer : IDisposable, IPooledConsumer
  {
    internal SvcBusConsumer cs;
    protected ConsumerPool _pool;
    private readonly NativeMethods.SvcBusOnAsyncWait _nativeCallback;

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

    protected Consumer()
    {
      _nativeCallback = WaitForResponseAsyncNativeCallback;
    }

    public SvcBusConsumer Handle
    {
      get { return cs; }
    }

    public uint ResponseTimeout
    {
      set
      {
        checkConsumerResult(NativeMethods.SvcBusConsumer_setResponsePipeTimeout(cs, value), "set_ResponseTimeout");
        GC.KeepAlive(this);
      }
    }

    public uint MaxRetriesOnTimeout
    {
      set
      {
        NativeMethods.SvcBusConsumer_setMaxRetriesOnTimeout(cs, value);
        GC.KeepAlive(this);
      }
    }

    protected void checkConsumerResult(int result, string msg)
    {
      if (result != NativeMethods.SERVICE_BUS_OK)
      {
        var e = new ConsumerException(msg, NativeMethods.SvcBusConsumer_getlastErrorCode(cs), Marshal.PtrToStringAnsi(NativeMethods.SvcBusConsumer_getlastErrorMsg(cs)));
        GC.KeepAlive(this);
        throw e;
      }
    }

    ~Consumer()
    {
      Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (cs == SvcBusConsumer.Zero)
        return;
      if (_pool == null)
        Utils.UnmanagedObjectTracker.Unregister(GetType(), cs);
      else if (disposing)
        _pool.Release(this);
      cs = SvcBusConsumer.Zero;
    }

    protected void RegisterHandle(SvcBusHandles deps)
    {
      Utils.UnmanagedObjectTracker.Register(GetType(), cs, 
                                            Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusConsumer_destroy) + 
                                            Utils.SvcBusHandleDestroyDelegate(NativeMethods.SvcBusConsumer_dealloc), deps);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public void send(Message msg)
    {
      checkConsumerResult(NativeMethods.SvcBusConsumer_send(cs, msg.Handle), "send");
      GC.KeepAlive(msg);
      GC.KeepAlive(this);
    }

    public void sendBroadcast(Message msg)
    {
      checkConsumerResult(NativeMethods.SvcBusConsumer_sendBroadcast(cs, msg.Handle), "sendBroadcast");
      GC.KeepAlive(msg);
      GC.KeepAlive(this);
    }

    public void updateHeartbeat()
    {
      checkConsumerResult(NativeMethods.SvcBusConsumer_updateHeartbeat(cs), "updateHeartbeat");
      GC.KeepAlive(this);
    }

    public void startUpdateHeartbeatTimer(uint millis)
    {
      checkConsumerResult(NativeMethods.SvcBusConsumer_startUpdateHeartbeatTimer(cs, millis), "startUpdateHeartbeatTimer");
      GC.KeepAlive(this);
    }

    public void stopUpdateHeartbeatTimer()
    {
      NativeMethods.SvcBusConsumer_stopUpdateHeartbeatTimer(cs);
      GC.KeepAlive(this);
    }

    public Bson wait(Oid msgId)
    {
      var result = bson.Zero;
      checkConsumerResult(NativeMethods.SvcBusConsumer_wait(cs, ref result, msgId.oid), "wait");
      GC.KeepAlive(msgId);
      GC.KeepAlive(this);
      return Builder.newBson(result);
    }

    public Bson waitMultiple(Oid[] msgIds)
    {
      var result = bson.Zero;

      var msgIdsPtrs = new bson_oid_p[msgIds.Length + 1];
      for (int i = 0; i < msgIds.Length; i++)
        msgIdsPtrs[i] = msgIds[i].oid;
      msgIdsPtrs[msgIds.Length] = IntPtr.Zero; // should be null-terminated

      checkConsumerResult(NativeMethods.SvcBusConsumer_wait_multiple(cs, ref result, msgIdsPtrs), "waitMultiple");
      foreach(var msgId in msgIds)
        GC.KeepAlive(msgId);
      GC.KeepAlive(this);
      return Builder.newBson(result);
    }

    public Bson sendAndWait(Message msg)
    {
      var result = bson.Zero;
      checkConsumerResult(NativeMethods.SvcBusConsumer_sendAndWait(cs, ref result, msg.Handle), "sendAndWait");
      GC.KeepAlive(msg);
      GC.KeepAlive(this);
      return Builder.newBson(result);
    }

    public void markBroadcastRequestAsTaken(Oid id)
    {
      checkConsumerResult(NativeMethods.SvcBusConsumer_markBroadcastRequestAsTaken(cs, id.oid), "markBroadcastRequestAsTaken");
      GC.KeepAlive(id);
      GC.KeepAlive(this);
    }


    public Bson waitBroadcast(Oid msgId)
    {
      var result = bson.Zero;
      checkConsumerResult(NativeMethods.SvcBusConsumer_waitBroadcast(cs, ref result, msgId.oid), "waitBroadcast");
      GC.KeepAlive(msgId);
      GC.KeepAlive(this);
      return Builder.newBson(result);
    }
    
    public void truncateResponseCollection()
    {
      checkConsumerResult(NativeMethods.SvcBusConsumer_truncateResponseCollection(cs), "truncateResponseCollection");
      GC.KeepAlive(this);
    }

    public void NotifyReleased()
    {
      _pool = null;
      cs = SvcBusConsumer.Zero;
    }

    public string ResponsePipeName
    {
      get
      {
        var result = Marshal.PtrToStringAnsi(NativeMethods.SvcBusConsumer_getResponsePipeName(Handle));
        GC.KeepAlive(this);
        return result;
      }
    }

    public delegate void OnAsyncWait(Bson response, Message sourceMessage, IReplier replier, object userdata);

    private void WaitForResponseAsyncNativeCallback(IntPtr err, bson_oid_p response, IntPtr userdata)
    {
      try
      {
        var skipHandleFree = false;
        var handle = (GCHandle)userdata;
        try
        {
          var data = handle.Target as WaitForResponseAsyncNativeCallbackData;
          if (data == null)
            return; /* This condition highlights an internal error. But there's nothing we can do here... 
                       we don't want to kill the service-bus dll worker thread throwing an exception */
          var error = new GenericErr(err);
          try
          {
            if (error.ErrCode == (int)svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_OK)
            {
              skipHandleFree = data.isBroadcast;
              data.Callback(Builder.newBson(response), data.SourceMsg, data.replier, data.UserData);
            }
            else
            {
              var exception = new ConsumerException(error.ErrMsg, error.ErrCode, "");
              var exceptionResponse = Responder.ResponseFromException(data.SourceMsg != null ? data.SourceMsg.messageId : null, exception);
              data.Callback(exceptionResponse.bson, data.SourceMsg, data.replier, data.UserData);
            }
          }
          catch (Exception exception)
          {
            /* If we don't own the message and responder, but an exception happens, we will take responsibility on doing cleaning
               of these two objects, otherwise, in most cases they will simply leak until garbage collected */
            if ((data.replier == null) || (data.SourceMsg == null) || (data.SourceMsg.responsePipeName == ""))
            {
              Logger.WriteLogEntry(typeof(Consumer), exception, "Error thrown in WaitForResponseAsyncNativeCallback delegate and no Responder to serialize back", LogLevel.Error);
              return;
            }
            // there's nobody to send a exception response back
            Logger.WriteLogEntry(typeof(Consumer), exception, "Error thrown in WaitForResponseAsyncNativeCallback delegate and serializing back to caller", LogLevel.Warning);
            var exceptionMessage = Responder.ResponseFromException(data.SourceMsg.messageId, exception);
            data.replier.send(data.SourceMsg.responsePipeName, exceptionMessage);
          }
        }
        finally
        {
          if (!skipHandleFree) handle.Free();
        }
      }
      catch (Exception exception)
      {
        Logger.WriteLogEntry(typeof(Consumer), exception, "Near fatal error thrown in WaitForResponseAsyncNativeCallback delegate", LogLevel.Fatal);
      }
    }

    class WaitForResponseAsyncNativeCallbackData
    {
      public OnAsyncWait Callback { get; set; }
      public object UserData { get; set; }
      public Message SourceMsg { get; set; }
      public IReplier replier { get; set; }
      public bool isBroadcast { get; set; }
    }

    public void waitForResponseAsync(Message sourceMessage, Message request,
                                     IReplier replier, OnAsyncWait callback, object callbackUserdata = null)
    {
      var handle = GCHandle.Alloc(new WaitForResponseAsyncNativeCallbackData
      {
        Callback = callback,
        UserData = callbackUserdata,
        SourceMsg = sourceMessage,
        replier = replier,
        isBroadcast = request.Broadcast
      });
      try
      {
        checkConsumerResult(NativeMethods.SvcBusConsumer_wait_for_response_async(Handle, request.Handle,
          _nativeCallback, (IntPtr)handle), "waitForResponseAsync");
        GC.KeepAlive(request);
        GC.KeepAlive(this);
      }
      catch
      {
        handle.Free();
        throw;
      }
    }
  }

  internal class _Consumer : Consumer
  {
    public _Consumer(PipeService ps, string name, Service svc, svc_bus_consumer_options_t options = 0)
    {
      cs = NativeMethods.SvcBusConsumer_alloc();
      checkConsumerResult(NativeMethods.SvcBusConsumer_init(cs, ps.Handle, name, svc.Handle, (int)options), "constructor");
      RegisterHandle(new SvcBusHandles { { ps.GetType(), ps.Handle }, { svc.GetType(), svc.Handle } });
      GC.KeepAlive(svc);
      GC.KeepAlive(ps);
    }

    public _Consumer(PipeService ps, string name, string responsePipeName, svc_bus_consumer_options_t options = 0)
    {
      cs = NativeMethods.SvcBusConsumer_alloc();
      checkConsumerResult(NativeMethods.SvcBusConsumer_initBindToResponsePipe(cs, ps.Handle, name, (int)options, responsePipeName), "constructor");
      RegisterHandle(new SvcBusHandles { { ps.GetType(), ps.Handle } });
      GC.KeepAlive(ps);
    }

    public _Consumer(ConsumerPool pool, SvcBusConsumer handle)
    {
      _pool = pool;
      cs = handle;
    }
  }
}