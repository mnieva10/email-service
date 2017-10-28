using System.Diagnostics;
using System.Globalization;
using Sovos.SvcBus;
using NUnit.Framework;
using System;
using System.Threading;
using System.Collections.Generic;
using ThreadPool = Sovos.WinApiThreadPool.ThreadPool;

namespace SvcBusTests
{
  [TestFixture, GTestStyleConsoleOutputAttribute]
  class DispatcherTest : IDisposable
  {
    Scope _scope;
    Service _service;
    PipeService _ps;
    Dispatcher _dispatcher;

    const string ThrowsException = "Throws Exception";

    private class SubObject
    {
      [SvcBusSerializable]
      public string StrValue { get; set; }
    }

    private class TestResponse
    {
      [SvcBusSerializable]
      // ReSharper disable once UnusedAutoPropertyAccessor.Local
      public int IntValue { private get; set; }
      [SvcBusSerializable]
      // ReSharper disable once UnusedAutoPropertyAccessor.Local
      public List<string> StrList { private get; set; }
      [SvcBusSerializable]
      // ReSharper disable once UnusedAutoPropertyAccessor.Local
      public SubObject SubObject { private get; set; }

      public TestResponse()
      {
        SubObject = new SubObject();
      }
    }

    [Serializable]
    private class TestDispatchInterfaceException : Exception
    {
      public TestDispatchInterfaceException(string message) : base(message) { }
    }

    private class CustomRequestProcessingStrategy : ChainableRequestProcessingStrategy
    {
      public override void Process(Message msg)
      {
        // For testing processing, we will create a processing strategy that always inject some delay
        Thread.Sleep(500);
      }
    }

    private class CustomResponseProcessingStrategy : IResponseProcessingStrategy
    {
      public void ProcessResponse(Message response)
      {
        response.bson.append("injected_field", 1);
      }
    }

    private class TestDispatchInterface : DispatchInterface, IDisposable
    {
      private readonly Scope _scope;

      private ConsumerPoolDictionary _hybridConsumerPoolDictionary;
      private ConsumerPoolDictionary hybridDictionary
      {
        get
        {
          return _hybridConsumerPoolDictionary ?? (_hybridConsumerPoolDictionary = _scope.newConsumerPoolDictionary());
        }
      }

      private ConsumerPool _hybridConsumerPool;
      private ConsumerPool hybridConsumerPool
      {
        get
        {
          if (_hybridConsumerPool == null)
          {
            _hybridConsumerPool = hybridDictionary.GetConsumerPool(Constants.ConnectionString, "c_sharp_emulated_volatile", "test_consumer",
                                                                   svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);

            /* The following two lines of code are critical!!!! */
            _hybridConsumerPool.AutoStartHeartbeatTimer = 0;
            _hybridConsumerPool.startAsyncWorkPool(2, 4); // ThreadPool operating is absolutely necessary for waitForResponseAsync to work
            _hybridConsumerPool.CopyConsumers = true; // Cloning of consumers is absolutely necessary for waitForResponseAsync to work
          }
          return _hybridConsumerPool;
        }
      }

      protected override void DescribeMethod(string methodName, Bson response)
      {
        Bson sub;
        switch (methodName)
        {
          case "GetInstancePointer":
            response.append(DispatcherConstants.Description, "this is the GetInstancePointer method");
            sub = response.appendDocumentBegin(DispatcherConstants.Params);
            response.appendDocumentEnd(sub);
            sub = response.appendDocumentBegin(DispatcherConstants.Result);
            sub.append("instance_pointer", "bsonLONG: returns the pointer casted to integer of the current instance");
            response.appendDocumentEnd(sub);
            sub = response.appendArrayBegin(DispatcherConstants.Throws);
            response.appendArrayEnd(sub);
            break;
          case "GetThreadId":
            response.append(DispatcherConstants.Description, "this is the GetThreadId method");
            sub = response.appendDocumentBegin(DispatcherConstants.Params);
            response.appendDocumentEnd(sub);
            sub = response.appendDocumentBegin(DispatcherConstants.Result);
            sub.append("thread_id", "bsonINT: returns the thread ID of the thread executing the method call");
            response.appendDocumentEnd(sub);
            sub = response.appendArrayBegin(DispatcherConstants.Throws);
            response.appendArrayEnd(sub);
            break;
          case "ThrowException":
            response.append(DispatcherConstants.Description, "this is the ThrowException method");
            sub = response.appendDocumentBegin(DispatcherConstants.Params);
            response.appendDocumentEnd(sub);
            sub = response.appendDocumentBegin(DispatcherConstants.Result);
            response.appendDocumentEnd(sub);
            sub = response.appendArrayBegin(DispatcherConstants.Throws);
            sub.append("", "TestDispatchInterfaceException");
            response.appendArrayEnd(sub);
            break;
          default:
            base.DescribeMethod(methodName, response);
            break;
        }
      }

      protected override bool SupportsMethodDescribe()
      {
        return true;
      }

      public TestDispatchInterface(object userData)
        : base(userData)
      {
        _scope = new Scope();
      }
      
      // ReSharper disable once MemberCanBePrivate.Local
      public object Echo(Message msg)
      {
        var echoRequest = new SubObject();
        var temp = (object)echoRequest;
        DeserializeRequest(msg, ref temp);

        var result = new SubObject { StrValue = echoRequest.StrValue };
        return result;
      }

      // ReSharper disable once UnusedMember.Local
      public object EchoWithSmallWait(Message msg)
      {
        Thread.Sleep(100);
        return Echo(msg);
      }

      // ReSharper disable once UnusedMember.Local
      public void ThrowException(Message msg)
      {
        throw new TestDispatchInterfaceException(ThrowsException);
      }

      // ReSharper disable once UnusedMember.Local
      public object ReceiveAndReturnAnonymousObject(Message msg)
      {
        dynamic echoRequest = DeserializeRequest(msg /*, new { StrValue = AnonymousType.String } */);
        return new { StrValue = (string)echoRequest.StrValue };
      }

      // ReSharper disable once UnusedMember.Local
      public object ReceiveAndReturnAnonymousObjectWithType(Message msg)
      {
        dynamic echoRequest = DeserializeRequest(msg /*, new { StrValue = AnonymousType.String } */);
        return new
        {
          _type = "SubObject",
          echoRequest.StrValue
        };
      }

      // ReSharper disable once UnusedMember.Local
      public object GetThreadId(Message msg)
      {
        Message res = Builder.newMessage(msg.messageId);
        res.bson.append("thread_id", Thread.CurrentThread.ManagedThreadId);
        return res;
      }

      // ReSharper disable once UnusedMember.Local
      public object GetInstancePointer(Message msg)
      {
        Message res = Builder.newMessage(msg.messageId);
        res.bson.append("instance_pointer", GetHashCode());
        Thread.Sleep(500); // Need to sleep to force creating other instance of DispatchInterface(for MultiInstanceDispatchInterface test)
        return res;
      }

      // ReSharper disable once UnusedMember.Local
      public object MsgNoResponse(Message msg)
      {
        return null;
      }

      // ReSharper disable once UnusedMember.Local
      public object MsgReturnNoResponse_ReturnDirect(Message msg, IReplier replier)
      {
        var response = Builder.newMessage(msg.messageId);
        response.bson.append("strdata", "the_data");
        replier.send(msg.responsePipeName, response);
        return null;
      }

      /* This method exemplifies an hybrid method part of a DispatchInterface that is also a consumer
         of other service. The special characteristic of this method is that it won't block the calling
         thread waiting for a response, but rather will use the callback based waitForResponseAsync() method 
         with an anonymous delegate to produce the response asynchronously. The callback is called
         from within a thread managed by C service-bus library.
         Developer must be careful which objects are referred from within callback. Some objects upon leaving
         scope are disposed.
         Another key factor is that responder can be null if called from command line processor, si the safe
         thing to do is check for null in the callback before attempting to send a response back*/
      // ReSharper disable once UnusedMember.Local
      public object hybridMethod(Message msg, IReplier replier)
      {
        // ReSharper disable once InconsistentNaming
        using (var methodScope = new Scope())
        {
          var methodDic = methodScope.newDispatchInterfaceConsumer(hybridConsumerPool); // Consumer that use waitForResponseAsync() must be obtained from pool

          var methodRequest = new SubObject { StrValue = "Hello World" }; // Mock request
          methodDic.sendAndWaitResponseAsync("Echo", msg, methodRequest, replier, (outerResponse, outerMsg, outerResponder, outerUserdata) =>
          {
            using (var innerScope = new Scope())
            {
              var innerDic = innerScope.newDispatchInterfaceConsumer(hybridConsumerPool);
              var innerRequest = new SubObject { StrValue = "Hello World" }; // Inner Mock request
              innerDic.sendAndWaitResponseAsync("Echo", outerMsg, innerRequest, outerResponder, (innerResponse, innerMsg, innerResponder, innerUserdata) =>
              {
                if (innerResponder != null)
                {
                  var responseObj = new SubObject { StrValue = "some_data" };
                  innerResponder.send(innerMsg.responsePipeName, responseObj, innerMsg.messageId,
                    ResponseProcessingStrategy, false);
                }
                // ReSharper disable once RedundantIfElseBlock
                else
                {
                  /* A real service should write here some code to send output to console processing. In console processing mode
                     there will be no responder passed to these "hybrid" kind of methods */
                }
              });
            }
          });
        }
        return null; // Dispatcher core now supports methods returning null. On those cases no response will be sent back to Consumer
      }

      // ReSharper disable once UnusedMember.Local
      public object hybridMethodWithExceptionInCallback(Message msg, IReplier replier)
      {
        // ReSharper disable once InconsistentNaming
        using (var __scope = new Scope())
        {
          var dic = __scope.newDispatchInterfaceConsumer(hybridConsumerPool); // Consumer that use waitForResponseAsync() must be obtained from pool

          var request = new SubObject { StrValue = "Hello World" }; // Mock request
          dic.sendAndWaitResponseAsync("Echo", msg, request, replier, (response, _msg, _responder, userdata) =>
          {
            BsonDeserializer.CheckSvcBusException(response);
            throw new Exception("Explosion");
          });
        }
        return null; // Dispatcher core now supports methods returning null. On those cases no response will be sent back to Consumer
      }

      // ReSharper disable once UnusedMember.Local
      public object ReturnObject(Message msg)
      {
        return new TestResponse
        {
          IntValue = 10,
          StrList = new List<string> { "hello", "world" },
          SubObject = new SubObject { StrValue = "str value テ" }
        };
      }

      public void Dispose()
      {
        _scope.Dispose();
      }
    }

    public object BuildResponseObject(string methodName)
    {
      return new SubObject();
    }

    [TestFixtureSetUp]
    public void FixtureSetUp()
    {
      var map = BuildableSerializableTypeMapper.Mapper;
      map.Register("SubObject", typeof(SubObject));
      map.Register("TestResponse", typeof(TestResponse));
    }

    [SetUp]
    public void SetUp()
    {
      _scope = new Scope();
      _service = Builder.newService("c_sharp_test_volatile", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);
      _ps = new _PipeService(Constants.ConnectionString);
      try
      {
        _ps.dropServicePipe(_service);
      }
      catch (PipeServiceException) { /* Ignore */ }
      _ps.statsCollector.FlushInterval = 500;
      _ps.startAsyncWorkPool(2, 4);
      _dispatcher = _scope.newDispatcher("c_sharp_test_dispatcher", _service, _ps, typeof(TestDispatchInterface));
      _dispatcher.Logger = new SimpleConsoleLogger();
      var service = Builder.newService("c_sharp_emulated_volatile", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);
      var servicePersistence = Builder.newServicePersistence(_ps);
      servicePersistence.Save(service);
    }

    [TearDown]
    public void TearDown()
    {
      if (_dispatcher != null)
        _dispatcher.Dispose();
      Dispose();
    }

    [Test]
    public void CallSimpleDescribe()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      Consumer consumer = _scope.newConsumer(_ps, "consumer_test", _service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      consumer.ResponseTimeout = 500;

      Message msg = Builder.newMessage(Message.InitMode.inited);
      msg.bson.append(DispatcherConstants.Command, "Describe");
      Bson response = consumer.sendAndWait(msg);

      response.find(DispatcherConstants.Commands);
    }

    [Test]
    public void Call_describe_and_testThrowException()
    {
      var memLogger = new LoggerSpy();
      _dispatcher.Logger = memLogger;
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      Consumer consumer = _scope.newConsumer(_ps, "consumer_test", _service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      consumer.ResponseTimeout = 500;

      Message msg = Builder.newMessage(Message.InitMode.inited);
      msg.bson.append(DispatcherConstants.Command, "Describe");
      Bson response = consumer.sendAndWait(msg);

      Iterator it = response.find(DispatcherConstants.Commands);

      Iterator itMethods = Builder.newIterator(it);
      while (itMethods.next())
        if (itMethods.key == "ThrowException")
          break;
      Assert.AreEqual("ThrowException", itMethods.key);

      Iterator itMethodDescription = Builder.newIterator(itMethods);
      Assert.AreNotEqual(bson_type.BSON_EOO, itMethodDescription.next());
      Assert.AreEqual(DispatcherConstants.Description, itMethodDescription.key);
      Assert.AreNotEqual(bson_type.BSON_EOO, itMethodDescription.next());
      Assert.AreEqual(DispatcherConstants.Params, itMethodDescription.key);

      msg = Builder.newMessage(Message.InitMode.inited);
      msg.bson.append(DispatcherConstants.Command, "ThrowException");
      response = consumer.sendAndWait(msg);

      it = response.find(DispatcherConstants.Exception);
      Assert.AreEqual("TestDispatchInterfaceException", (string)it);
      Assert.AreNotEqual(bson_type.BSON_EOO, it.next());
      Assert.AreEqual(DispatcherConstants.ExceptionMessage, it.key);
      Assert.AreEqual(ThrowsException, (string)it);
      Assert.AreEqual(1, memLogger.LogEntries.Count);
      Assert.IsTrue(memLogger.LogEntries[0].StartsWith("Throws Exception:Error thrown in DispatcherJob.Run method"));
      Assert.IsTrue(memLogger.LogEntries[0].EndsWith("Warning"));
    }

    [Test]
    public void MultithreadedDispatchInterface()
    {
      _dispatcher.Start(2, DispatchMode.SingletonDispatchInterface); // Start with two threads. We will check the threadid in the body of the test
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      Consumer consumer = _scope.newConsumer(_ps, "consumer_test", _service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      consumer.ResponseTimeout = 100;

      int curThreadId = 0, lastThreadId, i = 0;
      do
      {
        lastThreadId = curThreadId;
        Message msg = Builder.newMessage(Message.InitMode.inited);
        msg.bson.append(DispatcherConstants.Command, "GetThreadId");
        Bson response = consumer.sendAndWait(msg);

        Iterator it = response.find("thread_id");
        curThreadId = (int)it;
        i++;
      }
      while (curThreadId == lastThreadId && i <= 50);
      // On tests running on debugger, it take a few cycles until a different thred picks the request
      // 50 loops is more then enough
      Assert.AreNotEqual(curThreadId, lastThreadId);
    }

    void Internal_MultithreadedMultiInstanceDispatchInterface(DispatchMode mode)
    {
      _dispatcher.Start(4, mode);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      Consumer consumer1 = _scope.newConsumer(_ps, "consumer_test", _service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      consumer1.ResponseTimeout = 5000;
      Consumer consumer2 = _scope.newConsumer(_ps, "consumer_test", _service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      consumer2.ResponseTimeout = 5000;

      Message msg = Builder.newMessage(Message.InitMode.inited);
      msg.bson.append(DispatcherConstants.Command, "GetInstancePointer");
      Message msg2 = Builder.newMessage(Message.InitMode.inited);

      msg2.bson.append(DispatcherConstants.Command, "GetInstancePointer");
      consumer1.send(msg);
      consumer2.send(msg2);

      Bson response = BsonDeserializer.CheckSvcBusException(consumer1.wait(msg.messageId));
      Iterator it = response.find("instance_pointer");
      Assert.NotNull(it);
      var curInstancePointer = (int)it;
      response = BsonDeserializer.CheckSvcBusException(consumer2.wait(msg2.messageId));
      it = response.find("instance_pointer");
      Assert.NotNull(it);
      var curInstancePointer2 = (int)it;

      if (mode == DispatchMode.SingletonDispatchInterface)
        Assert.AreEqual(curInstancePointer, curInstancePointer2);
      else
        Assert.AreNotEqual(curInstancePointer, curInstancePointer2);
    }

    [Test]
    public void MultiInstanceDispatchInterface()
    {
      Internal_MultithreadedMultiInstanceDispatchInterface(DispatchMode.MultiInstanceDispatchInterface);
    }

    [Test]
    public void SingletonDispatchInterface()
    {
      Internal_MultithreadedMultiInstanceDispatchInterface(DispatchMode.SingletonDispatchInterface);
    }

    [Test]
    public void NonExistingMethod()
    {
      var memLogger = new LoggerSpy();
      _dispatcher.Logger = memLogger;

      const string methodName = "unexisting_method";
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      Consumer consumer = _scope.newConsumer(_ps, "consumer_test", _service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      consumer.ResponseTimeout = 500;

      Message msg = Builder.newMessage(Message.InitMode.inited);
      msg.bson.append(DispatcherConstants.Command, methodName);

      Assert.Throws<MethodNotFoundException>(
        () => BsonDeserializer.CheckSvcBusException(consumer.sendAndWait(msg))
      );
      Assert.AreEqual(1, memLogger.LogEntries.Count);
      Assert.IsTrue(memLogger.LogEntries[0].StartsWith(string.Format("Method '{0}' was not found", methodName)));
      Assert.IsTrue(memLogger.LogEntries[0].EndsWith("Warning"));
    }

    private const uint MULTI_THREADED_TEST_CONSUMER_COUNT = 2;

    sealed class ConsumerJob : IDisposable
    {
      readonly Consumer[] _consumers;
      readonly Scope _scope;

      public string ErrorMessage { get; private set; }
      public int RequestsToSend { private get; set; }
      public int RequestsReceived { get; private set; }

      public ConsumerJob(Service service, PipeService ps)
      {
        RequestsReceived = 0;
        RequestsToSend = 500;
        _scope = new Scope();
        _consumers = new Consumer[MULTI_THREADED_TEST_CONSUMER_COUNT];
        for (int i = 0; i < _consumers.Length; i++)
        {
          _consumers[i] = _scope.newConsumer(ps, GetHashCode().ToString(CultureInfo.InvariantCulture), service,
            svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
          _consumers[i].ResponseTimeout = 4000;
        }
      }

      public void Run(object context)
      {
        var evt = (ManualResetEvent)context;
        Message msg = null;

        try
        {
          for (int i = 0; i < RequestsToSend; i++)
            foreach (Consumer consumer in _consumers)
            {
              msg = Builder.newMessage(Message.InitMode.inited);
              msg.bson.append(DispatcherConstants.Command, "Describe");
              msg.bson.append("trailing_data", "more data");
              var b = msg.bson.appendDocumentBegin("params");
              b.append("StrValue", "Hello");
              msg.bson.appendDocumentEnd(b);
              consumer.send(msg);
              Bson response = BsonDeserializer.CheckSvcBusException(consumer.wait(msg.messageId));
              Iterator it = response.find(DispatcherConstants.Commands);
              if (it.bsonType == bson_type.BSON_EOO)
                throw new Exception("\"methods\" array not found in response");
              RequestsReceived++;
            }
        }
        catch (Exception ex)
        {
          if(msg != null)
            Console.WriteLine(msg.bson.ToJson());
          Console.WriteLine(ex.StackTrace);
          var st = new StackTrace(ex, true);
          // Get the top stack frame
          var stackFrames = st.GetFrames();
          if (stackFrames != null)
          {
            var frame = stackFrames[0];
            // Get the line number from the stack frame
            ErrorMessage = string.Format("Error {0} during execution of ConsumerJob in file {1} line {2}", ex.Message, frame.GetFileName(), frame.GetFileLineNumber());
          }
        }
        finally
        {
          evt.Set();
        }
      }
      /*
      public void Run(object context)
      {
        for (int i = 0; i < RequestsToSend; i++)
          foreach (Consumer consumer in _consumers)
          {
            Message msg = Builder.newMessage(_ps.oidGenerator);
            msg.bson.append(DispatcherConstants.Command, "Describe");
            msg.bson.append("trailing_data", "more data");

            consumer.send(msg);
            Bson response = BaseBsonSerializer.CheckSvcBusException(consumer.wait(msg.messageId));
            Iterator it = _scope.add(response.find(DispatcherConstants.Commands));
            if(it.bsonType == bson_type.BSON_EOO)
              throw new Exception("\"methods\" array not found in response");
            RequestsReceived++;
          }
        var evt = (ManualResetEvent)context;
        evt.Set();
      }
      */
      ~ConsumerJob()
      {
        Dispose(false);
      }

      private void Dispose(bool disposing)
      {
        if (disposing)
          _scope.Dispose();
      }

      public void Dispose()
      {
        Dispose(true);
        GC.SuppressFinalize(this);
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), 
    TestCase(false),
    TestCase(true)]
    public void StressMultiThreadedDispatcherWithParametrizedSingleThreadedResponder(bool singleThreadedResponder)
    {
      const int threadsToRun = 8, requestsToSend = 500;
      _dispatcher.ReplySingleThreadedMode = singleThreadedResponder;
      _dispatcher.Start(threadsToRun, DispatchMode.MultiInstanceDispatchInterface);
      Thread.Sleep(2000); // sleep is to let the producer pick up first to get capped collection properly initialized

      using (var pool = new ThreadPool(threadsToRun, threadsToRun))
      {
        var jobs = new List<ConsumerJob>();
        var events = new WaitHandle[threadsToRun];
        for (var j = 0; j < threadsToRun; j++)
        {
          events[j] = new ManualResetEvent(false);
          var job = (ConsumerJob)_scope.add(new ConsumerJob(_service, _ps));
          job.RequestsToSend = requestsToSend;
          pool.QueueUserWorkItem(job.Run, events[j]);
          jobs.Add(job);
        }

        WaitHandle.WaitAll(events);
        foreach (var job in jobs)
        {
          if (job.ErrorMessage != null)
          {
            Assert.AreEqual("", job.ErrorMessage);
          }
          // MULTI_THREADED_TEST_CONSUMER_COUNT consumers
          Assert.AreEqual(requestsToSend * MULTI_THREADED_TEST_CONSUMER_COUNT, job.RequestsReceived);
        }
      }
    }

    [Test]
    public void UsingCustomRequestProcessingStrategy()
    {
      _dispatcher.RequestProcessingStrategy = new CustomRequestProcessingStrategy();
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      Consumer consumer = _scope.newConsumer(_ps, "consumer_test", _service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);

      Message msg = Builder.newMessage(Message.InitMode.inited);
      msg.bson.append(DispatcherConstants.Command, "Describe");
      consumer.send(msg);

      msg = Builder.newMessage(Message.InitMode.inited);
      msg.bson.append(DispatcherConstants.Command, "Describe");
      int start = Environment.TickCount;
      consumer.sendAndWait(msg);

      Assert.IsTrue(Environment.TickCount - start >= 450);
    }

    [Test]
    public void UsingCustomResponseProcessingStrategy()
    {
      _dispatcher.ResponseProcessingStrategy = new CustomResponseProcessingStrategy();
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      Consumer consumer = _scope.newConsumer(_ps, "consumer_test", _service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);

      Message msg = Builder.newMessage(Message.InitMode.inited);
      msg.bson.append(DispatcherConstants.Command, "Describe");
      Bson response = consumer.sendAndWait(msg);

      Iterator it = response.find("injected_field");
      Assert.AreNotEqual(bson_type.BSON_EOO, it.bsonType);
    }

    [Test]
    public void CallVersionMethod()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      Consumer consumer = _scope.newConsumer(_ps, "consumer_test", _service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      consumer.ResponseTimeout = 500;

      Message msg = Builder.newMessage(Message.InitMode.inited);
      msg.bson.append(DispatcherConstants.Command, "Version");
      Bson response = consumer.sendAndWait(msg);

      Iterator it = response.find("Version");
      Assert.AreNotEqual(bson_type.BSON_EOO, it.bsonType);
    }

    [Test]
    public void EchoUsingDispatchInterfaceConsumer_SendAndWait()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      DispatchInterfaceConsumer dic = _scope.newDispatchInterfaceConsumer(_ps, "consumer_test", _service,
                                                                          svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      dic.Consumer.ResponseTimeout = 100;

      var request = new SubObject { StrValue = "Hello World" };

      var response = (SubObject)dic.SendAndWait("Echo", request);
      Assert.AreEqual(request.StrValue, response.StrValue);
    }

    [Test]
    public void EchoUsingDispatchInterfaceConsumerFromPool_SendAndWait()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      ConsumerPoolDictionary dictionary = _scope.newConsumerPoolDictionary();
      ConsumerPool pool = dictionary.GetConsumerPool(Constants.ConnectionString, _service.Name, "test_consumer",
                                                      svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      pool.AutoStartHeartbeatTimer = 0;

      DispatchInterfaceConsumer dic = _scope.newDispatchInterfaceConsumer(pool);
      dic.Consumer.ResponseTimeout = 100;

      var request = new SubObject { StrValue = "Hello World" };

      var response = (SubObject)dic.SendAndWait("Echo", request);
      Assert.AreEqual(request.StrValue, response.StrValue);
    }

    [Test]
    public void BroadcastDispatchInterfaceConsumer_waitForResponseAsync()
    {
      using (var _dispatcher2 = _scope.newDispatcher("c_sharp_test_dispatcher_2", _service, _ps, typeof (TestDispatchInterface)))
      {
        _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
        _dispatcher2.Logger = new SimpleConsoleLogger();
        _dispatcher2.Start(1, DispatchMode.SingletonDispatchInterface);
        Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

        ConsumerPoolDictionary dictionary = _scope.newConsumerPoolDictionary();
        ConsumerPool pool = dictionary.GetConsumerPool(Constants.ConnectionString, _service.Name,
          "c_sharp_test_consumer",
          svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
        _scope.add(pool);
        pool.CopyConsumers = true;
        pool.startAsyncWorkPool(2, 2);
        pool.AutoStartHeartbeatTimer = 0;

        using (var dic = _scope.newDispatchInterfaceConsumer(pool))
        {
          var request = new SubObject {StrValue = "Hello World"};
          // ReSharper disable once NotAccessedVariable
          int responseCount = 0;
          bool responseReceived = false;
          var msg = dic.BuildRequestMessage("Echo", request);
          dic.Consumer.ResponseTimeout = 1000;
          msg.Broadcast = true; // Recommend to set Broadcast property to true before using WaitResponseAsync
          // Otherwise in case of timeout, system can't detect message was a broadcast
          // and internal method in C SvcBusConsumer_markBroadcastRequestAsTaken_internal won't be called
          // leaving an orphan broadcast message in the service pipe
          dic.WaitResponseAsync(null, msg, null,
            delegate(Bson response, Message message, IReplier replier, object userdata)
            {
              responseReceived = response.find("response") != null;
              responseCount++;
            });
          dic.SendBroadcast(msg);
          Thread.Sleep(200);
          Assert.IsTrue(responseReceived);
          Assert.AreEqual(2, responseCount);
          Thread.Sleep(4000);
          Assert.IsFalse(responseReceived);
          Assert.AreEqual(3, responseCount);
        }
      }
    }

    [Test]
    public void HybridMethodUsingDispatchInterfaceConsumerFromPool_waitForResponseAsync()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      ConsumerPoolDictionary dictionary = _scope.newConsumerPoolDictionary();
      ConsumerPool pool = dictionary.GetConsumerPool(Constants.ConnectionString, _service.Name, "test_consumer",
                                                      svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      _scope.add(pool);
      pool.AutoStartHeartbeatTimer = 0;

      /* Let's setup all objects required for an emulated external service upon which this method will depend */
      var service2 = Builder.newService("c_sharp_emulated_volatile", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);
      var ps2 = _scope.newPipeService(Constants.ConnectionString);
      using (var dispatcher2 = _scope.newDispatcher("c_sharp_test_dispatcher", service2, ps2, typeof(TestDispatchInterface))) {
        dispatcher2.Logger = new SimpleConsoleLogger();
        dispatcher2.Start(1, DispatchMode.SingletonDispatchInterface);
        Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized
        /* Emulated dispatcher started at this point */
        using (var dic = _scope.newDispatchInterfaceConsumer(pool))
        {
          var request = new SubObject {StrValue = "Hello World"};
          try
          {
            dic.SendAndWait("hybridMethodWithExceptionInCallback", request);
            Assert.Fail("Should have never reached this point");
          }
          catch (Exception e)
          {
            Assert.IsTrue(e.Message.Contains("Explosion"), "Expecting word explosion in exception message");
          }
        }
      }
    }

    [Test]
    public void Dispatcher_Verify_ProducerJobRunning()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500);
      Assert.IsTrue(_dispatcher.ProducerJobRunning);
      _dispatcher.Dispose();
      Thread.Sleep(500);
      Assert.IsFalse(_dispatcher.ProducerJobRunning);
    }

    [Test]
    public void DispatchInterfaceMethodReturnNull_Success()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      ConsumerPoolDictionary dictionary = _scope.newConsumerPoolDictionary();
      ConsumerPool pool = dictionary.GetConsumerPool(Constants.ConnectionString, _service.Name, "test_consumer",
                                                      svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      pool.AutoStartHeartbeatTimer = 0;

      DispatchInterfaceConsumer dic = _scope.newDispatchInterfaceConsumer(pool);
      dic.Consumer.ResponseTimeout = 100;

      var request = new SubObject { StrValue = "Hello World" };
      Assert.That(() => dic.SendAndWait("MsgNoResponse", request), Throws.TypeOf<ConsumerException>());
    }

    [Test]
    public void waitForResponseDirectlyFromMethod_Success()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      ConsumerPoolDictionary dictionary = _scope.newConsumerPoolDictionary();
      ConsumerPool pool = dictionary.GetConsumerPool(Constants.ConnectionString, _service.Name, "test_consumer",
                                                      svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      pool.AutoStartHeartbeatTimer = 0;

      DispatchInterfaceConsumer dic = _scope.newDispatchInterfaceConsumer(pool);

      var request = new SubObject { StrValue = "Hello World" };

      var response = dic.SendAndWait("MsgReturnNoResponse_ReturnDirect", request);
      Assert.IsTrue(response != null);
      var responseStr = response as string;
      Assert.IsTrue(responseStr != null);
      Assert.IsTrue(responseStr.Contains("the_data"));
    }

    [Test]
    public void EchoUsingDispatchInterfaceConsumerFromPool_SendWithNoResponsePipe()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      ConsumerPoolDictionary dictionary = _scope.newConsumerPoolDictionary();
      ConsumerPool pool = dictionary.GetConsumerPool(Constants.ConnectionString, _service.Name, "test_consumer", 0);

      DispatchInterfaceConsumer dic = _scope.newDispatchInterfaceConsumer(pool);

      var request = new SubObject { StrValue = "Hello World" };

      dic.Send("Echo", request);
    }

    [Test]
    public void EchoSendingAnonymousTypeAndReceiveAnonymousType()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      ConsumerPoolDictionary dictionary = _scope.newConsumerPoolDictionary();
      ConsumerPool pool = dictionary.GetConsumerPool(Constants.ConnectionString, _service.Name, "test_consumer", svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      pool.startAsyncWorkPool(2, 2);

      DispatchInterfaceConsumer dic = _scope.newDispatchInterfaceConsumer(pool);

      dynamic result = dic.SendAndWait("ReceiveAndReturnAnonymousObject", new { StrValue = "Hello World" });
      Assert.AreEqual("Hello World", result.StrValue);
    }

    [Test]
    public void EchoSendingAnonymousTypeAndReceiveAnonymousTypeWithType()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      ConsumerPoolDictionary dictionary = _scope.newConsumerPoolDictionary();
      ConsumerPool pool = dictionary.GetConsumerPool(Constants.ConnectionString, _service.Name, "test_consumer", svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      pool.startAsyncWorkPool(2, 2);

      DispatchInterfaceConsumer dic = _scope.newDispatchInterfaceConsumer(pool);

      SubObject result = dic.SendAndWait("ReceiveAndReturnAnonymousObjectWithType", new { StrValue = "Hello World" }) as SubObject;
      Assert.AreNotEqual(null, result);
      // ReSharper disable once PossibleNullReferenceException
      Assert.AreEqual("Hello World", result.StrValue);
    }

    [Test]
    public void EchoUsingDispatchInterfaceConsumer_Send_And_Then_Wait()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      DispatchInterfaceConsumer dic = _scope.newDispatchInterfaceConsumer(_ps, "consumer_test", _service,
                                                                          svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);

      var request = new SubObject { StrValue = "Hello World" };

      // Two rounds
      for (int i = 0; i < 2; i++)
      {
        Oid id = dic.Send("Echo", request);
        var response = (SubObject)dic.Wait(id);
        Assert.AreEqual(request.StrValue, response.StrValue);
      }

      // Now using waitMultiple
      var oids = new[] { dic.Send("Echo", request), dic.Send("Echo", request) };
      // ReSharper disable once UnusedVariable
      foreach (var oid in oids)
      {
        var response = (SubObject)dic.WaitMultiple(oids);
        Assert.AreEqual(request.StrValue, response.StrValue);
      }
    }

    [Test]
    public void EchoUsingDispatchInterfaceConsumer_SendBroadcast_And_Then_WaitBroadcast()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      DispatchInterfaceConsumer dic = _scope.newDispatchInterfaceConsumer(_ps, "consumer_test", _service,
                                                                          svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      dic.Consumer.ResponseTimeout = 100;

      var request = new SubObject { StrValue = "Hello World" };
      Oid id = dic.SendBroadcast("Echo", request);

      var response = (SubObject)dic.WaitBroadcast(id);
      Assert.AreEqual(request.StrValue, response.StrValue);
    }
    
    [Test]
    public void EchoUsingRequestDeserializer_Stress()
    {
      _dispatcher.Start(2, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      Consumer consumer = _scope.newConsumer(_ps, "consumer_test", _service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      consumer.ResponseTimeout = 500;

      for (int i = 0; i < 1000; i++)
      {
        Message msg = Builder.newMessage(Message.InitMode.inited);
        msg.bson.append(DispatcherConstants.Command, "Echo");

        Bson sub = msg.bson.appendDocumentBegin(DispatcherConstants.Params);
        sub.append("StrValue", "hello");
        msg.bson.appendDocumentEnd(sub);
        Bson response = BsonDeserializer.CheckSvcBusException(consumer.sendAndWait(msg));

        Iterator it = response.find(DispatcherConstants.Response);
        Assert.AreEqual(bson_type.BSON_OBJECT, it.bsonType);
        Iterator subit = Builder.newIterator(it);

        Assert.IsTrue(subit.next());
        Assert.AreEqual(DispatcherConstants.ActualType, subit.key);
        Assert.IsTrue(subit.next());
        Assert.AreEqual("StrValue", subit.key);
        Assert.AreEqual("hello", (string)subit);
      }
    }

    [Test]
    public void SerializedObject()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

      Consumer consumer = _scope.newConsumer(_ps, "consumer_test", _service, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      consumer.ResponseTimeout = 100;

      Message msg = Builder.newMessage(Message.InitMode.inited);
      msg.bson.append(DispatcherConstants.Command, "ReturnObject");

      Bson response = BsonDeserializer.CheckSvcBusException(consumer.sendAndWait(msg));
      Iterator it = response.find(DispatcherConstants.Response);
      Assert.AreEqual(bson_type.BSON_OBJECT, it.bsonType);
      it = Builder.newIterator(it);

      Assert.IsTrue(it.next());
      Assert.AreEqual(DispatcherConstants.ActualType, it.key);

      Assert.IsTrue(it.next());
      Assert.AreEqual("IntValue", it.key);
      Assert.AreEqual(10, (int)it);

      Assert.IsTrue(it.next());
      Assert.AreEqual("StrList", it.key);
      Iterator subit = Builder.newIterator(it);
      {
        Assert.IsTrue(subit.next());
        Assert.AreEqual("hello", (string)subit);
        Assert.IsTrue(subit.next());
        Assert.AreEqual("world", (string)subit);
      }

      Assert.IsTrue(it.next());
      Assert.AreEqual("SubObject", it.key);
      subit = Builder.newIterator(it);
      {
        Assert.IsTrue(subit.next());
        Assert.AreEqual(DispatcherConstants.ActualType, subit.key);

        Assert.IsTrue(subit.next());
        Assert.AreEqual("str value テ", (string)subit);
      }

      Assert.False(it.next());
    }

    [Test]
    public void PersistService()
    {
      var randomServiceName = DateTime.Now.ToString(CultureInfo.InvariantCulture);
      var randomSerivce = Builder.newService(randomServiceName, svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);

      var presistance = Builder.newServicePersistence(_ps); // should not be persisted
      var e = Assert.Throws<ServicePersistenceException>(
        () => presistance.Load(randomServiceName)
      );
      Assert.AreEqual(service_persistence_err.SERVICE_NOT_FOUND, e.ErrorCode);

      using (_scope.newDispatcher("c_sharp_test_dispatcher", randomSerivce, _ps, typeof (TestDispatchInterface)))
      {
        presistance.Load(randomServiceName); // should be persisted
      }
    }

    [Test]
    public void TestEchoBurstUsingDispatchInterfaceConsumer()
    {
      _dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);
      Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized
      var Consumer =
      _scope.newDispatchInterfaceConsumer(_ps, "consumer_test", _service,
        svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      Consumer.Consumer.ResponseTimeout = 200;
      uint StartTime = (uint)Environment.TickCount;
      var Req = new SubObject { StrValue = "Hello World" };
      // ReSharper disable once InconsistentNaming
      var IDs = new Oid[10];
      for (var i = 0; i < 10; i++)
      {
        IDs[i] = Consumer.Send("EchoWithSmallWait", Req);
      }
      for (var i = 0; i < 10; i++)
      {
        var Resp = (SubObject)Consumer.Wait(IDs[i]);
        Assert.AreEqual(Req.StrValue, Resp.StrValue);
      }
      Assert.IsTrue((uint)Environment.TickCount - StartTime < 2000,
        "Response should be nearly instantaneous. Took too much time to process all requests");
    }

    private class BusyDispatchInterface : DispatchInterface
    {
      private readonly ManualResetEvent _event;

      public BusyDispatchInterface(object userData)
        : base(userData)
      {
        _event = (ManualResetEvent)userData;
      }

      // ReSharper disable once UnusedMember.Local
      public object VeryLongMethod(Message msg)
      {
        _event.WaitOne(15 * 1000);

        return new SubObject();
      }

      // ReSharper disable once UnusedMember.Local
      public object PriorityMethod(Message msg)
      {
        _event.Set();

        return new SubObject();
      }
    }

    [Test]
    public void PriorityRequestsShouldBeProcessed()
    {
      var svcOnlyInsert = Builder.newService("test_service_only_insert", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);
      const int workersCount = 2;
      using (var stopEvent = new ManualResetEvent(false))
      {
        var localDispatcher = _scope.newDispatcher("c_sharp_test_dispatcher", svcOnlyInsert, _ps, typeof(BusyDispatchInterface));
        localDispatcher.Logger = new SimpleConsoleLogger();
        localDispatcher.Start(workersCount, DispatchMode.MultiInstanceDispatchInterface, stopEvent);
        Thread.Sleep(500); // sleep is to let the producer pick up first to get capped collection properly initialized

        var dic = _scope.newDispatchInterfaceConsumer(_ps, "consumer_test", svcOnlyInsert,
          svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);

        try
        {
          // force all workers to be busy
          for (int i = 0; i < workersCount; i++)
            dic.Send("VeryLongMethod", new SubObject());

          Assert.Throws<ConsumerException>(() => dic.SendAndWait("VeryLongMethod", new SubObject()),
            "common request should be timed out");
          Assert.DoesNotThrow(() => dic.SendAndWait("PriorityMethod", new SubObject(), true), "priority request should be processed");
        }
        finally
        {
          stopEvent.Set();
          localDispatcher.Dispose();
          _ps.dropServicePipe(svcOnlyInsert);
        }
      }
    }

    public void Dispose()
    {
      if (_ps == null) 
        return;
      _ps.Dispose();
      _scope.Dispose();
      _service = null;
      _ps = null;
    }
  }
}