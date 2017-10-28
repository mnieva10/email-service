using System;
using System.Collections.Generic;
using System.Threading;

namespace Sovos.SvcBus
{
  public class DispatchInterfaceConsumer : IDisposable
  {
    private ConsumerPool _consumerPool; // Consider this for internal usage
    readonly Consumer _consumer;
    public uint insertionTime;

    public ILogger Logger
    {
      get { return _consumer.Logger; }
      set { _consumer.Logger = value; }
    }

    public Message BuildRequestMessage(string methodName, object request, bool priority = false)
    {
      var result = Builder.newMessage(Message.InitMode.inited);
      result.bson.append(DispatcherConstants.Command, methodName);
      result.bson.append(DispatcherConstants.Version, Version);
      result.bson.append(DispatcherConstants.Priority, priority);

      var b = result.bson;

      var serializer = Builder.newBsonSerializer(request.GetType(), request, b);
      serializer.Serialize(DispatcherConstants.Params);
      
      return result;
    }

    protected object BuildResponseObject(Bson responseBson, object target)
    {
      var it = responseBson.find(DispatcherConstants.Response);
      if (it == null) 
        return responseBson.ToJson();
      new BsonDeserializer(Builder.newIterator(it)).Deserialize(ref target);
      return target;
    }

    public DispatchInterfaceConsumer(PipeService ps, string name, Service svc, svc_bus_consumer_options_t options = 0)
    {
      _consumer = Builder.newConsumer(ps, name, svc, options);
    }

    public DispatchInterfaceConsumer(ConsumerPool consumerPool)
    {
      _consumerPool = consumerPool;
      _consumer = _consumerPool.Acquire();
    }

    /* if you don't find a Consumer method bellow you are looking for, it's because the function doesn't have a
       serialization/deserialization requirement therefore you should use directly the method provided by Consumer property */
    public object SendAndWait(string methodName, object request, bool priority = false, object target = null)
    {
      var r = BuildRequestMessage(methodName, request, priority);
      return SendAndWait(r, priority, target);
    }

    public object SendAndWait(Message request, bool priority, object target = null)
    {
      var response = BsonDeserializer.CheckSvcBusException(_consumer.sendAndWait(request));
      return BuildResponseObject(response, target);
    }

    public object SendAndWait(string methodName, object request, object target)
    {
      return SendAndWait(methodName, request, false, target);
    }

    public object SendAndWait(Message request, object target = null)
    {
      return SendAndWait(request, false, target);
    }

    public Oid Send(string methodName, object request, bool priority = false)
    {
      var r = BuildRequestMessage(methodName, request, priority);
      _consumer.send(r);
      return (Oid)r.messageId.Clone();
    }

    public Oid Send(Message request) 
    {
      _consumer.send(request);
      return (Oid)request.messageId.Clone();
    }

    public void sendAndWaitResponseAsync(string AMethodName, Message sourceMessage, object ARequest,
                                         IReplier replier, Consumer.OnAsyncWait ACallback, object callbackUserdata = null)
    {
      var RequestMsg = BuildRequestMessage(AMethodName, ARequest);
      // Notice we register are waiter *before* we send the message
      // because if producer replies too fast our callback processor won't
      // have request on list
      _consumer.waitForResponseAsync(sourceMessage, RequestMsg, replier, ACallback, callbackUserdata);
      // Next we send the message *after* we have established our callback
      _consumer.send(RequestMsg);
    }

    public void sendAndWaitResponseAsync(Message sourceMessage, Message ARequest,
                                         IReplier replier, Consumer.OnAsyncWait ACallback, object callbackUserdata = null)
    {
      _consumer.waitForResponseAsync(sourceMessage, ARequest, replier, ACallback, callbackUserdata);
      _consumer.send(ARequest);
    }

    public void WaitResponseAsync(Message sourceMessage, Message RequestMsg,
                                  IReplier replier, Consumer.OnAsyncWait ACallback, object callbackUserdata = null)
    {
      _consumer.waitForResponseAsync(sourceMessage, RequestMsg, replier, ACallback, callbackUserdata);
    }

    public Oid SendBroadcast(string methodName, object request, bool priority = false)
    {
      var r = BuildRequestMessage(methodName, request, priority);
      _consumer.sendBroadcast(r);
      return (Oid)r.messageId.Clone();
    }

    public Oid SendBroadcast(Message request)
    {
      _consumer.sendBroadcast(request);
      return (Oid)request.messageId.Clone();
    }

    public object Wait(Oid messageId, object target = null)
    {
      var response = BsonDeserializer.CheckSvcBusException(_consumer.wait(messageId));
      return BuildResponseObject(response, target);
    }

    public object WaitMultiple(Oid[] msgIds, object target = null)
    {
      var response = BsonDeserializer.CheckSvcBusException(_consumer.waitMultiple(msgIds));
      return BuildResponseObject(response, target);
    }

    public object WaitBroadcast(Oid msgId, object target = null)
    {
      var response = BsonDeserializer.CheckSvcBusException(_consumer.waitBroadcast(msgId));
      return BuildResponseObject(response, target);
    }
    
    public Consumer Consumer
    { 
      get { return _consumer; } 
    }
    public int Version{ get; set; }

    ~DispatchInterfaceConsumer()
    {
      Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing) return;
      if (_consumer == null)
        return;
      if (_consumerPool != null)
      {
        _consumerPool.Release(_consumer);
        _consumerPool = null;
      }
      else _consumer.Dispose();
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
  }

  public class DispatchInterfaceConsumerPool : IDisposable
  {
    private ILogger _logger = new SimpleConsoleLogger();
    private readonly ConsumerPool _consumerPool;

    public uint ResponseTimeout { get; set; }

    public uint HeartbeatTimerInterval
    {
      set { _consumerPool.AutoStartHeartbeatTimer = value; }
    }

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
    
    public uint ttl { get; set; }

    public DispatchInterfaceConsumerPool(PipeService ps, Service service, string name,
                                         svc_bus_consumer_options_t consumerOptions = svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE)
    {
      const int defaultHeartbeatTimerInterval = 90000;

      _consumerPool = Builder.newConsumerPool(ps, name, service, (int)consumerOptions);
      _consumerPool.CopyConsumers = true;
      HeartbeatTimerInterval = defaultHeartbeatTimerInterval;
    }

    private DispatchInterfaceConsumer BuildDic()
    {
      var dic = Builder.newDispatchInterfaceConsumer(_consumerPool);
      dic.Consumer.ResponseTimeout = ResponseTimeout;
      return dic;
    }

    public DispatchInterfaceConsumer Acquire()
    {
      DispatchInterfaceConsumer dic = BuildDic();
      dic.Logger = Logger;
      return dic;
    }

    public void Release(DispatchInterfaceConsumer dic)
    {
      dic.Dispose();
    }

    ~DispatchInterfaceConsumerPool()
    {
      Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing) 
        return;
      _consumerPool.Dispose();
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
  }
}