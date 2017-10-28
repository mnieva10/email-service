using System;
using Sovos.SvcBus;
using NUnit.Framework;

namespace SvcBusTests
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable"),
   System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture, GTestStyleConsoleOutputAttribute]
  class SvcBusBsonTests
  {
    private Bson b;
    private Scope _scope;
    private PipeService ps;

    [SetUp]
    public void SetUp()
    {
      _scope = new Scope();
      b = Builder.newBson();
      ps = _scope.newPipeService(Constants.ConnectionString);
    }

    [TearDown]
    public void TearDown()
    {
      _scope.Dispose();
      ps = null;
      b = null;
    }

    [Test]
    public void TestOidRandomGeneration()
    {
      var oid = Builder.newOid();
      oid.Gen();
      var oid2 = Builder.newOid();
      oid2.Gen();
      Assert.AreNotEqual(oid, oid2);
    }

    [Test]
    public void TestAppendInt()
    {
      b.append("test", 11);
      var it = b.find("test");
      Assert.AreEqual(bson_type.BSON_INT, it.bsonType);
      Assert.AreEqual(11, (int) it);
    }
    
    [Test]
    public void TestAppendString()
    {
      b.append("test", "str");
      var it = b.find("test");
      Assert.AreEqual(bson_type.BSON_STRING, it.bsonType);
      Assert.AreEqual("str", (string) it);
    }

    [Test]
    public void TestAppendStringUnicodeRussianChar()
    {
      b.append("test", "strκ");
      var it = b.find("test");
      Assert.AreEqual(bson_type.BSON_STRING, it.bsonType);
      Assert.AreEqual("strκ", (string) it);
    }
    
    [Test]
    public void TestFind_Fails()
    {
      var it = b.find("test");
      Assert.IsNull(it);
    }
    
    [Test]
    public void TestOidEquality()
    {
      var oid1 = Builder.newOid();
      oid1.Gen();
      var oid2 = Builder.newOid();
      oid2.Gen();
      // ReSharper disable once EqualExpressionComparison
      Assert.IsTrue(oid1.Equals(oid1));
      // ReSharper disable once EqualExpressionComparison
      Assert.IsFalse(oid1 == oid2);
      Assert.IsFalse(oid1.Equals(oid2));
      Assert.IsTrue(oid1 != oid2);
      // ReSharper disable once EqualExpressionComparison
    }
    
    [Test]
    public void TestCopyBson()
    {
      b.append("data", 111);
      var bb = (Bson)b.Clone();
      var it = bb.find("data");
      Assert.AreEqual(bson_type.BSON_INT, it.bsonType);
      Assert.AreEqual(111, (int)it);
    }

    [Test]
    public void TestBsonIteratorNext()
    {
      b.append("data", 111);
      var it = Builder.newIterator(b);
      Assert.True(it.next());
      Assert.False(it.next());
    }

    [Test]
    public void TestBsonIteratorKey()
    {
      b.append("data", 111);
      var it = Builder.newIterator(b);
      it.next();
      Assert.AreEqual("data", it.key);
    }
    
    [Test]
    public void TestBsonIteratorDouble()
    {
      b.append("data", 111.111);
      var it = Builder.newIterator(b);
      Assert.True(it.next());
      Assert.AreEqual(111.111, (double)it);
    }

    [Test]
    public void TestBsonIteratorInt64()
    {
      b.append("data", (Int64)111);
      var it = Builder.newIterator(b);
      Assert.True(it.next());
      Assert.AreEqual(111, (Int64)it);
    }

    [Test]
    public void TestBsonIteratorBool()
    {
      b.append("data", true);
      var it = Builder.newIterator(b);
      Assert.True(it.next());
      Assert.AreEqual(true, (bool)it);
    }

    [Test]
    public void TestBsonIteratorDateTime()
    {
      var d = new DateTime(2010, 12, 31, 10, 11, 32, 874);
      b.append("data", d);
      var it = Builder.newIterator(b);
      Assert.True(it.next());
      Assert.AreEqual(d, (DateTime)it);
    }
    
    [Test]
    public void TestBsonIteratorBinaryData()
    {
      var d = new Byte[255];
      for (var i = 0; i < d.Length; i++)
      {
        d[i] = (byte) i;
      }
      b.append("data", bson_binary_subtype_t.BSON_BIN_BINARY, d);
      var it = Builder.newIterator(b);
      it.next();
      var dd = (byte[])it;
      Assert.AreEqual(d.Length, dd.Length);
      for (var i = 0; i < dd.Length; i++)
      {
        Assert.AreEqual(d[i], dd[i]);
      }
    }

    [Test]
    public void TestBsonIteratorSubIteratorFromSubObject()
    {
      b.append("data", 111);
      Bson sub = b.appendDocumentBegin("sub");
      sub.append("data_sub", 222);
      b.appendDocumentEnd(sub);
      var it = Builder.newIterator(b);
      Assert.True(it.next());
      Assert.AreEqual(111, (int)it);
      Assert.True(it.next());
      var subit = new _Iterator(it);
      Assert.True(subit.next());
      Assert.AreEqual(222, (int)subit);
      Assert.False(subit.next());
      Assert.False(it.next());
    }

    [Test]
    public void TestBsonIteratorSubIteratorFromArray()
    {
      b.append("data", 111);
      Bson sub = b.appendArrayBegin("arr");
      sub.append("1", 222);
      sub.append("2", 333);
      sub.append("3", "hello");
      b.appendArrayEnd(sub);
      var it = Builder.newIterator(b);
      Assert.True(it.next());
      Assert.AreEqual(111, (int)it);
      Assert.True(it.next());
      var subit = new _Iterator(it);
      Assert.True(subit.next());
      Assert.AreEqual(222, (int)subit);
      Assert.True(subit.next());
      Assert.AreEqual(333, (int)subit);
      Assert.True(subit.next());
      Assert.AreEqual("hello", (string)subit);
      Assert.False(subit.next());
      Assert.False(it.next());
    }

    [Test]
    public void TestBsonIteratorNull()
    {
      b.appendNull("data");
      var it = Builder.newIterator(b);
      Assert.True(it.next());
    }

    [Test]
    public void TestBsonIteratorMaxKeyAndMinKey()
    {
      b.appendMaxKey("data_1");
      b.appendMinKey("data_2");
      var it = Builder.newIterator(b);
      Assert.True(it.next());
      Assert.True(it.next());
    }

    [Test]
    public void TestBsonIteratorOidAppended()
    {
      var oid = Builder.newOid();
      b.append("data", oid);
      var it = Builder.newIterator(b);
      Assert.True(it.next());
    }

    /*
     * We don't want to run the following test "normally". Enable it only to check the behavior of the Scope class Exception
     * accumulation and serialization behavior. Otherwise, keep it commented out. It may corrupt the C heap
     * 
    [Test]
    public void TestDisposeBadBsonThruScope()
    {
      try
      {
        using (var aScope = new Scope())
        {
          b.append("data", "hello");
          var bb = aScope.add(Builder.newBson(b));
          NativeMethods.bson_destroy(bb.Handle); // This will generate Access Violation when scope is destroyed
        }
      }
      catch (EScope exception)
      {
        Assert.AreEqual("Error disposing _Bson [Attempted to read or write protected memory. This is often an indication that other memory is corrupt.]\r\n", exception.Message);
      }
    }
    */
  }
}
