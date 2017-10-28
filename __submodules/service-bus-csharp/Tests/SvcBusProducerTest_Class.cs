using Sovos.SvcBus;
using NUnit.Framework;

namespace SvcBusTests
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture, GTestStyleConsoleOutputAttribute]
  class SvcBusProducerTests
  {
    private Scope _scope;
    private Service svc_volatile, svc_durable;
    private PipeService ps;
    private Producer producer_durable;

    [SetUp]
    public void SetUp()
    {
      _scope = new Scope();
      ps = _scope.newPipeService(Constants.ConnectionString);

      svc_volatile = Builder.newService("test_service_volatile_2", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE, 1024, 1024);
      svc_durable = Builder.newService("test_service_durable_2", svc_bus_queue_mode_t.SERVICE_BUS_DURABLE);

      producer_durable = _scope.newProducer(ps, "producer", svc_durable);
    }

    [TearDown]
    public void TearDown()
    {
      _scope.Dispose();
      svc_volatile = null;
      svc_durable = null;
      ps = null;
      producer_durable = null;
    }

    private Producer newProducer()
    {
      return _scope.newProducer(ps, "producer", svc_volatile);
    }

    [Test]
    public void TestCreateProducer()
    {
      var producer = newProducer();
      Assert.AreNotEqual(null, producer);
    }

    [Test]
    public void TestProducerSend()
    {
      var producer = newProducer();
      var msg = Builder.newMessage(Message.InitMode.inited);
      producer.responder.send("bla_bla", msg);
    }

    [Test]
    public void TestProducerTake_Fail()
    {
      var producer = newProducer();
      var msg = Builder.newMessage(Message.InitMode.inited);
      Assert.AreEqual(false, producer.take(msg));
    }

    [Test]
    public void TestProducerPoke()
    {
      // insert test data
      var consumer = _scope.newConsumer(ps, "consumer", svc_durable);
      var msg = Builder.newMessage(Message.InitMode.inited);
      consumer.send(msg);

      // extend test data using poke()
      var extendDoc = Builder.newBson();
      extendDoc.append("test", 1);

      Assert.AreEqual(true, producer_durable.poke(msg._id, extendDoc));
    }

    [Test]
    public void TestProducerRemoveFromDurable()
    {
      // insert test data
      var consumer = _scope.newConsumer(ps, "consumer", svc_durable);
      var msg = Builder.newMessage(Message.InitMode.inited);
      consumer.send(msg);
      var msg2 = producer_durable.peek(msg._id);
      Assert.True(msg2 != null);
      producer_durable.remove(msg._id);
      try
      {
        producer_durable.peek(msg._id);
      }
      catch (PeekObjectNotFoundException){ 
        // Message properly removed from collection
      }
    }

    [Test]
    public void TestProducerPoke_Fail()
    {
      var producer = newProducer();

      var extendDoc = Builder.newBson();
      extendDoc.append("test", 1);

      Assert.AreEqual(false, producer.poke(Builder.newOid(), extendDoc));
    }

    [Test]
    public void TestProducerWait_Fail()
    {
      var producer = newProducer();
      try
      {
        producer.RequestTimeout = 1000;
        producer.wait();
      }
      catch (ProducerException e)
      {
        Assert.AreEqual(svc_bus_producer_err_t.SERVICE_BUS_PRODUCER_WAIT_EXCEEDED, e.ErrorCode);
        Assert.AreEqual("wait [2 : Max wait time exceeded]", e.Message);
        Assert.AreEqual("Max wait time exceeded", e.SourceMessage);
      }
    }

    [Test]
    public void TestProducerWaitAndTake_Fail()
    {
      var producer = newProducer();
      try
      {
        producer.RequestTimeout = 1000;
        producer.waitAndTake();
      }
      catch (ProducerException e)
      {
        Assert.AreEqual(svc_bus_producer_err_t.SERVICE_BUS_PRODUCER_WAIT_EXCEEDED, e.ErrorCode);
        Assert.AreEqual("waitAndTake [2 : Max wait time exceeded]", e.Message);
        Assert.AreEqual("Max wait time exceeded", e.SourceMessage);
      }
    }
  }
}
