using Sovos.SvcBus;
using NUnit.Framework;

namespace SvcBusTests
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture, GTestStyleConsoleOutputAttribute]
  class SvcBusProducerAndConsumerTests
  {
    private Scope _scope;
    private Service svc_volatile;
    private PipeService ps;

    [SetUp]
    public void SetUp()
    {
      _scope = new Scope();
      svc_volatile = Builder.newService("test_service_volatile_4", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE, 1024, 1024);
      ps = _scope.newPipeService(Constants.ConnectionString);
    }

    [TearDown]
    public void TearDown()
    {
      ps.dropPipe(svc_volatile.Name);
      _scope.Dispose();
      ps = null;
      svc_volatile = null;
    }

    [Test]
    public void TestProducerAndConsumer()
    {
      /* Let's create producer first, we must make sure request collection is capped */
      var producer = _scope.newProducer(ps, "producer", svc_volatile);
      var consumer = _scope.newConsumer(ps, "consumer", svc_volatile, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
      /* Let's create and send an empty message*/
      var msg = Builder.newMessage(Message.InitMode.inited);
      consumer.send(msg);
      var msg2 = producer.wait();
      /* Let's do some peeking to test our peek() api */
      producer.peek(msg._id);
      try
      {
        producer.peek(msg.messageId);
        Assert.Fail("should have throw PeekObjectNotFoundException exception");
      }
      catch (PeekObjectNotFoundException)
      {
      }
      /* Let's take the message now */
      producer.take(msg2);
      /* Let's try waitAndTake() now */
      var msg5 = Builder.newMessage(Message.InitMode.inited);
      consumer.send(msg5);
      var msg6 = producer.waitAndTake();
      Assert.AreEqual(true, msg6.messageId == msg5.messageId);
      /* Let's create and send a response message with some data */
      var msg3 = Builder.newMessage(msg6.messageId);
      msg3.bson.append("data", 111);
      producer.responder.send(msg6.responsePipeName, msg3);
      /* consumer waits for message from producer */
      var msg4 = consumer.wait(msg5.messageId);
      /* We got a message, let's do some checking to make sure we got response we wanted */
      var it = msg4.find("data");
      Assert.AreEqual(111, (int)it);
    }
  }
}
