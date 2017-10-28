using System;
using System.Threading;
using Sovos.SvcBus;
using NUnit.Framework;

namespace SvcBusTests
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture, GTestStyleConsoleOutputAttribute]
  class SvcBusConsumerTests
  {
    private Scope _scope;
    private Service svc_volatile;
    private Service svc_only_insert;
    private PipeService ps;
    private Producer _producer;
    private Consumer _consumer;

    [SetUp]
    public void SetUp()
    {
      _scope = new Scope();
      svc_volatile = Builder.newService("test_service_volatile", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE, 1024, 1024);
      svc_only_insert = Builder.newService("test_service_only_insert", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE, 1024, 1024);
      ps = _scope.newPipeService(Constants.ConnectionString);
      _producer = _scope.newProducer(ps, "prod", svc_volatile);
      _producer.RequestTimeout = 10000;

      _consumer = _scope.newConsumer(ps, "consumer", svc_volatile, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
    }

    [TearDown]
    public void TearDown()
    {
      svc_volatile = null;
      svc_only_insert = null;
      ps = null;
      _producer = null;
      _consumer = null;
      _scope.Dispose();
    }

    public void TestCreateConsumer_Fails()
    {
      var bad_service = new _Service("response_", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);
      try
      {
        // ReSharper disable once UnusedVariable
        var cs = _scope.newConsumer(ps, "consumer", bad_service);
      }
      catch (ConsumerException e)
      {
        Assert.AreEqual(svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_INVALID_REQUEST_PIPE_NAME, e.ErrorCode);
        Assert.AreEqual("constructor [1 : SvcBusConsumer can't bind to response pipe]", e.Message);
      }
    }

    [Test]
    public void TestSendMessage()
    {
      var cs = _scope.newConsumer(ps, "consumer", svc_only_insert,
          svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      var oid = Builder.newOid();
      oid.Gen();
      var msg = Builder.newMessage(oid);
      cs.send(msg);
    }

    [Test]
    public void TestSendBroadcastMessage()
    {
      var cs = _scope.newConsumer(ps, "consumer", svc_only_insert,
          svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      var oid = Builder.newOid();
      oid.Gen();
      var msg = Builder.newMessage(oid);
      cs.sendBroadcast(msg);
    }

    private bool _broadcastResponseReceived;
    private bool _timeoutReceived;

    void BroadcastResponseCallback(Bson response, Message sourceMessage, IReplier replier, object userdata)
    {
      _broadcastResponseReceived = true;
      _timeoutReceived = response.find(DispatcherConstants.Exception) != null;
    }

    [Test]
    public void TestSendBroadcastMessageWaitAsyncTimeoutReceived()
    {
      ps.startAsyncWorkPool(2, 2);
      var cs = _scope.newConsumer(ps, "consumer", svc_only_insert,
          svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      var oid = Builder.newOid();
      oid.Gen();
      var msg = Builder.newMessage(oid);
      _broadcastResponseReceived = false;
      cs.ResponseTimeout = 1000;
      msg.Broadcast = true;
      cs.waitForResponseAsync(null, msg, null, BroadcastResponseCallback);
      cs.sendBroadcast(msg);
      Thread.Sleep(5000);
      Assert.IsTrue(_broadcastResponseReceived);
      Assert.IsTrue(_timeoutReceived);
    }

    [Test]
    public void TestMarkBroadcastRequestAsTaken()
    {
      var cs = _scope.newConsumer(ps, "consumer", svc_volatile,
          svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      var oid = Builder.newOid();
      oid.Gen();
      cs.markBroadcastRequestAsTaken(oid);
    }

    [Test]
    public void TestSetMaxRetriesOnTimeout()
    {
      var cs = _scope.newConsumer(ps, "consumer", svc_volatile,
          svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      cs.MaxRetriesOnTimeout = 3;
    }

    [Test]
    public void TestUpdateHeartbeat()
    {
      _consumer.updateHeartbeat();
    }

    [Test]
    public void TestStartAndStopUpdateHeartbeatTimer()
    {
      try
      {
        _consumer.startUpdateHeartbeatTimer(100);
      }
      catch (SvcBusException E)
      {
        Assert.AreEqual("startUpdateHeartbeatTimer [6 : pipeservice thread pool not initialized for async operations]", E.Message);
        Assert.AreEqual((int)svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_THREADPOOL_NO_INIT, E.ErrorCode);
      }
      ps.startAsyncWorkPool(2, 2);
      _consumer.startUpdateHeartbeatTimer(100);
      Thread.Sleep(500); // Let's force at least one execution of timer based updateHeartbeat
      _consumer.stopUpdateHeartbeatTimer();
      ps.stopAsyncWorkPool();
    }

    [Test]
    public void TestWaitBroadcast_Fail()
    {
      try
      {
        _consumer.ResponseTimeout = 1000;
        var oid = Builder.newOid();
        _consumer.waitBroadcast(oid);
      }
      catch (ConsumerException e)
      {
        Assert.AreEqual(svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_WAIT_EXCEEDED, e.ErrorCode);
        Assert.AreEqual("Max wait time exceeded", e.SourceMessage);
        Assert.AreEqual("waitBroadcast [4 : Max wait time exceeded]", e.Message);
      }
    }

    [Test]
    public void TestWait_Fail()
    {
      try
      {
        _consumer.ResponseTimeout = 1000;
        var oid = Builder.newOid();
        _consumer.wait(oid);
      }
      catch (ConsumerException e)
      {
        Assert.AreEqual(svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_WAIT_EXCEEDED, e.ErrorCode);
        Assert.AreEqual("Max wait time exceeded", e.SourceMessage);
        Assert.AreEqual("wait [4 : Max wait time exceeded]", e.Message);
      }
    }

    [Test]
    public void TestWaitMultiple()
    {
      const int msgsToSend = 2;
      Oid[] oids = new Oid[2];

      for (int i = 0; i < msgsToSend; i++)
      {
        Message msg = Builder.newMessage(Message.InitMode.inited);
        oids[i] = msg.messageId;
        _consumer.send(msg);

        var request = _producer.waitAndTake();
        var response = Builder.newMessage(request.messageId);
        response.bson.append("data", 111);
        _producer.responder.send(request.responsePipeName, response);
      }

      for (int i = 0; i < msgsToSend; i++)
      {
        Bson response = _consumer.waitMultiple(oids);
        Iterator it = response.find("data");
        Assert.AreEqual(111, (int)it);
      }
    }

    [Test]
    public void TestWaitMultiple_Fail()
    {
      var cs = _scope.newConsumer(ps, "consumer", svc_volatile, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      try
      {
        cs.ResponseTimeout = 1000;
        var oid = Builder.newOid();
        var oid2 = Builder.newOid();
        var oids = new Oid[2];
        oids[0] = oid;
        oids[1] = oid2;
        cs.waitMultiple(oids);
      }
      catch (ConsumerException e)
      {
        Assert.AreEqual(svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_WAIT_EXCEEDED, e.ErrorCode);
        Assert.AreEqual("Max wait time exceeded", e.SourceMessage);
        Assert.AreEqual("waitMultiple [4 : Max wait time exceeded]", e.Message);
      }
    }

    [Test]
    public void TestSendAndWait_Fail()
    {
      var cs = _scope.newConsumer(ps, "consumer", svc_only_insert, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      try
      {
        var oid = Builder.newOid();
        var msg = Builder.newMessage(oid);
        cs.ResponseTimeout = 1000;
        cs.sendAndWait(msg);
      }
      catch (ConsumerException e)
      {
        Assert.AreEqual(svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_WAIT_EXCEEDED, e.ErrorCode);
        Assert.AreEqual("Max wait time exceeded", e.SourceMessage);
        Assert.AreEqual("sendAndWait [4 : Max wait time exceeded]", e.Message);
      }
    }

    [Test]
    public void TestSetResponseTimeout_Fail()
    {
      try
      {
        var cs = _scope.newConsumer(ps, "consumer", svc_only_insert);
        cs.ResponseTimeout = 1000;
      }
      catch (ConsumerException e)
      {
        Assert.AreEqual(svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_RESPONSE_PIPE_CANT_BE_NULL, e.ErrorCode);
        Assert.AreEqual("Response pipe is NULL", e.SourceMessage);
        Assert.AreEqual("set_ResponseTimeout [2 : Response pipe is NULL]", e.Message);
      }
    }
    
    [Test]
    public void TestInitBind()
    {
      var c1 = _scope.newConsumer(ps, "consumer", svc_only_insert,
        svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE |
        svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_KEEP_RESPONSE_PIPE);
      var c2 = _scope.newConsumer(ps, "consumer", c1.ResponsePipeName);

      try
      {
        c2.send(Builder.newMessage(Message.InitMode.inited));
        Assert.Fail("can't send from binded to response pipe consumer");
      }
      catch (ConsumerException e)
      {
        Assert.AreEqual(svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_REQUEST_PIPE_CANT_BE_NULL, e.ErrorCode);
      }
    }

    [Test]
    public void TestWaitForResponseAsync()
    {
      var callbackCalled = new ManualResetEvent(false);
      try
      {
        ps.startAsyncWorkPool(2, 2);

        var req = Builder.newMessage(Message.InitMode.inited);
        _consumer.waitForResponseAsync(null, req, null, (resp, msg, responder, userdata) =>
        {
          // ReSharper disable once AccessToDisposedClosure
          callbackCalled.Set();
          Assert.AreEqual(111, (int)resp.find("data"));
        }, new object());
        _consumer.send(req);

        var request = _producer.waitAndTake();
        var response = Builder.newMessage(request.messageId);
        response.bson.append("data", 111);
        _producer.responder.send(request.responsePipeName, response);

        Assert.True(callbackCalled.WaitOne(10000));
      }
      finally
      {
        callbackCalled.Dispose();
      }
    }

    [Test]
    public void TestWaitForResponseAsyncTimeout()
    {
      var callbackCalled = new ManualResetEvent(false);
      try
      {
        ps.startAsyncWorkPool(2, 2);

        var req = Builder.newMessage(Message.InitMode.inited);
        _consumer.ResponseTimeout = 2000;
        _consumer.waitForResponseAsync(null, req, null, (resp, msg, responder, userdata) =>
        {
          Assert.AreEqual("ConsumerException", (string)resp.find(DispatcherConstants.Exception));
          try
          {
            BsonDeserializer.CheckSvcBusException(resp);
          }
          catch (ConsumerException)
          {
            // ReSharper disable once AccessToDisposedClosure
            callbackCalled.Set();
          }
        }, new object());

        Assert.IsTrue(callbackCalled.WaitOne(6000));
      }
      finally
      {
        callbackCalled.Dispose();
      }
    }

    [Test]
    public void ShouldUseLoggerIfSet()
    {
      ps.startAsyncWorkPool(2, 2);
      var loggerSpy = new AsyncLoggerSpy();
      _consumer.Logger = loggerSpy;

      const string expectedExceptionMsg = "expected";

      var req = Builder.newMessage();
      _consumer.waitForResponseAsync(null, req, null, (resp, msg, responder, userdata) =>
      {
        throw new Exception(expectedExceptionMsg);
      });
      _consumer.send(req);

      var request = _producer.waitAndTake();
      _producer.responder.send(request.responsePipeName, Builder.newMessage(request.messageId));

      Assert.True(loggerSpy.WaitForEntryWritten(3000));

      Assert.AreEqual(1, loggerSpy.LogEntries.Count);
      StringAssert.Contains(expectedExceptionMsg, loggerSpy.LogEntries[0]);
    }
  }
}
