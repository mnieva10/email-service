using Sovos.SvcBus;
using NUnit.Framework;

namespace SvcBusTests
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture, GTestStyleConsoleOutputAttribute]
  class SvcBusResponderTests
  {
    private Scope _scope;
    private PipeService ps;

    [SetUp]
    public void SetUp()
    {
      _scope = new Scope();
      ps = _scope.newPipeService(Constants.ConnectionString);
    }

    [TearDown]
    public void TearDown()
    {
      _scope.Dispose();
      ps = null;
    }

    private Responder newResponder()
    {
      return Builder.newResponder(ps);
    }

    [Test]
    public void TestCreateResponder()
    {
      var responder = newResponder();
      Assert.AreNotEqual(null, responder);
    }

    [Test]
    public void TestCreateResponderCopy()
    {
      var srcResponder = newResponder();
      var responder = Builder.newResponder(srcResponder.Handle);
      Assert.AreNotEqual(null, responder);
    }

    [Test]
    public void TestResponderSend()
    {
      var responder = newResponder();
      Assert.AreNotEqual(null, responder);
      var message = Builder.newMessage(Message.InitMode.inited);
      responder.send("fake_collection_pipe", message);
    }
  }
}
