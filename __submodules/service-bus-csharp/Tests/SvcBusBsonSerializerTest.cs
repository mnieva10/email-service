using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Sovos.SvcBus;

namespace SvcBusTests
{
  [TestFixture, GTestStyleConsoleOutputAttribute]
  public class BsonSerializerTest
  {
    #region Test Classes
    class PrimitivesObject
    {
      [SvcBusSerializable]
      public string StringProp { get; set; }
      [SvcBusSerializable]
      public int IntProp { get; set; }
      [SvcBusSerializable]
      public double DoubleProp { get; set; }
      [SvcBusSerializable]
      public DateTime DateTimeProp { get; set; }
      [SvcBusSerializable]
      public bool BoolProp { get; set; }
      [SvcBusSerializable]
      public long LongProp { get; set; }
      [SvcBusSerializable]
      // ReSharper disable once UnusedMember.Local
      public string NullProperty { get { return null; } }

      // ReSharper disable once UnusedAutoPropertyAccessor.Local
      public string NonSerializable { get; set; }
    }

    private class ListObject
    {
      [SvcBusSerializable]
      public List<string> StringList { get; set; }

      [SvcBusSerializable]
      public List<object> ObjectList { get; set; }

      [SvcBusSerializable]
      public List<int> IntArray { get; set; }

      [SvcBusSerializable]
      public List<object> BaseTypeArray { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public List<List<string>> StringList2d { get; set; }

      public ListObject()
      {
        StringList = new List<string>();
        ObjectList = new List<object>();
        IntArray = new List<int>();
        BaseTypeArray = new List<object>();
        StringList2d = new List<List<string>>();
      }
    }

    [Flags]
    enum Enumeration { First = 1, Second = 2 };

    // ReSharper disable once UnusedMember.Local
    enum Enumeration2 { First = 1, Second = 2 };

    class SubObjectInt
    {
      [SvcBusSerializable]
      public int TheInt { get; set; }
    }

    class TestObject
    {
      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public int The_00_Int { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public long The_01_Int64 { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public char The_02_AnsiChar { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public Enumeration2 The_03_Enumeration { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public double The_04_Float { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public string The_05_String { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public string The_06_ShortString { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public Enumeration The_07_Set { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      // ReSharper disable once MemberCanBePrivate.Local
      public SubObjectInt The_08_SubObject { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public char The_10_WChar { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public string The_11_AnsiString { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public string The_12_WideString { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      // ReSharper disable once MemberCanBePrivate.Local
      public List<string> The_13_StringList { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public int The_14_VariantAsInteger { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public string The_15_VariantAsString { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public List<int> The_16_VariantAsList { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public List<List<int>> The_17_VariantTwoNestedList { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public List<int> The_18_VariantAsListEmpty { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public bool The_19_Boolean { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public DateTime The_20_DateTime { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public MemoryStream The_21_MemStream { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public MemoryStream The_22_BlankMemStream { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      public Enumeration The_23_EmptySet { get; set; }

      public TestObject()
      {
        The_08_SubObject = new SubObjectInt();
        The_13_StringList = new List<string>();
        The_16_VariantAsList = new List<int>(2);
        The_17_VariantTwoNestedList = new List<List<int>>();
        The_18_VariantAsListEmpty = new List<int>();
        The_21_MemStream = new MemoryStream();
        The_22_BlankMemStream = new MemoryStream();
      }
    }

    class SubTestObjectWithSomeList
    {
      [SvcBusSerializable]
      // ReSharper disable once UnusedAutoPropertyAccessor.Local
      public List<string> StringList { get; set; }

      public SubTestObjectWithSomeList()
      {
        StringList = new List<string>();
      }
    }

    class TestObjectWithSomeList
    {
      [SvcBusSerializable]
      // ReSharper disable once MemberCanBePrivate.Local
      // ReSharper disable once UnusedAutoPropertyAccessor.Local
      public List<string> StringList { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once UnusedAutoPropertyAccessor.Local
      public List<object> ObjectList { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once MemberCanBePrivate.Local
      // ReSharper disable once UnusedAutoPropertyAccessor.Local
      public List<int> IntArray { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once InconsistentNaming
      // ReSharper disable once UnusedAutoPropertyAccessor.Local
      public List<List<string>> StringList2d { get; set; }

      [SvcBusSerializable]
      // ReSharper disable once UnusedAutoPropertyAccessor.Local
      public SubTestObjectWithSomeList SubObject { get; set; }

      public TestObjectWithSomeList()
      {
        StringList = new List<string>();
        ObjectList = new List<object>();
        IntArray = new List<int>();
        StringList2d = new List<List<string>>();
        SubObject = new SubTestObjectWithSomeList();
      }
    }

    class TestStream : Stream
    {
      public override bool CanRead
      {
        get { throw new NotImplementedException(); }
      }

      public override bool CanSeek
      {
        get { throw new NotImplementedException(); }
      }

      public override bool CanWrite
      {
        get { throw new NotImplementedException(); }
      }

      public override void Flush()
      {
        throw new NotImplementedException();
      }

      public override long Length
      {
        get { throw new NotImplementedException(); }
      }

      public override long Position
      {
        get
        {
          throw new NotImplementedException();
        }
        set
        {
          throw new NotImplementedException();
        }
      }

      public override int Read(byte[] buffer, int offset, int count)
      {
        throw new NotImplementedException();
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
        throw new NotImplementedException();
      }

      public override void SetLength(long value)
      {
        throw new NotImplementedException();
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
        throw new NotImplementedException();
      }
    }

    class DictionaryObject
    {
      [SvcBusSerializable]
      public Dictionary<string, int> SimpleMap { get; set; }

      [SvcBusSerializable]
      public Dictionary<string, PrimitivesObject> PrimitivesObjectMap { get; set; }

      [SvcBusSerializable]
      public Dictionary<PrimitivesObject, BaseDoc> PolyObjectMap { get; set; }

      [SvcBusSerializable]
      public Dictionary<string, List<int>> ListMap { get; set; }
    }

    class DumyStreamSerializer : BaseBsonSerializer
    {
      public override void Serialize(string name, string typeName = "")
      {
        throw new NotImplementedException();
      }
    }

    class MacroParsingServiceRequestScript
    {
      [SvcBusSerializable]
      public string Script { get; set; }
      [SvcBusSerializable]
      public Dictionary<string, string> Vars { get; set; }
      [SvcBusSerializable]
      public Dictionary<string, int> Defs { get; set; }
    }

    class MacroParsingServiceRequest
    {
      [SvcBusSerializable]
      public string Schema { get; set; }
      [SvcBusSerializable]
      public string UserName { get; set; }
      [SvcBusSerializable]
      public List<MacroParsingServiceRequestScript> Scripts { get; set; }
    }
    #endregion

    #region Polymorphic Test Classes
    public abstract class BaseDoc
    {
      private string _prop;
      [SvcBusSerializableAttribute]
      public string ClassName { get { return _prop ?? GetType().Name; } set { _prop = value; } }
    }

    public class SimpleDoc : BaseDoc
    {
      [SvcBusSerializableAttribute]
      public List<BaseDoc> Forms { get; set; }

      [SvcBusSerializableAttribute]
      public int RequiredFormNumber { get; set; }
    }

    public class VariantDoc : BaseDoc
    {
      [SvcBusSerializableAttribute]
      public List<object> VariantForms { get; set; }

      [SvcBusSerializableAttribute]
      public object VariantForm { get; set; }

      [SvcBusSerializableAttribute]
      public object VariantPrimitive { get; set; }

      public VariantDoc() { }

      public VariantDoc(string name)
      {
        VariantForm = name;
      }
    }

    public class ReviewForm : BaseDoc
    {
      [SvcBusSerializableAttribute]
      // ReSharper disable once InconsistentNaming
      public DateTime ReviewDT { get; set; }

      [SvcBusSerializableAttribute]
      public List<int> MarkedPages { get; set; }
    }

    public class FormA : SimpleDoc
    {
      [SvcBusSerializableAttribute]
      public BaseDoc MainForm { get; set; }

      [SvcBusSerializableAttribute]
      public BaseDoc OptionalForm { get; set; }

      [SvcBusSerializableAttribute]
      public bool IsCompleted { get; set; }
    }

    public class FormAExtended : FormA
    {
      [SvcBusSerializableAttribute]
      public Double Amount { get; set; }
    }

    #endregion

    private BaseBsonSerializer _bsonSerializer;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      var map = BuildableSerializableTypeMapper.Mapper;
      map.Register("PrimitivesObject", typeof(PrimitivesObject));
      map.Register("ListObject", typeof(ListObject));
      map.Register("SubObjectInt", typeof(SubObjectInt));
      map.Register("TestObject", typeof(TestObject));
      map.Register("SubTestObjectWithSomeList", typeof(SubTestObjectWithSomeList));
      map.Register("TestObjectWithSomeList", typeof(TestObjectWithSomeList));
      map.Register("SimpleDoc", typeof(SimpleDoc));
      map.Register("VariantDoc", typeof(VariantDoc));
      map.Register("ReviewForm", typeof(ReviewForm));
      map.Register("FormA", typeof(FormA));
      map.Register("FormAExtended", typeof(FormAExtended));
      map.Register("DictionaryObject", typeof(DictionaryObject));
      map.Register("MacroParsingServiceRequest", typeof(MacroParsingServiceRequest));
    }

    [SetUp]
    public void SetUp()
    {
#pragma warning disable 0618
      BaseBsonSerializer.Clear();
#pragma warning restore 0618

      BaseBsonSerializer.RegisterSerializer(typeof(object), typeof(DefaultBsonSerializer));
      BaseBsonSerializer.RegisterSerializer(typeof(Stream), typeof(StreamBsonSerializer));
      BaseBsonSerializer.RegisterSerializer(typeof(ExpandoObject), typeof(ExpandoObjectBsonSerializer));

      _bsonSerializer = Builder.newBsonSerializer(typeof(object));
    }

    [TearDown]
    public void TearDown() { }

    #region Polymorphic Tests

    [Test]
    public void PolymorphicObject()
    {
      var reviewForm = new ReviewForm { ReviewDT = DateTime.Now, MarkedPages = new List<int> { 1, 5, 9 } };

      var b = Builder.newBson();
      _bsonSerializer.Target = b;
      _bsonSerializer.Source = reviewForm;
      _bsonSerializer.Serialize("");

      var it = Builder.newIterator(b);
      it.next();
      Assert.AreEqual(DispatcherConstants.ActualType, it.key);
      Assert.AreEqual(bson_type.BSON_STRING, it.bsonType);
      Assert.AreEqual("ReviewForm", (string)it);

      it.next();
      Assert.AreEqual(bson_type.BSON_DATE, it.bsonType);
      Assert.AreEqual("ReviewDT", it.key);
      Assert.AreEqual(reviewForm.ReviewDT.ToString("D"), ((DateTime)it).ToString("D"));


      //Same derived type target
      it = Builder.newIterator(b);
      object newObj = new ReviewForm();
      new BsonDeserializer(it).Deserialize(ref newObj);

      var form = (ReviewForm)newObj;
      Assert.AreEqual(reviewForm.ReviewDT.ToString("D"), form.ReviewDT.ToString("D"));
      Assert.AreEqual(3, form.MarkedPages.Count);

      //Other compatible derived type target
      it = Builder.newIterator(b);
      newObj = new SimpleDoc();
      new BsonDeserializer(it).Deserialize(ref newObj);

      Assert.AreEqual("SimpleDoc", newObj.GetType().Name);

      //Null target
      it = Builder.newIterator(b);
      newObj = null;
      new BsonDeserializer(it).Deserialize(ref newObj);

      Assert.AreEqual("ReviewForm", form.GetType().Name);
      form = (ReviewForm)newObj;
      Assert.AreEqual(reviewForm.ReviewDT.ToString("D"), form.ReviewDT.ToString("D"));
      Assert.AreEqual(3, form.MarkedPages.Count);
    }

    [Test]
    public void PolymorphicProperty()
    {
      var reviewForm = new ReviewForm { ReviewDT = DateTime.Now, MarkedPages = new List<int> { 1, 5, 9 } };
      var simpleDoc = new SimpleDoc { RequiredFormNumber = 1, Forms = new List<BaseDoc> { reviewForm, new ReviewForm { ReviewDT = DateTime.Now.Subtract(new TimeSpan(10, 0, 0, 0)) } } };
      var simpleDocList = new List<BaseDoc> { simpleDoc, new SimpleDoc { RequiredFormNumber = 2 } };
      var formA = new FormA { IsCompleted = false, MainForm = reviewForm, OptionalForm = simpleDoc, Forms = simpleDocList };

      var b = Builder.newBson();
      _bsonSerializer.Source = formA;
      _bsonSerializer.Target = b;
      _bsonSerializer.Serialize("");

      var it = Builder.newIterator(b);
      object newObj = null;
      new BsonDeserializer(it).Deserialize(ref newObj);

      Assert.AreEqual("FormA", newObj.GetType().Name);
      var form = (FormA)newObj;
      Assert.AreEqual("ReviewForm", form.MainForm.GetType().Name);
      Assert.AreEqual("SimpleDoc", form.OptionalForm.GetType().Name);
      Assert.AreEqual(reviewForm.ReviewDT.ToString("D"), ((ReviewForm)form.MainForm).ReviewDT.ToString("D"));
    }

    [Test]
    public void PolymorphicObjectWithPolyList()
    {
      var reviewForm = new ReviewForm { ReviewDT = DateTime.Now, MarkedPages = new List<int> { 1, 5, 9 } };
      var simpleDoc = new SimpleDoc { RequiredFormNumber = 1, Forms = new List<BaseDoc> { reviewForm, new ReviewForm { ReviewDT = DateTime.Now.Subtract(new TimeSpan(10, 0, 0, 0)) } } };
      var formA = new FormA { IsCompleted = false, MainForm = reviewForm, OptionalForm = simpleDoc };
      var docList = new List<BaseDoc> { reviewForm, simpleDoc, formA };
      var formAExtended = new FormAExtended { Amount = 123.45, IsCompleted = true, MainForm = formA, Forms = docList };

      var b = Builder.newBson();
      _bsonSerializer.Target = b;
      _bsonSerializer.Source = formAExtended;
      _bsonSerializer.Serialize("");

      var it = Builder.newIterator(b);
      object newObj = null;
      var deserlzr = new BsonDeserializer(it);
      deserlzr.Deserialize(ref newObj);

      Assert.AreEqual("FormAExtended", newObj.GetType().Name);
      var form = (FormAExtended)newObj;
      Assert.AreEqual(3, form.Forms.Count);
      Assert.AreEqual("ReviewForm", form.Forms[0].GetType().Name);
      Assert.AreEqual(3, ((ReviewForm)form.Forms[0]).MarkedPages.Count);
      Assert.AreEqual("SimpleDoc", form.Forms[1].GetType().Name);
      Assert.AreEqual(1, ((SimpleDoc)form.Forms[1]).RequiredFormNumber);
      Assert.AreEqual(2, ((SimpleDoc)form.Forms[1]).Forms.Count);
      Assert.AreEqual("FormA", form.Forms[2].GetType().Name);
      Assert.AreEqual("SimpleDoc", ((FormA)form.Forms[2]).OptionalForm.GetType().Name);
    }

    [Test]
    public void VariantPropertyAndArray()
    {
      var reviewForm = new ReviewForm { ReviewDT = DateTime.Now, MarkedPages = new List<int> { 1, 5, 9 } };
      var simpleDoc = new SimpleDoc { RequiredFormNumber = 1, Forms = new List<BaseDoc> { reviewForm, new ReviewForm { ReviewDT = DateTime.Now.Subtract(new TimeSpan(10, 0, 0, 0)) } } };
      var formA = new FormA { IsCompleted = false, MainForm = reviewForm, OptionalForm = simpleDoc };

      var variantDoc = new VariantDoc
      {
        VariantForm = simpleDoc,
        VariantPrimitive = 123.45,
        VariantForms = new List<object> { 123, "test", reviewForm, simpleDoc, formA }
      };

      var b = Builder.newBson();
      _bsonSerializer.Target = b;
      _bsonSerializer.Source = variantDoc;
      _bsonSerializer.Serialize("");

      var it = Builder.newIterator(b);
      object newObj = null;
      var deserlzr = new BsonDeserializer(it);
      deserlzr.Deserialize(ref newObj);

      Assert.AreEqual("VariantDoc", newObj.GetType().Name);
      var form = (VariantDoc)newObj;
      Assert.AreEqual(5, form.VariantForms.Count);
      Assert.AreEqual("SimpleDoc", form.VariantForm.GetType().Name);
      Assert.AreEqual(1, ((SimpleDoc)form.VariantForm).RequiredFormNumber);
      Assert.AreEqual(2, ((SimpleDoc)form.VariantForm).Forms.Count);
      Assert.AreEqual(123.45, form.VariantPrimitive);
      Assert.AreEqual(123, form.VariantForms[0]);
      Assert.AreEqual("test", form.VariantForms[1]);
      Assert.AreEqual("ReviewForm", form.VariantForms[2].GetType().Name);
      Assert.AreEqual(3, ((ReviewForm)form.VariantForms[2]).MarkedPages.Count);
      Assert.AreEqual("SimpleDoc", form.VariantForms[3].GetType().Name);
      Assert.AreEqual(1, ((SimpleDoc)form.VariantForms[3]).RequiredFormNumber);
      Assert.AreEqual(2, ((SimpleDoc)form.VariantForms[3]).Forms.Count);
      Assert.AreEqual("FormA", form.VariantForms[4].GetType().Name);
      Assert.AreEqual("SimpleDoc", ((FormA)form.VariantForms[4]).OptionalForm.GetType().Name);
    }

    [Test]
    public void DeserilaizeWithContext()
    {
      var reviewForm = new ReviewForm { ReviewDT = DateTime.Now, MarkedPages = new List<int> { 1, 5, 9 } };
      var simpleDoc = new SimpleDoc { RequiredFormNumber = 1, Forms = new List<BaseDoc> { reviewForm, new ReviewForm { ReviewDT = DateTime.Now.Subtract(new TimeSpan(10, 0, 0, 0)) } } };
      var formA = new FormA { IsCompleted = false, MainForm = reviewForm, OptionalForm = simpleDoc };

      var variantDoc = new VariantDoc()
      {
        VariantForm = simpleDoc,
        VariantPrimitive = 123.45,
        VariantForms = new List<object> { 123, "test", reviewForm, simpleDoc, formA }
      };

      var b = Builder.newBson();
      _bsonSerializer.Source = variantDoc;
      _bsonSerializer.Target = b;
      _bsonSerializer.Serialize("");

      var it = Builder.newIterator(b);
      object newObj = null;
      var deserlzr = new BsonDeserializer(it);
      var context = "test name";
      deserlzr.Deserialize(ref newObj, context);

      Assert.AreEqual("VariantDoc", newObj.GetType().Name);
      var form = (VariantDoc)newObj;
      Assert.AreEqual(5, form.VariantForms.Count);
      Assert.AreEqual("String", form.VariantForm.GetType().Name);
      Assert.AreEqual(context, form.VariantForm);

      Assert.AreEqual(123.45, form.VariantPrimitive);
      Assert.AreEqual(123, form.VariantForms[0]);
      Assert.AreEqual("test", form.VariantForms[1]);
      Assert.AreEqual("ReviewForm", form.VariantForms[2].GetType().Name);
      Assert.AreEqual(3, ((ReviewForm)form.VariantForms[2]).MarkedPages.Count);
      Assert.AreEqual("SimpleDoc", form.VariantForms[3].GetType().Name);
      Assert.AreEqual(1, ((SimpleDoc)form.VariantForms[3]).RequiredFormNumber);
      Assert.AreEqual(2, ((SimpleDoc)form.VariantForms[3]).Forms.Count);
      Assert.AreEqual("FormA", form.VariantForms[4].GetType().Name);
      Assert.AreEqual("SimpleDoc", ((FormA)form.VariantForms[4]).OptionalForm.GetType().Name);
    }

    #endregion

    #region Dictionary Tests

    private static void ItCheckInt(ref Iterator it, string key, int value)
    {
      it.next();
      Assert.AreEqual(key, it.key);
      Assert.AreEqual(bson_type.BSON_INT, it.bsonType);
      Assert.AreEqual(value, (int)it);
    }

    private static void KeyValueSubobjectCheckInt(ref Iterator it, string key, int value)
    {
      Assert.IsTrue(it.next());
      Assert.AreEqual(bson_type.BSON_OBJECT, it.bsonType);
      Assert.AreEqual(DispatcherConstants.CollectionKey + DispatcherConstants.CollectionValue, it.key);

      var keyValueIt = Builder.newIterator(it);
      Assert.IsTrue(keyValueIt.next());
      Assert.AreEqual(bson_type.BSON_OBJECT, keyValueIt.bsonType);
      Assert.AreEqual(DispatcherConstants.CollectionKey, keyValueIt.key);

      var keyIt = Builder.newIterator(keyValueIt);
      Assert.IsTrue(keyIt.next());
      Assert.AreEqual(bson_type.BSON_STRING, keyIt.bsonType);
      Assert.AreEqual(key, (string)keyIt);

      Assert.IsTrue(keyValueIt.next());
      Assert.AreEqual(bson_type.BSON_OBJECT, keyValueIt.bsonType);

      var valueIt = Builder.newIterator(keyValueIt);
      Assert.IsTrue(valueIt.next());
      Assert.AreEqual(bson_type.BSON_INT, valueIt.bsonType);
      Assert.AreEqual(value, (int)valueIt);
    }

    [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
    [TestCase(DictionarySerializationMode.ForceComplex)]
    [TestCase(DictionarySerializationMode.Simple)]
    public void SimpleDictionary(DictionarySerializationMode mode)
    {
      BuildableSerializableTypeMapper.dictionarySerializationMode = mode;
      var obj = new DictionaryObject { SimpleMap = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } } };

      var b = Builder.newBson();
      _bsonSerializer.Target = b;
      _bsonSerializer.Source = obj;
      _bsonSerializer.Serialize("");

      var it = Builder.newIterator(b);

      it.next();
      Assert.AreEqual(DispatcherConstants.ActualType, it.key);
      Assert.AreEqual(bson_type.BSON_STRING, it.bsonType);
      Assert.AreEqual("DictionaryObject", (string)it);

      it.next();
      Assert.AreEqual(mode == DictionarySerializationMode.Simple ? bson_type.BSON_OBJECT : bson_type.BSON_ARRAY, it.bsonType);
      Assert.AreEqual("SimpleMap", it.key);

      var subSubIt = Builder.newIterator(it);
      if (mode == DictionarySerializationMode.Simple)
      {
        ItCheckInt(ref subSubIt, "a", 1);
        ItCheckInt(ref subSubIt, "b", 2);
        ItCheckInt(ref subSubIt, "c", 3);
      }
      else
      {
        KeyValueSubobjectCheckInt(ref subSubIt, "a", 1);
        KeyValueSubobjectCheckInt(ref subSubIt, "b", 2);
        KeyValueSubobjectCheckInt(ref subSubIt, "c", 3);
      }

      it = Builder.newIterator(b);
      object newObj = new DictionaryObject();
      new BsonDeserializer(it).Deserialize(ref newObj);

      Assert.AreEqual(obj.SimpleMap, ((DictionaryObject)newObj).SimpleMap);
      // reset to default
      BuildableSerializableTypeMapper.dictionarySerializationMode = DictionarySerializationMode.Simple;
    }

    [Test]
    public void MacroParsingServiceRequestDefsVarsAsDictionary()
    {
      var obj = new MacroParsingServiceRequest
      {
        Schema = "CEENV149",
        UserName = "Admin",
        Scripts = new List<MacroParsingServiceRequestScript> {
                      new MacroParsingServiceRequestScript { Script = "1.xql" },
                      new MacroParsingServiceRequestScript { Script = "2.xql", 
                                                             Defs = new Dictionary<string, int> { { "d1", 1 }, { "d2", 0 } },
                                                             Vars = new Dictionary<string, string> { { "v1", "my value" }, { "v2", "my value 2" } }
                      }
                    }
      };

      var b = Builder.newBson();
      _bsonSerializer.Target = b;
      _bsonSerializer.Source = obj;

      _bsonSerializer.Serialize("");

      var it = Builder.newIterator(b);
      object newObj = new MacroParsingServiceRequest();
      new BsonDeserializer(it).Deserialize(ref newObj);

      Assert.AreEqual(obj.Schema, ((MacroParsingServiceRequest)newObj).Schema);
      Assert.AreEqual(obj.UserName, ((MacroParsingServiceRequest)newObj).UserName);
      Assert.AreEqual(obj.Scripts[0].Script, ((MacroParsingServiceRequest)newObj).Scripts[0].Script);
      Assert.AreEqual(obj.Scripts[0].Defs, ((MacroParsingServiceRequest)newObj).Scripts[0].Defs);
      Assert.AreEqual(obj.Scripts[0].Vars, ((MacroParsingServiceRequest)newObj).Scripts[0].Vars);
    }

    [Test, SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public void PrimitivesObjectDictionary()
    {
      var map = new DictionaryObject
      {
        PrimitivesObjectMap = new Dictionary<string, PrimitivesObject> { { "a", new PrimitivesObject { DoubleProp = 2.45, DateTimeProp = DateTime.Now } }, 
                                                                                                        { "b", new PrimitivesObject { IntProp = 245, BoolProp = true} } }
      };
      var b = Builder.newBson();
      _bsonSerializer.Target = b;
      _bsonSerializer.Source = map;

      _bsonSerializer.Serialize("");

      var it = Builder.newIterator(b);

      it.next();
      Assert.AreEqual(DispatcherConstants.ActualType, it.key);
      Assert.AreEqual(bson_type.BSON_STRING, it.bsonType);
      Assert.AreEqual("DictionaryObject", (string)it);

      it.next();
      Assert.AreEqual(bson_type.BSON_OBJECT, it.bsonType);
      Assert.AreEqual("PrimitivesObjectMap", it.key);

      var subIt = Builder.newIterator(it);
      subIt.next();
      Assert.AreEqual(bson_type.BSON_OBJECT, subIt.bsonType);
      Assert.AreEqual("a", subIt.key);

      subIt.next();
      Assert.AreEqual(bson_type.BSON_OBJECT, subIt.bsonType);
      Assert.AreEqual("b", subIt.key);

      var subSubIt = Builder.newIterator(subIt);
      subSubIt.next();
      Assert.AreEqual(bson_type.BSON_STRING, subSubIt.bsonType);
      Assert.AreEqual(DispatcherConstants.ActualType, subSubIt.key);
      subSubIt.next();
      Assert.AreEqual(bson_type.BSON_INT, subSubIt.bsonType);
      Assert.AreEqual(245, (int)subSubIt);

      it = Builder.newIterator(b);
      object newObj = new DictionaryObject();
      new BsonDeserializer(it).Deserialize(ref newObj);

      var map1 = (DictionaryObject)newObj;
      Assert.AreEqual(2, map1.PrimitivesObjectMap.Count);
      Assert.IsTrue(map1.PrimitivesObjectMap.ContainsKey("b"));
      Assert.AreEqual(245, map1.PrimitivesObjectMap["b"].IntProp);
    }

    [Test, SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public void PolyObjectDictionary()
    {
      var key1 = new PrimitivesObject { DoubleProp = 2.45, DateTimeProp = DateTime.Now };
      var value2 = new FormA { IsCompleted = false, RequiredFormNumber = 25 };
      var map = new DictionaryObject
      {
        PolyObjectMap = new Dictionary<PrimitivesObject, BaseDoc> { { key1, new ReviewForm { ReviewDT = DateTime.Now, MarkedPages = new List<int> { 1, 5, 9 } } },
          { new PrimitivesObject { IntProp = 245, BoolProp = true }, value2} }
      };

      var b = Builder.newBson();
      _bsonSerializer.Target = b;
      _bsonSerializer.Source = map;
      _bsonSerializer.Serialize("");

      var it = Builder.newIterator(b);
      object newObj = new DictionaryObject();
      new BsonDeserializer(it).Deserialize(ref newObj);

      var map1 = (DictionaryObject)newObj;
      Assert.AreEqual(2, map1.PolyObjectMap.Count);
      var keyArr = new PrimitivesObject[10];
      map1.PolyObjectMap.Keys.CopyTo(keyArr, 0);
      Assert.AreEqual(2.45, keyArr[0].DoubleProp);
      var valueArr = new BaseDoc[10];
      map1.PolyObjectMap.Values.CopyTo(valueArr, 0);
      Assert.AreEqual("FormA", valueArr[1].ClassName);
    }

    [Test, SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public void ListDictionary()
    {
      var map = new DictionaryObject
      {
        ListMap = new Dictionary<string, List<int>> { { "a", new List<int> { 1, 2, 3 } }, 
                                                                                     { "b", new List<int> { 10, 20, 30 } } }
      };

      var b = Builder.newBson();
      _bsonSerializer.Target = b;
      _bsonSerializer.Source = map;

      _bsonSerializer.Serialize("");

      var it = Builder.newIterator(b);
      object newObj = new DictionaryObject();
      new BsonDeserializer(it).Deserialize(ref newObj);

      var map1 = (DictionaryObject)newObj;
      Assert.AreEqual(map.ListMap, map1.ListMap);
    }

    #endregion

    [Test, SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    //Added suppress message because the code analyis was giving a false positive.
    public void SerializeListObject()
    {
      var obj = new ListObject
      {
        StringList = new List<string> { "string1", "string 2", "another string" },
        ObjectList = new List<object> { 10, false, new DateTime(2014, 3, 4) },
        IntArray = new List<int> { 1, 20, 300 },
        BaseTypeArray = new List<object> { "test", 25.34, true },
        StringList2d = new List<List<string>> { new List<string> { "string1", "string2" }, new List<string> { "string3", "string4" } }
      };

      var b = Builder.newBson();
      _bsonSerializer.Target = b;
      _bsonSerializer.Source = obj;
      _bsonSerializer.Serialize("");

      var subit = Builder.newIterator(b);
      {
        subit.next();
        Assert.AreEqual(DispatcherConstants.ActualType, subit.key);
        Assert.AreEqual(bson_type.BSON_STRING, subit.bsonType);
        Assert.AreEqual("ListObject", (string)subit);

        subit.next();
        Assert.AreEqual("StringList", subit.key);
        Assert.AreEqual(string.Empty, (string)subit);

        var subit2 = Builder.newIterator(subit);
        {
          subit2.next();
          Assert.AreEqual("string1", (string)subit2);
          subit2.next();
          Assert.AreEqual("string 2", (string)subit2);
          subit2.next();
          Assert.AreEqual("another string", (string)subit2);
          Assert.False(subit2.next());
        }

        subit.next();
        Assert.AreEqual("ObjectList", subit.key);
        Assert.AreEqual(string.Empty, (string)subit);

        subit2 = Builder.newIterator(subit);
        {
          subit2.next();
          Assert.AreEqual(10, (int)subit2);
          subit2.next();
          Assert.AreEqual(false, (bool)subit2);
          subit2.next();
          Assert.AreEqual(new DateTime(2014, 3, 4), (DateTime)subit2);
          Assert.False(subit2.next());
        }

        subit.next();
        Assert.AreEqual("IntArray", subit.key);
        Assert.AreEqual(string.Empty, (string)subit);

        subit2 = Builder.newIterator(subit);
        {
          subit2.next();
          Assert.AreEqual(1, (int)subit2);
          subit2.next();
          Assert.AreEqual(20, (int)subit2);
          subit2.next();
          Assert.AreEqual(300, (int)subit2);
          Assert.False(subit2.next());
        }

        subit.next();
        Assert.AreEqual("BaseTypeArray", subit.key);
        Assert.AreEqual(string.Empty, (string)subit);

        subit2 = Builder.newIterator(subit);
        {
          subit2.next();
          Assert.AreEqual("test", (string)subit2);
          subit2.next();
          Assert.AreEqual(25.34, (double)subit2);
          subit2.next();
          Assert.AreEqual(true, (bool)subit2);
          Assert.False(subit2.next());
        }

        subit.next();
        Assert.AreEqual("StringList2d", subit.key);
        Assert.AreEqual(string.Empty, (string)subit);
        Assert.AreEqual(bson_type.BSON_ARRAY, subit.bsonType);

        subit2 = Builder.newIterator(subit);
        {
          subit2.next();
          Assert.AreEqual(bson_type.BSON_ARRAY, subit2.bsonType);
          Assert.AreEqual(string.Empty, (string)subit2);

          var subit3 = Builder.newIterator(subit2);
          {
            subit3.next();
            Assert.AreEqual("string1", (string)subit3);
            subit3.next();
            Assert.AreEqual("string2", (string)subit3);
          }

          subit2.next();
          Assert.AreEqual(bson_type.BSON_ARRAY, subit2.bsonType);
          Assert.AreEqual(string.Empty, (string)subit2);

          subit3 = Builder.newIterator(subit2);
          {
            subit3.next();
            Assert.AreEqual("string3", (string)subit3);
            subit3.next();
            Assert.AreEqual("string4", (string)subit3);
          }
        }

        Assert.False(subit.next());
      }

      var it = Builder.newIterator(b);
      object newObj = new ListObject();
      new BsonDeserializer(it).Deserialize(ref newObj);

      var newObjAsListObject = (ListObject)newObj;
      Assert.AreEqual(obj.ObjectList, newObjAsListObject.ObjectList);
      Assert.AreEqual(obj.StringList, newObjAsListObject.StringList);
      Assert.AreEqual(obj.IntArray, newObjAsListObject.IntArray);
      Assert.AreEqual(obj.BaseTypeArray, newObjAsListObject.BaseTypeArray);
      Assert.AreEqual(obj.StringList2d, newObjAsListObject.StringList2d);
    }

    [Test]
    public void SerializePrimitivesObject()
    {
      var obj = new PrimitivesObject
      {
        BoolProp = true,
        DateTimeProp = new DateTime(2010, 2, 25),
        DoubleProp = 12.34,
        IntProp = 147,
        StringProp = "I am PrimitivesObject",
        LongProp = 4294967296L,
        NonSerializable = "NonSerializable property"
      };
      var b = Builder.newBson();
      _bsonSerializer.Target = b;
      _bsonSerializer.Source = obj;
      _bsonSerializer.Serialize("");

      var subit = Builder.newIterator(b);
      subit.next();
      Assert.AreEqual(DispatcherConstants.ActualType, subit.key);
      Assert.AreEqual(bson_type.BSON_STRING, subit.bsonType);
      Assert.AreEqual("PrimitivesObject", (string)subit);

      subit.next();
      Assert.AreEqual("StringProp", subit.key);
      Assert.AreEqual(obj.StringProp, (string)subit);

      subit.next();
      Assert.AreEqual("IntProp", subit.key);
      Assert.AreEqual(obj.IntProp, (int)subit);

      subit.next();
      Assert.AreEqual("DoubleProp", subit.key);
      Assert.AreEqual(obj.DoubleProp, (double)subit);

      subit.next();
      Assert.AreEqual("DateTimeProp", subit.key);
      Assert.AreEqual(obj.DateTimeProp, (DateTime)subit);

      subit.next();
      Assert.AreEqual("BoolProp", subit.key);
      Assert.AreEqual(obj.BoolProp, (bool)subit);

      subit.next();
      Assert.AreEqual("LongProp", subit.key);
      Assert.AreEqual(obj.LongProp, (long)subit);

      Assert.False(subit.next());

      var it = Builder.newIterator(b);
      object newObj = new PrimitivesObject();
      new BsonDeserializer(it).Deserialize(ref newObj);

      Assert.AreEqual(obj.BoolProp, ((PrimitivesObject)newObj).BoolProp);
      Assert.AreEqual(obj.DateTimeProp, ((PrimitivesObject)newObj).DateTimeProp);
      Assert.AreEqual(obj.DoubleProp, ((PrimitivesObject)newObj).DoubleProp);
      Assert.AreEqual(obj.IntProp, ((PrimitivesObject)newObj).IntProp);
      Assert.AreEqual(obj.LongProp, ((PrimitivesObject)newObj).LongProp);
      Assert.AreEqual(obj.StringProp, ((PrimitivesObject)newObj).StringProp);
    }

    [Test, SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public void SerializeTestObject()
    {
      var uniEncoding = new UnicodeEncoding();
      byte[] someData = uniEncoding.GetBytes("1234567890qwertyuiop");

      var obj = new TestObject
      {
        The_00_Int = 10,
        The_01_Int64 = 11L,
        The_02_AnsiChar = 'B',
        The_03_Enumeration = Enumeration2.Second,
        The_04_Float = 1.5,
        The_05_String = "дом",
        The_06_ShortString = "Hello",
        The_07_Set = Enumeration.First | Enumeration.Second,
        The_10_WChar = 'ス',
        The_11_AnsiString = "Hello World",
        The_12_WideString = "jステステl",
        The_14_VariantAsInteger = 14,
        The_15_VariantAsString = "дом дом дом",
        The_19_Boolean = true,
        The_20_DateTime = DateTime.Now,
        The_23_EmptySet = 0,
        The_08_SubObject = { TheInt = 12 }
      };
      obj.The_13_StringList.Add("дом");
      obj.The_13_StringList.Add("ом");
      obj.The_16_VariantAsList.Add(16);
      obj.The_16_VariantAsList.Add(12);
      obj.The_17_VariantTwoNestedList = new List<List<int>> { new List<int>(2), new List<int>(2) };
      obj.The_17_VariantTwoNestedList[0].Add(16);
      obj.The_17_VariantTwoNestedList[0].Add(12);
      obj.The_17_VariantTwoNestedList[1].Add(33);
      obj.The_17_VariantTwoNestedList[1].Add(44);
      obj.The_21_MemStream.Write(someData, 0, someData.Length);

      Bson b = Builder.newBson();
      _bsonSerializer.Target = b;
      _bsonSerializer.Source = obj;
      _bsonSerializer.Serialize("");

      var it = Builder.newIterator(b);

      it.next();
      Assert.AreEqual(DispatcherConstants.ActualType, it.key);
      Assert.AreEqual(bson_type.BSON_STRING, it.bsonType);
      Assert.AreEqual("TestObject", (string)it);

      it.next();
      Assert.AreEqual(obj.The_00_Int, (int)it);

      it.next();
      Assert.AreEqual(obj.The_01_Int64, (long)it);

      it.next();
      Assert.AreEqual(obj.The_02_AnsiChar.ToString(), (string)it);

      it.next();
      Assert.AreEqual(obj.The_03_Enumeration, (Enumeration2)(int)it);

      it.next();
      Assert.AreEqual(obj.The_04_Float, (double)it);

      it.next();
      Assert.AreEqual(obj.The_05_String, (string)it);

      it.next();
      Assert.AreEqual(obj.The_06_ShortString, (string)it);

      it.next();
      var subit = Builder.newIterator(it);
      subit.next();
      string enumValues = (string)subit + ", ";
      subit.next();
      enumValues += (string)subit;
      Assert.AreEqual(obj.The_07_Set.ToString(), enumValues);

      it.next();
      subit = Builder.newIterator(it);

      subit.next();
      Assert.AreEqual(DispatcherConstants.ActualType, subit.key);
      Assert.AreEqual("SubObjectInt", (string)subit);
      subit.next();
      Assert.AreEqual(obj.The_08_SubObject.TheInt, (int)subit);

      it.next();
      Assert.AreEqual(obj.The_10_WChar.ToString(), (string)it);

      it.next();
      Assert.AreEqual(obj.The_11_AnsiString, (string)it);

      it.next();
      Assert.AreEqual(obj.The_12_WideString, (string)it);

      it.next();
      subit = Builder.newIterator(it);
      foreach (string s in obj.The_13_StringList)
      {
        subit.next();
        Assert.AreEqual(s, (string)subit);
      }

      it.next();
      Assert.AreEqual(obj.The_14_VariantAsInteger, (int)it);

      it.next();
      Assert.AreEqual(obj.The_15_VariantAsString, (string)it);

      it.next();
      subit = Builder.newIterator(it);
      foreach (int i in obj.The_16_VariantAsList)
      {
        subit.next();
        Assert.AreEqual(i, (int)subit);
      }

      it.next();

      it.next();
      subit = Builder.newIterator(it);
      Assert.False(subit.next());

      it.next();
      Assert.AreEqual(obj.The_19_Boolean, (bool)it);

      it.next();
      Assert.True(Utils.MilisecondsPrecisionCompare(obj.The_20_DateTime, (DateTime)it) == 0);

      it.next();
      Assert.AreEqual(obj.The_21_MemStream.Length, ((byte[])it).Length);

      it.next();
      Assert.AreEqual(obj.The_22_BlankMemStream.Length, ((byte[])it).Length);

      it = Builder.newIterator(b);
      object newObj = new TestObject();
      new BsonDeserializer(it).Deserialize(ref newObj);

      var castedNewObj = (TestObject)newObj;
      Assert.AreEqual(obj.The_00_Int, castedNewObj.The_00_Int);
      Assert.AreEqual(obj.The_01_Int64, castedNewObj.The_01_Int64);
      Assert.AreEqual(obj.The_02_AnsiChar, castedNewObj.The_02_AnsiChar);
      Assert.AreEqual(obj.The_03_Enumeration, castedNewObj.The_03_Enumeration);
      Assert.AreEqual(obj.The_04_Float, castedNewObj.The_04_Float);
      Assert.AreEqual(obj.The_05_String, castedNewObj.The_05_String);
      Assert.AreEqual(obj.The_06_ShortString, castedNewObj.The_06_ShortString);
      Assert.AreEqual(obj.The_07_Set, castedNewObj.The_07_Set);
      Assert.AreEqual(obj.The_08_SubObject.TheInt, castedNewObj.The_08_SubObject.TheInt);
      Assert.AreEqual(obj.The_10_WChar, castedNewObj.The_10_WChar);
      Assert.AreEqual(obj.The_11_AnsiString, castedNewObj.The_11_AnsiString);
      Assert.AreEqual(obj.The_12_WideString, castedNewObj.The_12_WideString);
      Assert.That(obj.The_13_StringList, Is.EquivalentTo(castedNewObj.The_13_StringList));
      Assert.AreEqual(obj.The_14_VariantAsInteger, castedNewObj.The_14_VariantAsInteger);
      Assert.AreEqual(obj.The_15_VariantAsString, castedNewObj.The_15_VariantAsString);
      Assert.That(obj.The_16_VariantAsList, Is.EquivalentTo(castedNewObj.The_16_VariantAsList));
      Assert.That(obj.The_17_VariantTwoNestedList, Is.EquivalentTo(castedNewObj.The_17_VariantTwoNestedList));
      Assert.That(obj.The_18_VariantAsListEmpty, Is.EquivalentTo(castedNewObj.The_18_VariantAsListEmpty));
      Assert.AreEqual(obj.The_19_Boolean, castedNewObj.The_19_Boolean);
      Assert.True(Utils.MilisecondsPrecisionCompare(obj.The_20_DateTime, castedNewObj.The_20_DateTime) == 0);
      Assert.AreEqual(obj.The_21_MemStream, castedNewObj.The_21_MemStream);
      Assert.AreEqual(obj.The_22_BlankMemStream, castedNewObj.The_22_BlankMemStream);
      Assert.AreEqual(obj.The_23_EmptySet, castedNewObj.The_23_EmptySet);
      obj.The_21_MemStream.Dispose();
    }

    [Test]
    public void SerializePrimitivesType_NotSupported()
    {
      var obj = "I am of primitive type";

      var b = Builder.newBson();
      _bsonSerializer.Target = b;
      _bsonSerializer.Source = obj;
      _bsonSerializer.Serialize("");

      var subit = Builder.newIterator(b);
      {
        Assert.IsTrue(subit.next());
        Assert.AreEqual(DispatcherConstants.ActualType, subit.key);
        Assert.AreEqual(bson_type.BSON_STRING, subit.bsonType);
        Assert.AreEqual("String", (string)subit);

        Assert.IsFalse(subit.next());
      }
    }

    [Test]
    public void BuildableSerializableTypeMapTest()
    {
      var map1 = BuildableSerializableTypeMapper.Mapper;
      var map2 = BuildableSerializableTypeMapper.Mapper;
      var mapCount = map1.map.Count;

      Assert.AreEqual(mapCount, map1.map.Count);
      Assert.AreEqual(mapCount, map2.map.Count);
      map1.Register("ReviewForm1", typeof(ReviewForm));
      Assert.AreEqual(mapCount + 1, map1.map.Count);
      Assert.AreEqual(mapCount + 1, map2.map.Count);
      map2.Register("FormA1", typeof(FormA));
      Assert.AreEqual(mapCount + 2, map1.map.Count);
      Assert.AreEqual(mapCount + 2, map2.map.Count);
      map1.Unregister("FormA1");
      Assert.AreEqual(mapCount + 1, map1.map.Count);
      Assert.AreEqual(mapCount + 1, map2.map.Count);
      map2.Unregister("ReviewForm1");
      Assert.AreEqual(mapCount, map1.map.Count);
      Assert.AreEqual(mapCount, map2.map.Count);
    }

    [TestCase(typeof(TestObject), typeof(DefaultBsonSerializer))]
    [TestCase(typeof(TestStream), typeof(StreamBsonSerializer))]
    [TestCase(typeof(string), typeof(DefaultBsonSerializer))]
    public void CreateSerializer_WithoutCustomRegister_GetDefault(Type targetType, Type expected)
    {
      var serializer = Builder.newBsonSerializer(targetType);
      Assert.AreEqual(expected, serializer.GetType());
    }

    [Test]
    public void SerializeAnonymousTypeObject()
    {
      var b = Builder.newBson();
        var obj = new
        {
          BoolProp = true,
          DateTimeProp = new DateTime(2010, 2, 25),
          DoubleProp = 12.34,
          IntProp = 147,
          StringProp = "I am PrimitivesObject",
          LongProp = 4294967296L
        };
        _bsonSerializer.Target = b;
        _bsonSerializer.Source = obj;
        _bsonSerializer.Serialize("", "MyType");

        var subit = Builder.newIterator(b);
        subit.next();
        Assert.AreEqual(DispatcherConstants.ActualType, subit.key);
        Assert.AreEqual(bson_type.BSON_STRING, subit.bsonType);
        Assert.AreEqual("MyType", (string)subit);

        subit.next();
        Assert.AreEqual("BoolProp", subit.key);
        Assert.AreEqual(obj.BoolProp, (bool)subit);

        subit.next();
        Assert.AreEqual("DateTimeProp", subit.key);
        Assert.AreEqual(obj.DateTimeProp, (DateTime)subit);

        subit.next();
        Assert.AreEqual("DoubleProp", subit.key);
        Assert.AreEqual(obj.DoubleProp, (double)subit);

        subit.next();
        Assert.AreEqual("IntProp", subit.key);
        Assert.AreEqual(obj.IntProp, (int)subit);

        subit.next();
        Assert.AreEqual("StringProp", subit.key);
        Assert.AreEqual(obj.StringProp, (string)subit);

        subit.next();
        Assert.AreEqual("LongProp", subit.key);
        Assert.AreEqual(obj.LongProp, (long)subit);

        Assert.False(subit.next());

        var it = Builder.newIterator(b);
        object newObj = new PrimitivesObject();
        new BsonDeserializer(it).Deserialize(ref newObj);

        Assert.AreEqual(obj.BoolProp, ((PrimitivesObject)newObj).BoolProp);
        Assert.AreEqual(obj.DateTimeProp, ((PrimitivesObject)newObj).DateTimeProp);
        Assert.AreEqual(obj.DoubleProp, ((PrimitivesObject)newObj).DoubleProp);
        Assert.AreEqual(obj.IntProp, ((PrimitivesObject)newObj).IntProp);
        Assert.AreEqual(obj.LongProp, ((PrimitivesObject)newObj).LongProp);
        Assert.AreEqual(obj.StringProp, ((PrimitivesObject)newObj).StringProp);
    }

    [Test]
    public void SerializeAndDeserializeAnonymousTypeObject()
    {
      var b = Builder.newBson();
        var obj = new
        {
          BoolProp = true,
          DateTimeProp = new DateTime(2010, 2, 25),
          DoubleProp = 12.34,
          IntProp = 1,
          StringProp = "I am PrimitivesObject",
          LongProp = 4294967296L
        };
        _bsonSerializer.Target = b;
        _bsonSerializer.Source = obj;
        _bsonSerializer.Serialize("", "MyType");

        var subit = Builder.newIterator(b);
        subit.next();
        Assert.AreEqual(DispatcherConstants.ActualType, subit.key);
        Assert.AreEqual(bson_type.BSON_STRING, subit.bsonType);
        Assert.AreEqual("MyType", (string)subit);

        subit.next();
        Assert.AreEqual("BoolProp", subit.key);
        Assert.AreEqual(obj.BoolProp, (bool)subit);

        subit.next();
        Assert.AreEqual("DateTimeProp", subit.key);
        Assert.AreEqual(obj.DateTimeProp, (DateTime)subit);

        subit.next();
        Assert.AreEqual("DoubleProp", subit.key);
        Assert.AreEqual(obj.DoubleProp, (double)subit);

        subit.next();
        Assert.AreEqual("IntProp", subit.key);
        Assert.AreEqual(obj.IntProp, (int)subit);

        subit.next();
        Assert.AreEqual("StringProp", subit.key);
        Assert.AreEqual(obj.StringProp, (string)subit);

        subit.next();
        Assert.AreEqual("LongProp", subit.key);
        Assert.AreEqual(obj.LongProp, (long)subit);

        Assert.False(subit.next());

        var it = Builder.newIterator(b);
        var newObj = new
        {
          BoolProp = AnonymousType.Bool,
          DateTimeProp = AnonymousType.DateTime,
          DoubleProp = AnonymousType.Double,
          IntProp = AnonymousType.Int,
          StringProp = AnonymousType.String,
          LongProp = AnonymousType.Long
        };
        object refObject = newObj;
        new BsonDeserializer(it).Deserialize(ref refObject);

        Assert.AreEqual(obj.BoolProp, newObj.BoolProp);
        Assert.AreEqual(obj.DateTimeProp, newObj.DateTimeProp);
        Assert.AreEqual(obj.DoubleProp, newObj.DoubleProp);
        Assert.AreEqual(obj.IntProp, newObj.IntProp);
        Assert.AreEqual(obj.LongProp, newObj.LongProp);
        Assert.AreEqual(obj.StringProp, newObj.StringProp);
    }

    [Test]
    public void SerializAnonymousTypeObjectAndDeserializeExpandoObject()
    {
      var b = Builder.newBson();
        var obj = new
        {
          BoolProp = true,
          DateTimeProp = new DateTime(2010, 2, 25),
          DoubleProp = 12.34,
          IntProp = 1,
          StringProp = "I am PrimitivesObject",
          LongProp = 4294967296L
        };
        _bsonSerializer.Target = b;
        _bsonSerializer.Source = obj;
        _bsonSerializer.Serialize("", "MyType");

        var subit = Builder.newIterator(b);
        subit.next();
        Assert.AreEqual(DispatcherConstants.ActualType, subit.key);
        Assert.AreEqual(bson_type.BSON_STRING, subit.bsonType);
        Assert.AreEqual("MyType", (string)subit);

        subit.next();
        Assert.AreEqual("BoolProp", subit.key);
        Assert.AreEqual(obj.BoolProp, (bool)subit);

        subit.next();
        Assert.AreEqual("DateTimeProp", subit.key);
        Assert.AreEqual(obj.DateTimeProp, (DateTime)subit);

        subit.next();
        Assert.AreEqual("DoubleProp", subit.key);
        Assert.AreEqual(obj.DoubleProp, (double)subit);

        subit.next();
        Assert.AreEqual("IntProp", subit.key);
        Assert.AreEqual(obj.IntProp, (int)subit);

        subit.next();
        Assert.AreEqual("StringProp", subit.key);
        Assert.AreEqual(obj.StringProp, (string)subit);

        subit.next();
        Assert.AreEqual("LongProp", subit.key);
        Assert.AreEqual(obj.LongProp, (long)subit);

        Assert.False(subit.next());

        var it = Builder.newIterator(b);

        object refObject = null;
        new BsonDeserializer(it).Deserialize(ref refObject);

        dynamic newObj = refObject;
        Assert.AreEqual(obj.BoolProp, newObj.BoolProp);
        Assert.AreEqual(obj.DateTimeProp, newObj.DateTimeProp);
        Assert.AreEqual(obj.DoubleProp, newObj.DoubleProp);
        Assert.AreEqual(obj.IntProp, newObj.IntProp);
        Assert.AreEqual(obj.LongProp, newObj.LongProp);
        Assert.AreEqual(obj.StringProp, newObj.StringProp);
    }

    [Test]
    public void SerializeExpandoObjectAndDeserializeExpandoObject()
    {
      var b = Builder.newBson();
        dynamic obj = new ExpandoObject();
        obj.BoolProp = true;
        obj.DateTimeProp = new DateTime(2010, 2, 25);
        obj.DoubleProp = 12.34;
        obj.IntProp = 1;
        obj.StringProp = "I am PrimitivesObject";
        obj.LongProp = 4294967296L;
        obj.arr = new List<SubObjectInt> { new SubObjectInt { TheInt = 1 }, new SubObjectInt { TheInt = 2 } };
        obj.arr2 = new List<int> { 1, 2 };
        obj.arr3 = new List<object> { new { StrValue = "Hello" }, new { StrValue = "World" } };
        obj.subObj = new ExpandoObject();
        obj.subObj.a = 1;
        _bsonSerializer.Target = b;
        _bsonSerializer.Source = obj;
        _bsonSerializer.Serialize("", "MyType");

        var subit = Builder.newIterator(b);
        subit.next();
        Assert.AreEqual(DispatcherConstants.ActualType, subit.key);
        Assert.AreEqual(bson_type.BSON_STRING, subit.bsonType);
        Assert.AreEqual("MyType", (string)subit);

        subit.next();
        Assert.AreEqual("BoolProp", subit.key);
        Assert.AreEqual(obj.BoolProp, (bool)subit);

        subit.next();
        Assert.AreEqual("DateTimeProp", subit.key);
        Assert.AreEqual(obj.DateTimeProp, (DateTime)subit);

        subit.next();
        Assert.AreEqual("DoubleProp", subit.key);
        Assert.AreEqual(obj.DoubleProp, (double)subit);

        subit.next();
        Assert.AreEqual("IntProp", subit.key);
        Assert.AreEqual(obj.IntProp, (int)subit);

        subit.next();
        Assert.AreEqual("StringProp", subit.key);
        Assert.AreEqual(obj.StringProp, (string)subit);

        subit.next();
        Assert.AreEqual("LongProp", subit.key);
        Assert.AreEqual(obj.LongProp, (long)subit);

        Assert.True(subit.next());
        Assert.AreEqual(bson_type.BSON_ARRAY, subit.bsonType);
        var arrayit = Builder.newIterator(subit);
        Assert.True(arrayit.next());
        Assert.AreEqual(bson_type.BSON_OBJECT, arrayit.bsonType);
        var subObjIt = Builder.newIterator(arrayit);
        Assert.True(subObjIt.next());
        Assert.AreEqual("SubObjectInt", (string)subObjIt);
        Assert.True(subObjIt.next());
        Assert.AreEqual(obj.arr[0].TheInt, (int)subObjIt);
        Assert.False(subObjIt.next());
        Assert.True(arrayit.next());
        Assert.AreEqual(bson_type.BSON_OBJECT, arrayit.bsonType);
        subObjIt = Builder.newIterator(arrayit);
        Assert.True(subObjIt.next());
        Assert.AreEqual("SubObjectInt", (string)subObjIt);
        Assert.True(subObjIt.next());
        Assert.AreEqual(obj.arr[1].TheInt, (int)subObjIt);
        Assert.False(subObjIt.next());
        Assert.False(arrayit.next());

        Assert.True(subit.next());
        Assert.AreEqual(bson_type.BSON_ARRAY, subit.bsonType);
        arrayit = Builder.newIterator(subit);
        Assert.True(arrayit.next());
        Assert.AreEqual(bson_type.BSON_INT, arrayit.bsonType);
        Assert.AreEqual(obj.arr2[0], (int)arrayit);
        Assert.True(arrayit.next());
        Assert.AreEqual(bson_type.BSON_INT, arrayit.bsonType);
        Assert.AreEqual(obj.arr2[1], (int)arrayit);
        Assert.False(arrayit.next());

        Assert.True(subit.next());
        Assert.AreEqual(bson_type.BSON_ARRAY, subit.bsonType);
        arrayit = Builder.newIterator(subit);
        Assert.True(arrayit.next());
        Assert.AreEqual(bson_type.BSON_OBJECT, arrayit.bsonType);
        subObjIt = Builder.newIterator(arrayit);
        Assert.True(subObjIt.next());
        Assert.AreEqual(obj.arr3[0].StrValue, (string)subObjIt);
        Assert.False(subObjIt.next());
        Assert.True(arrayit.next());
        Assert.AreEqual(bson_type.BSON_OBJECT, arrayit.bsonType);
        subObjIt = Builder.newIterator(arrayit);
        Assert.True(subObjIt.next());
        Assert.AreEqual(obj.arr3[1].StrValue, (string)subObjIt);
        Assert.False(subObjIt.next());
        Assert.False(arrayit.next());

        Assert.True(subit.next());
        Assert.AreEqual(bson_type.BSON_OBJECT, subit.bsonType);
        subObjIt = Builder.newIterator(subit);
        Assert.True(subObjIt.next());
        Assert.AreEqual(1, (int)subObjIt);
        Assert.False(subObjIt.next());

        Assert.False(subit.next());

        var it = Builder.newIterator(b);

        object refObject = null;
        new BsonDeserializer(it).Deserialize(ref refObject);

        dynamic newObj = refObject;
        Assert.AreEqual(obj.BoolProp, newObj.BoolProp);
        Assert.AreEqual(obj.DateTimeProp, newObj.DateTimeProp);
        Assert.AreEqual(obj.DoubleProp, newObj.DoubleProp);
        Assert.AreEqual(obj.IntProp, newObj.IntProp);
        Assert.AreEqual(obj.LongProp, newObj.LongProp);
        Assert.AreEqual(obj.StringProp, newObj.StringProp);
        Assert.AreEqual(obj.arr[0].TheInt, newObj.arr[0].TheInt);
        Assert.AreEqual(obj.arr[1].TheInt, newObj.arr[1].TheInt);
        Assert.AreEqual(obj.arr2[0], newObj.arr2[0]);
        Assert.AreEqual(obj.arr2[1], newObj.arr2[1]);
        Assert.AreEqual(obj.subObj.a, newObj.subObj.a);
    }

    [Test]
    public void CreateSerializer_AfterRemoveAllDefaults_ThrowException()
    {
#pragma warning disable 0618
      BaseBsonSerializer.Clear();
#pragma warning restore 0618

      Assert.Throws<BsonSerializerException>(() =>
      {
        Builder.newBsonSerializer(typeof(string));
      });
    }

    [Test]
    public void CreatedSerializer_AfterRemoveCustom_ShouldBeDefault()
    {
      var targetType = typeof(TestStream);
      BaseBsonSerializer.RegisterSerializer(targetType, typeof(DumyStreamSerializer));

      BaseBsonSerializer.UnregisterSerializer(targetType, typeof(DumyStreamSerializer));

      var serializer = Builder.newBsonSerializer(targetType);
      Assert.AreEqual(typeof(StreamBsonSerializer), serializer.GetType());
    }
  }
}