using System;
using Sovos.SvcBus;
using NUnit.Framework;

namespace SvcBusTests
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture, GTestStyleConsoleOutputAttribute]
  class SvcBusMessageTests
  {
    private Scope _scope;
    private Message msg;
    private PipeService ps;

    [SetUp]
    public void SetUp()
    {
      _scope = new Scope();
      var id = Builder.newOid();
      ps = _scope.newPipeService(Constants.ConnectionString);
      id.Gen();
      msg = Builder.newMessage(id);
    }

    [TearDown]
    public void TearDown()
    {
      _scope.Dispose();
      msg = null;
      ps = null;
    }

    [Test]
    public void CreateMessage()
    {
      Assert.AreNotEqual(null, msg);
    }

    [Test]
    public void CheckReturns_id()
    {
      Assert.AreNotEqual(IntPtr.Zero, msg._id);
    }

    [Test]
    public void CheckReturns_messageId()
    {
      Assert.AreNotEqual(IntPtr.Zero, msg.messageId);
    }

    [Test]
    public void CheckReturns_responsePipeName()
    {
      Assert.AreEqual(string.Empty, msg.responsePipeName);
    }

    [Test]
    public void CheckReturns_responsePipeNameFromScopedMessage()
    {
      var msg2 = Builder.newMessage(Message.InitMode.inited);
      Assert.AreEqual(string.Empty, msg2.responsePipeName);
    }

    [Test]
    public void CheckBsonObject()
    {
      var b = msg.bson;
      var it = b.find("_id");
      Assert.AreNotEqual(bson_type.BSON_EOO, it.bsonType);
      var o = (Oid) it;
      Assert.AreNotEqual(null, o);
      Assert.AreNotEqual(0, o.Int(0));
      Assert.AreNotEqual(0, o.Int(1));
    }

    [Test]
    public void TestAccessCommand()
    {
      // Default value of command is blank string
      Assert.AreEqual("", msg.command); 
    }

    [Test]
    public void TestAccessBroadcast()
    {
      // Default value of command is blank string
      Assert.AreEqual(false, msg.Broadcast);
    }
    
    [Test]
    public void CopyConstructor()
    {
      msg.bson.append("test", 67890);
      Assert.AreEqual(67890, (int)msg.bson.find("test"));
      var msg1 = Builder.newMessage(msg);
      Assert.AreEqual(msg._id, msg1._id);
      Assert.AreEqual(msg.messageId, msg1.messageId);
      Assert.AreEqual(67890, (int)msg1.bson.find("test"));
    }

    [Test]
    public void ConstructorWithMessage()
    {
      var id = Builder.newOid();
      var msg1 = Builder.newMessage( id);
      msg1.bson.append("test", 12345);
      Assert.AreEqual(12345, (int)msg1.bson.find("test"));
      Assert.AreNotEqual(msg._id, msg1._id);
      Assert.AreNotEqual(msg.messageId, msg1.messageId);

      msg.bson.append("test", 67890);
      Assert.AreEqual(67890, (int)msg.bson.find("test"));

      msg1 = Builder.newMessage(msg);
      Assert.AreEqual(msg._id, msg1._id);
      Assert.AreEqual(msg.messageId, msg1.messageId);
      Assert.AreEqual(67890, (int)msg1.bson.find("test"));
    }
  }
}
