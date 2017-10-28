using System;
using NUnit.Framework;
using System.Runtime.InteropServices;
using Sovos.SvcBus;

[assembly: GTestStyleConsoleOutputAttribute]
namespace SvcBusTests
{
  [TestFixture, GTestStyleConsoleOutputAttribute]
  public class SvcBusGeneralApiTests : IDisposable
  {
    IntPtr ps;

    ~SvcBusGeneralApiTests()
    {
      Dispose(false); 
    }

    [SetUp]
    public void SetUp()
    {
      ps = NativeMethods.SvcBusPipeService_alloc();      
      Assert.AreEqual(0, NativeMethods.SvcBusPipeService_init(ps, Constants.ConnectionString));      
    }

    [TearDown]
    public void TearDown()
    {
      NativeMethods.SvcBusPipeService_destroy(ps);     
      NativeMethods.SvcBusPipeService_dealloc(ps);      
    }

    [Test]
    public void ServiceBusOidGen()
    {
      var oid_p = NativeMethods.ServiceBusOid_alloc();
      try
      {
        NativeMethods.bson_oid_init(oid_p);
        var oid = new Oid(oid_p);
        Assert.AreNotEqual(0, oid.Int(0));
        Assert.AreNotEqual(0, oid.Int(1));
        /* We will not check int3 in purpose. Because of how OIDs are generated, first time function is used third integer is zero */

        NativeMethods.bson_oid_init(oid_p);
        oid = new Oid(oid_p);
        Assert.AreNotEqual(0, oid.Int(0));
        Assert.AreNotEqual(0, oid.Int(1));
        Assert.AreNotEqual(0, oid.Int(2)); // Second pass int3 should be != 0
      }
      finally
      {
        NativeMethods.ServiceBusOid_dealloc(oid_p);
      }
    }

    [Test]
    public void OidComparisonTest()
    {
      var oid1_p = NativeMethods.ServiceBusOid_alloc();
      var oid2_p = NativeMethods.ServiceBusOid_alloc();
      NativeMethods.bson_oid_init(oid1_p);
      NativeMethods.bson_oid_init(oid2_p);
      Assert.AreEqual(1, NativeMethods.SvcBus_isBsonOidEqual(oid1_p, oid1_p));
      Assert.AreNotEqual(1, NativeMethods.SvcBus_isBsonOidEqual(oid1_p, oid2_p));
      NativeMethods.ServiceBusOid_dealloc(oid1_p);
      NativeMethods.ServiceBusOid_dealloc(oid2_p);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
      Dispose(true);
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"), TestFixture, GTestStyleConsoleOutputAttribute]
  public class SvcBusMessageApiTests : IDisposable
  {
    private IntPtr msg;
    private IntPtr ps;

    [SetUp]
    public void SetUp()
    {
      msg = NativeMethods.SvcBusMessage_alloc();
      Assert.AreNotEqual(IntPtr.Zero, msg);
      ps = NativeMethods.SvcBusPipeService_alloc();
      Assert.AreEqual(0, NativeMethods.SvcBusPipeService_init(ps, Constants.ConnectionString));
    }

    [TearDown]
    public void TearDown()
    {
      if (!msg.Equals(IntPtr.Zero))
      {
        NativeMethods.SvcBusMessage_dealloc(msg);
      }
      NativeMethods.SvcBusPipeService_destroy(ps);
      NativeMethods.SvcBusPipeService_dealloc(ps);
    }

    ~SvcBusMessageApiTests()
    {
      Dispose(false);
    }

    [Test]
    public void SvcBusMessage_AllocAndDealloc()
    {
      Assert.AreNotEqual(IntPtr.Zero, msg);
    }

    [Test]
    public void SvcBusMessage_InitCopy()
    {
      NativeMethods.SvcBusMessage_init(msg, IntPtr.Zero);
      var oidP = NativeMethods.SvcBusMessage_get__id(msg);
      var oid = new Oid(oidP);
      var b = NativeMethods.SvcBusMessage_getBson(msg);
      NativeMethods.bson_append_int32(b, "test", -1, 12345);

      var msg1 = NativeMethods.SvcBusMessage_alloc();
      NativeMethods.SvcBusMessage_init_copy(msg1, msg);
      var oidP1 = NativeMethods.SvcBusMessage_get__id(msg1);
      var oid1 = new Oid(oidP1);
      Assert.AreEqual(oid.Int(0), oid1.Int(0));
      Assert.AreEqual(oid.Int(1), oid1.Int(1));
      //svm when bson copy is fixed enable this check, it fails so far
      //Assert.AreEqual(Marshal.ReadIntPtr(b), Marshal.ReadIntPtr(b1));

      NativeMethods.SvcBusMessage_destroy(msg);

      Assert.AreNotEqual(0, oid1.Int(0));
      Assert.AreNotEqual(0, oid1.Int(1)); 
      NativeMethods.SvcBusMessage_destroy(msg1);
    }

    [Test]
    public void SvcBusMessage_InitAndDestroy()
    {
      NativeMethods.SvcBusMessage_init(msg, IntPtr.Zero);
      NativeMethods.SvcBusMessage_destroy(msg);
    }

    [Test]
    public void SvcBusMessage_getBson()
    {
      NativeMethods.SvcBusMessage_init(msg, IntPtr.Zero);
      IntPtr b = NativeMethods.SvcBusMessage_getBson(msg);
      Assert.AreNotEqual(IntPtr.Zero, b);
      NativeMethods.SvcBusMessage_destroy(msg);
    }

    [Test]
    public void SvcBusMessage_SvcBusMessage_get__id()
    {
      NativeMethods.SvcBusMessage_init(msg, IntPtr.Zero);
      
      var oid_p = NativeMethods.SvcBusMessage_get__id(msg);
      Assert.AreNotEqual(IntPtr.Zero, oid_p);

      var oid = new Oid(oid_p);

      /* For sure the first two integers of OID must be != 0 */
      Assert.AreNotEqual(0, oid.Int(0));
      Assert.AreNotEqual(0, oid.Int(1));      
      NativeMethods.SvcBusMessage_destroy(msg);
    }

    [Test]
    public void SvcBusMessage_SvcBusMessage_get_messageId()
    {
      NativeMethods.SvcBusMessage_init(msg, IntPtr.Zero);

      var oid_p = NativeMethods.SvcBusMessage_get_messageId(msg);
      Assert.AreNotEqual(IntPtr.Zero, oid_p);

      var oid = new Oid(oid_p);

      /* For sure the first two integers of OID must be != 0 */
      Assert.AreNotEqual(0, oid.Int(0));
      Assert.AreNotEqual(0, oid.Int(1));
      NativeMethods.SvcBusMessage_destroy(msg);
    }

    [Test]
    public void SvcBusMessage_SvcBusMessage_get_responsePipeName()
    {
      NativeMethods.SvcBusMessage_init(msg, IntPtr.Zero);

      var s = Marshal.PtrToStringAnsi(NativeMethods.SvcBusMessage_get_responsePipeName(msg));
      Assert.AreEqual("", s);

      NativeMethods.SvcBusMessage_destroy(msg);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
    
    public void Dispose()
    {
      Dispose(true);
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"), TestFixture, GTestStyleConsoleOutputAttribute]
  public class SvcBusPipeServiceApiTests : IDisposable
  {
    private IntPtr ps;

    [SetUp]
    public void SetUp()
    {
      ps = NativeMethods.SvcBusPipeService_alloc();
      Assert.AreNotEqual(IntPtr.Zero, ps);
    }

    [TearDown]
    public void TearDown()
    {
      if (!ps.Equals(IntPtr.Zero))
      {
        NativeMethods.SvcBusPipeService_dealloc(ps);
      }
    }

    ~SvcBusPipeServiceApiTests()
    {
      Dispose(false);
    }

    [Test]
    public void SvcBusPipeService_AllocAndDealloc()
    {
      Assert.AreNotEqual(IntPtr.Zero, ps);
    }

    [Test]
    public void SvcBusPipeService_Init()
    {
      Assert.AreEqual(0, NativeMethods.SvcBusPipeService_init(ps, Constants.ConnectionString));
      NativeMethods.SvcBusPipeService_destroy(ps);
    }

    [Test]
    public void SvcBusPipeService_releaseDeadResponsePipes()
    {
      Assert.AreEqual(0, NativeMethods.SvcBusPipeService_init(ps, Constants.ConnectionString));
      NativeMethods.SvcBusPipeService_releaseDeadResponsePipes(ps, 2000);
      NativeMethods.SvcBusPipeService_destroy(ps);
    }

    [Test]
    public void SvcBusPipeSercice_testErrorAPIs()
    {
      Assert.AreEqual(0, NativeMethods.SvcBusPipeService_init(ps, Constants.ConnectionString));
      Assert.AreEqual(0, NativeMethods.SvcBusPipeService_getlastErrorCode(ps));
      Assert.AreEqual("", Marshal.PtrToStringAnsi(NativeMethods.SvcBusPipeService_getlastErrorMsg(ps)));
      NativeMethods.SvcBusPipeService_destroy(ps);

      Assert.AreNotEqual(0, NativeMethods.SvcBusPipeService_init(ps, "mongodb://bad_ip/testdb"));
      Assert.AreNotEqual(0, NativeMethods.SvcBusPipeService_getlastErrorCode(ps));
      Assert.AreNotEqual("", Marshal.PtrToStringAnsi(NativeMethods.SvcBusPipeService_getlastErrorMsg(ps)));
      NativeMethods.SvcBusPipeService_destroy(ps);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
      Dispose(true);
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"), TestFixture, GTestStyleConsoleOutputAttribute]
  public class SvcBusServiceApiTests : IDisposable
  {
    private IntPtr svc;

    [SetUp]
    public void SetUp()
    {
      svc = NativeMethods.SvcBusService_alloc();
      NativeMethods.SvcBusService_init(svc, "service_test", (int)svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE, 1024 * 1024, 1024);
    }

    [TearDown]
    public void TearDown()
    {
      if (!svc.Equals(IntPtr.Zero))
      {
        NativeMethods.SvcBusService_destroy(svc);
        NativeMethods.SvcBusService_dealloc(svc);
      }
    }

    ~SvcBusServiceApiTests()
    {
      Dispose(false);
    }

    [Test]
    public void SvcBusService_CheckCreated()
    {
      Assert.AreNotEqual(null, svc);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
    
    public void Dispose()
    {
      Dispose(true);
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"), TestFixture, GTestStyleConsoleOutputAttribute]
  public class SvcBusConsumerApiTests : IDisposable
  {
    private IntPtr cs, ps, svc, msg;

    [SetUp]
    public void SetUp()
    {
      msg = IntPtr.Zero;
      svc = NativeMethods.SvcBusService_alloc();
      ps = NativeMethods.SvcBusPipeService_alloc();
      cs = NativeMethods.SvcBusConsumer_alloc();
      NativeMethods.SvcBusService_init(svc, "test_svc", (int)svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE, 2048, 2048);
      NativeMethods.SvcBusPipeService_init(ps, Constants.ConnectionString);
      Assert.AreEqual(0, NativeMethods.SvcBusConsumer_init(cs, ps, "consumer", svc, (int)svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE));
    }

    [TearDown]
    public void TearDown()
    {
      NativeMethods.SvcBusConsumer_destroy(cs);
      NativeMethods.SvcBusPipeService_destroy(ps);
      NativeMethods.SvcBusService_destroy(svc);
      NativeMethods.SvcBusConsumer_dealloc(cs);
      NativeMethods.SvcBusPipeService_dealloc(ps);
      NativeMethods.SvcBusService_dealloc(svc);
      if (msg == IntPtr.Zero) return;
      NativeMethods.SvcBusMessage_destroy(msg);
      NativeMethods.SvcBusMessage_dealloc(msg);
    }

    ~SvcBusConsumerApiTests()
    {
      Dispose(false);
    }

    [Test]
    public void TestConsumerCreated()
    {
      Assert.AreNotEqual(IntPtr.Zero, cs);
      Assert.AreEqual(0, NativeMethods.SvcBusConsumer_getlastErrorCode(cs));
    }

    [Test]
    public void TestConsumerSend()
    {
      msg = NativeMethods.SvcBusMessage_alloc();
      NativeMethods.SvcBusMessage_init(msg, IntPtr.Zero);
      Assert.AreEqual(0, NativeMethods.SvcBusConsumer_send(cs, msg));
      Assert.AreEqual(0, NativeMethods.SvcBusConsumer_getlastErrorCode(cs));
    }

    [Test]
    public void TestConsumerUpdateHeartbeat()
    {
      NativeMethods.SvcBusConsumer_updateHeartbeat(cs);
      Assert.AreEqual(0, NativeMethods.SvcBusConsumer_getlastErrorCode(cs));
    }

    [Test]
    public void TestConsumerErrorReporting()
    {
      Assert.AreEqual(0, NativeMethods.SvcBusConsumer_getlastErrorCode(cs));
      Assert.AreEqual("", Marshal.PtrToStringAnsi(NativeMethods.SvcBusConsumer_getlastErrorMsg(cs)));
      /* Let's cause some kind of error. Let's use a pipeservice that failed initializing */
      var _ps = NativeMethods.SvcBusPipeService_alloc();
      var _cs = NativeMethods.SvcBusConsumer_alloc();
      Assert.AreNotEqual(0, NativeMethods.SvcBusPipeService_init(_ps, "mongodb://bad_ip/testdb"));
      Assert.AreNotEqual(0, NativeMethods.SvcBusConsumer_init(_cs, _ps, "consumer", svc, (int)svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE));
      Assert.AreNotEqual((int)svc_bus_consumer_err_t.SERVICE_BUS_CONSUMER_OK, NativeMethods.SvcBusConsumer_getlastErrorCode(_cs));
      Assert.AreNotEqual("", Marshal.PtrToStringAnsi(NativeMethods.SvcBusConsumer_getlastErrorMsg(_cs)));
      NativeMethods.SvcBusConsumer_destroy(_cs);
      NativeMethods.SvcBusPipeService_destroy(_ps);
      NativeMethods.SvcBusConsumer_dealloc(_cs); 
      NativeMethods.SvcBusPipeService_dealloc(_ps);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
      Dispose(true);
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"), TestFixture, GTestStyleConsoleOutputAttribute]
  public class SvcBusProducerApiTests : IDisposable
  {
    private IntPtr producer, ps, svc, msg;

    [SetUp]
    public void SetUp()
    {
      msg = IntPtr.Zero;
      ps = IntPtr.Zero;
      producer = IntPtr.Zero;
      svc = NativeMethods.SvcBusService_alloc();
      NativeMethods.SvcBusService_init(svc, "test_svc_2", (int)svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE, 2048, 2048);
      ps = NativeMethods.SvcBusPipeService_alloc();
      Assert.AreEqual(0, NativeMethods.SvcBusPipeService_init(ps, Constants.ConnectionString));
      producer = NativeMethods.SvcBusProducer_alloc();
      Assert.AreEqual(0, NativeMethods.SvcBusProducer_init(producer, ps, "producer", svc));
    }

    [TearDown]
    public void TearDown()
    {
      if (producer != IntPtr.Zero)
      {
        NativeMethods.SvcBusProducer_destroy(producer);
        NativeMethods.SvcBusProducer_dealloc(producer);
      }
      NativeMethods.SvcBusPipeService_destroy(ps);
      NativeMethods.SvcBusPipeService_dealloc(ps);

      NativeMethods.SvcBusService_destroy(svc);
      NativeMethods.SvcBusService_dealloc(svc);
      
      if (msg == IntPtr.Zero) return;
      NativeMethods.SvcBusMessage_destroy(msg);
      NativeMethods.SvcBusMessage_dealloc(msg);
    }

    ~SvcBusProducerApiTests()
    {
      Dispose(false);
    }
    [Test]
    public void TestProducerCreated()
    {
      Assert.AreNotEqual(IntPtr.Zero, producer);
      Assert.AreEqual(0, NativeMethods.SvcBusProducer_getlastErrorCode(producer));
    }

    [Test]
    public void TestProducerSend()
    {
      msg = NativeMethods.SvcBusMessage_alloc();
      var msgId = NativeMethods.ServiceBusOid_alloc();
      NativeMethods.bson_oid_init(msgId);
      NativeMethods.SvcBusMessage_init(msg, msgId);
      Assert.AreEqual(0, NativeMethods.SvcBusResponder_send(NativeMethods.SvcBusProducer_getResponder(producer), "any_pipe", msg));
    }

    [Test]
    public void TestProducerTake_Fail()
    {
      msg = NativeMethods.SvcBusMessage_alloc();
      var msgId = NativeMethods.ServiceBusOid_alloc();
      NativeMethods.bson_oid_init(msgId);
      NativeMethods.SvcBusMessage_init(msg, msgId);
      Assert.AreNotEqual(0, NativeMethods.SvcBusProducer_take(producer, msg));
    }

    [Test]
    public void TestProducerWait_Fail()
    {
      msg = NativeMethods.SvcBusMessage_alloc();
      NativeMethods.SvcBusProducer_setRequestPipeTimeout(producer, 1000);
      Assert.AreNotEqual(0, NativeMethods.SvcBusProducer_wait(producer, msg));
      Assert.AreEqual(svc_bus_producer_err_t.SERVICE_BUS_PRODUCER_WAIT_EXCEEDED, (svc_bus_producer_err_t)NativeMethods.SvcBusProducer_getlastErrorCode(producer));
      Assert.AreEqual("Max wait time exceeded", Marshal.PtrToStringAnsi(NativeMethods.SvcBusProducer_getlastErrorMsg(producer)));
    }

    protected virtual void Dispose(bool disposing)
    {
    }
    
    public void Dispose()
    {
      Dispose(true);
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"), TestFixture, GTestStyleConsoleOutputAttribute]
  public class SvcBusProducerAndConsumerApiTests : IDisposable
  {
    private IntPtr producer, cs, ps, svc, msg1, msg2, msg3;

    [SetUp]
    public void SetUp()
    {
      msg1 = IntPtr.Zero;
      msg2 = IntPtr.Zero;
      msg3 = IntPtr.Zero;
      svc = NativeMethods.SvcBusService_alloc();
      ps = NativeMethods.SvcBusPipeService_alloc();
      producer = NativeMethods.SvcBusProducer_alloc();
      cs = NativeMethods.SvcBusConsumer_alloc();
      NativeMethods.SvcBusService_init(svc, "test_svc_3", (int)svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE, 2048, 2048);
      Assert.AreEqual(0, NativeMethods.SvcBusPipeService_init(ps, Constants.ConnectionString));
      Assert.AreEqual(0, NativeMethods.SvcBusProducer_init(producer, ps, "producer", svc));
      Assert.AreEqual(0, NativeMethods.SvcBusConsumer_init(cs, ps, "consumer", svc, (int)svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE));
    }

    [TearDown]
    public void TearDown()
    {
      NativeMethods.SvcBusConsumer_destroy(cs);
      NativeMethods.SvcBusProducer_destroy(producer);
      NativeMethods.SvcBusPipeService_destroy(ps);
      NativeMethods.SvcBusService_destroy(svc);
      NativeMethods.SvcBusConsumer_dealloc(cs);
      NativeMethods.SvcBusProducer_dealloc(producer);
      NativeMethods.SvcBusPipeService_dealloc(ps);
      NativeMethods.SvcBusService_dealloc(svc);
      if (msg1 != IntPtr.Zero)
      {
        NativeMethods.SvcBusMessage_destroy(msg1);
        NativeMethods.SvcBusMessage_dealloc(msg1);
      }
      if (msg2 != IntPtr.Zero)
      {
        NativeMethods.SvcBusMessage_destroy(msg2);
        NativeMethods.SvcBusMessage_dealloc(msg2);
      }
      if (msg3 != IntPtr.Zero)
      {
        NativeMethods.SvcBusMessage_destroy(msg3);
        NativeMethods.SvcBusMessage_dealloc(msg3);
      }
    }

    ~SvcBusProducerAndConsumerApiTests()
    {
      Dispose(false);
    }

    [Test]
    public void TestCheckConsumerAndProducerCreated()
    {
      Assert.AreNotEqual(IntPtr.Zero, producer);
      Assert.AreNotEqual(IntPtr.Zero, cs);
    }

    [Test]
    public void TestFullLoop()
    {
      /* Send message thru consumer */
      var result = IntPtr.Zero;
      msg1 = NativeMethods.SvcBusMessage_alloc();
      NativeMethods.SvcBusMessage_init(msg1, IntPtr.Zero);
      Assert.AreEqual(0, NativeMethods.SvcBusConsumer_send(cs, msg1));
      
      /* Producer wait for message */
      msg2 = NativeMethods.SvcBusMessage_alloc();
      Assert.AreEqual(0, NativeMethods.SvcBusProducer_wait(producer, msg2));

      /* We got a message, let's test peek into the message for fun */
      var peek_result = NativeMethods.bson_new();
      Assert.AreEqual(0, NativeMethods.SvcBusProducer_peek(producer, peek_result, NativeMethods.SvcBusMessage_get__id(msg2)));
      NativeMethods.bson_destroy(peek_result);
      peek_result = NativeMethods.bson_new();
      /* Let's make a call to peek we know will fail */
      Assert.AreNotEqual(0, NativeMethods.SvcBusProducer_peek(producer, peek_result, NativeMethods.SvcBusMessage_get_messageId(msg2)));
      NativeMethods.bson_destroy(peek_result);

      /* Producer takes the message */
      Assert.AreEqual(0, NativeMethods.SvcBusProducer_take(producer, msg2));
      
      /* Prepare the response message and send back to consumer */
      msg3 = NativeMethods.SvcBusMessage_alloc();
      NativeMethods.SvcBusMessage_init(msg3, NativeMethods.SvcBusMessage_get_messageId(msg2));
      var bson_msg = NativeMethods.SvcBusMessage_getBson(msg3);
      Assert.AreEqual(1, NativeMethods.bson_append_int32(bson_msg, "data", -1, 111));
      Assert.AreEqual(0, NativeMethods.SvcBusResponder_send(NativeMethods.SvcBusProducer_getResponder(producer), Marshal.PtrToStringAnsi(NativeMethods.SvcBusMessage_get_responsePipeName(msg2)), msg3));
      
      /* Consumer waits for response */
      Assert.AreEqual(0, NativeMethods.SvcBusConsumer_wait(cs, ref result, NativeMethods.SvcBusMessage_get_messageId(msg1)));

      /* We got a response, let's do some checks to make sure we got what we expected */
      bson_msg = NativeMethods.SvcBusMessage_getBson(msg3);
      var it = NativeMethods.bson_iter_alloc();
      Assert.AreEqual(1, NativeMethods.bson_iter_init_find(it, bson_msg, "data"));
      Assert.AreEqual(111, NativeMethods.bson_iter_int32(it));
      NativeMethods.bson_iter_dealloc(it);
      
      /* Final error checks on producer and consumer error flags */
      Assert.AreEqual(0, NativeMethods.SvcBusProducer_getlastErrorCode(producer));
      Assert.AreEqual(0, NativeMethods.SvcBusConsumer_getlastErrorCode(cs));
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
      Dispose(true);
    }
  }

}
