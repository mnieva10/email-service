using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using System.Web.Script.Serialization;
using Sovos.SvcBus;
using Sovos.SvcBus.Common.Model.Capability;
using Sovos.SvcBus.Common.Model.Extensions;
using Sovos.SvcBus.Common.Model.MicroserviceConsole;
using Sovos.SvcBus.Common.Model.Operation;

namespace ModelUT.MicroserviceConsole
{
    //todo: needs to be refactored 
    //using JsonTestFactory to create test objects as min
    //renaming tests to be meaningful

    [TestFixture]
    public class JsonFormatterTest
    {
        private JSonTestObject1 _jsonObj1;
        private JSonTestObject1 _jsonObj2;
        private JsonFormatter _format;
        private JSonTestObject3 _jsonObj3;
        private JSonTestObject1 _jsonObj4;
        private JSonTestObject1 _jsonObj5;
        private List<int> _intList;
        [SvcBusSerializable]
        private Dictionary<string, int> Dict { get; set; }
        private List<JSonTestObject1> _classList;
        private List<JSonTestObject2> _classList2;
        private JSonTestObject2 _jsonObj11;
        private JSonTestObject2 _jsonObj12;
        private JSonTestObject4 _jsonObj13;

        private DataDto _dataDto;
        private DataDto _dataDto2;

        public class JSonTestObject1
        {
            public DateTime MyDateTime;
            public string MyString;
            public string MyNullString;
            public int MyInt;
            public MemoryStream MyMemStream;
        }

        public class JSonTestObject2
        {
            public string MyString;
            public string MyNullString;
        }

        public class JSonTestObject3
        {
            public DateTime MyDateTime;
            public string MyString;
            public string MyNullString;
            public int MyInt;
            public JSonTestObject1 MyTestObject;
        }

        public class JSonTestObject4
        {
            public string MyString;
            public string MyNullString;
            public List<int> IntList;
        }

        [SetUp]
        public void SetUp()
        {
            _format = new JsonFormatter();

            _jsonObj1 = new JSonTestObject1();
            var bigDay = new DateTime(2015,9,14,8,30,0);
            _jsonObj1.MyDateTime = bigDay;
            _jsonObj1.MyString = "TestString";
            _jsonObj1.MyNullString = null;
            _jsonObj1.MyInt = 42;           
            var newSettingValue = "new setting value";
            _jsonObj1.MyMemStream = newSettingValue.ToMemoryStream();

            _jsonObj2 = new JSonTestObject1();
            bigDay = new DateTime(2017, 12, 25, 6, 0, 0);
            _jsonObj2.MyDateTime = bigDay;
            _jsonObj2.MyString = "TestString2";
            _jsonObj2.MyNullString = null;
            _jsonObj2.MyInt = 4200;
            newSettingValue = "new setting value  more more moar";
            _jsonObj2.MyMemStream = newSettingValue.ToMemoryStream();

             _jsonObj3 = new JSonTestObject3 {MyTestObject = _jsonObj1};
            bigDay = new DateTime(2012, 1, 1, 8, 30, 0);
            _jsonObj3.MyDateTime = bigDay;
            _jsonObj3.MyString = "TestString for json obj3";
            _jsonObj3.MyNullString = null;

            _jsonObj4 = new JSonTestObject1();
            bigDay = new DateTime(2017, 12, 25, 6, 0, 0);
            _jsonObj4.MyDateTime = bigDay;
            _jsonObj4.MyString = "x:\\testing\\test.txt";
            _jsonObj4.MyNullString = null;
            _jsonObj4.MyInt = 100;
            newSettingValue = "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz  that was 100 zs";
            _jsonObj4.MyMemStream = newSettingValue.ToMemoryStream();

            _jsonObj5 = new JSonTestObject1();
            bigDay = new DateTime(2017, 12, 25, 6, 0, 0);
            _jsonObj5.MyDateTime = bigDay;
            _jsonObj5.MyString = "TestString2";
            _jsonObj5.MyNullString = null;
            _jsonObj5.MyInt = 100;
            _jsonObj5.MyMemStream = null;

            _intList = new List<int> {4, 3, 2, 1};
            Dict = new Dictionary<string, int> {{"one", 1}, {"two", 2}, {"three", 3}};

            _classList = new List<JSonTestObject1> {_jsonObj5, _jsonObj5};

            _jsonObj11 = new JSonTestObject2 {MyString = "ROB IS KEWL"};
            _jsonObj12 = new JSonTestObject2();
            _jsonObj11.MyString = "ROB IS badbad";

            _classList2 = new List<JSonTestObject2> {_jsonObj11, _jsonObj12, _jsonObj11};

            _jsonObj13 = new JSonTestObject4 {MyString = "ploppy plop", IntList = _intList};

            _dataDto = new DataDto
                {
                    DataBool = true,
                    DataInt = 9,
                    DataList = new List<Object>(),
                    DataString = "Data String",
                    DataMap = new Dictionary<string, object> {{"String1", _jsonObj11}}
                };

            var myResource = new Resource {MacroParameters = new MacroParameters()};
            bigDay = new DateTime(2017, 12, 25, 6, 0, 0);
            myResource.DateChanged = bigDay;
            _dataDto.DataList.Add(myResource);
            
            _dataDto2 = new DataDto
                {
                    DataBool = true,
                    DataInt = 9,
                    DataList = new List<Object>(),
                    DataString = "Data String"
                };

            _dataDto2.DataList.Add(_jsonObj1);
            _dataDto2.DataList.Add(_jsonObj2);
            _dataDto2.DataList.Add(_jsonObj4); 
        }

        [Test]
        public void ComplexParseTest1()
        {
            var output = _format.SafeSerialize(_jsonObj1);
            Assert.AreEqual("{\"MyDateTime\":9/14/2015 8:30:00 AM,\"MyString\":\"TestString\",\"MyNullString\":null,\"MyInt\":42,\"MyMemStream\":\"new setting value\"  Stream Length 17}", output);
        }

        [Test]
        public void ComplexParseTest2()
        {
            var output = _format.SafeSerialize(_jsonObj2);
            Assert.AreEqual("{\"MyDateTime\":12/25/2017 6:00:00 AM,\"MyString\":\"TestString2\",\"MyNullString\":null,\"MyInt\":4200,\"MyMemStream\":\"new setting value  more more moar\"  Stream Length 33}", output);
        }

        [Test]
        public void ComplexParseTest3()
        {
            var output = _format.SafeSerialize(_jsonObj3);
            Assert.AreEqual("{\"MyDateTime\":1/1/2012 8:30:00 AM,\"MyString\":\"TestString for json obj3\",\"MyNullString\":null,\"MyInt\":0,\"MyTestObject\":{\"MyDateTime\":9/14/2015 8:30:00 AM,\"MyString\":\"TestString\",\"MyNullString\":null,\"MyInt\":42,\"MyMemStream\":\"new setting value\"  Stream Length 17}}", output);
        }

        [Test]
        public void ComplexParseTest4()
        {
            var output = _format.SafeSerialize(_jsonObj4);
            Assert.AreEqual("{\"MyDateTime\":12/25/2017 6:00:00 AM,\"MyString\":\"x:\\\\testing\\\\test.txt\",\"MyNullString\":null,\"MyInt\":100,\"MyMemStream\":\"zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz\"  Stream Length 117}", output);
            var jsonOut = _format.Print(output);
            jsonOut = _format.Print(output);
        }
        [Test]
        public void ComplexParseTest5()
        {
            var output = _format.SafeSerialize(_jsonObj5);
            Assert.AreEqual("{\"MyDateTime\":12/25/2017 6:00:00 AM,\"MyString\":\"TestString2\",\"MyNullString\":null,\"MyInt\":100,\"MyMemStream\":null}", output);
        }
        [Test]
        public void ComplexParseTest6()
        {
            var output = _format.SafeSerialize(_jsonObj13);
            Assert.AreEqual("{\"MyString\":\"ploppy plop\",\"MyNullString\":null,\"IntList\":[4,3,2,1]}", output);
        }
        
        [Test]
        public void SimpleParseTest1()
        {
            var jObj1 = new JSonTestObject2 {MyString = "ROB IS KEWL"};
            var output = _format.SafeSerialize(jObj1);
            Assert.AreEqual("{\"MyString\":\"ROB IS KEWL\",\"MyNullString\":null}", output);
            var myObj = new JavaScriptSerializer().Deserialize(output, jObj1.GetType());
            Assert.IsNotNull(myObj);
        }

        [Test]
        public void SimpleParseTest2()
        {
            var jObj1 = new JSonTestObject2 {MyString = ""};
            var output = _format.SafeSerialize(jObj1);
            Assert.AreEqual("{\"MyString\":\"\",\"MyNullString\":null}", output);
            var myObj = new JavaScriptSerializer().Deserialize(output, jObj1.GetType());
            Assert.IsNotNull(myObj);
        }

        [Test]
        public void StringParseTest1()
        {
            const string myString = "Hi there, how are you?";
            var output = _format.SafeSerialize(myString);
            Assert.AreEqual("\"Hi there, how are you?\"", output);
        }

        [Test]
        public void IntParseTest1()
        {
            const int myInt = 9;
            var output = _format.SafeSerialize(myInt);
            Assert.AreEqual("9", output);
        }

        [Test]
        public void DoubleParseTest1()
        {
            const double myDouble = 3.1415926;
            var output = _format.SafeSerialize(myDouble);
            Assert.AreEqual("3.1415926", output);
        }

        [Test]
        public void IntListParseTest1()
        {
            var output = _format.SafeSerialize(_intList);
            Assert.AreEqual("[4,3,2,1]", output);
            var myObj = new JavaScriptSerializer().Deserialize(output, _intList.GetType());
            Assert.IsNotNull(myObj);
        }

        [Test]
        public void ClassListParseTest1()
        {
            var output = _format.SafeSerialize(_classList);
            Assert.AreEqual("[{\"MyDateTime\":12/25/2017 6:00:00 AM,\"MyString\":\"TestString2\",\"MyNullString\":null,\"MyInt\":100,\"MyMemStream\":null},{\"MyDateTime\":12/25/2017 6:00:00 AM,\"MyString\":\"TestString2\",\"MyNullString\":null,\"MyInt\":100,\"MyMemStream\":null}]", output);
        }

        [Test]
        public void ClassListParseTest2()
        {
            var output = _format.SafeSerialize(_classList2);
            Assert.AreEqual("[{\"MyString\":\"ROB IS badbad\",\"MyNullString\":null},{\"MyString\":null,\"MyNullString\":null},{\"MyString\":\"ROB IS badbad\",\"MyNullString\":null}]", output);
        }

        [Test]
        public void DictParseTest1()
        {
            var output = _format.SafeSerialize(Dict);
            Assert.AreEqual("{{\"one\":1,\"two\":2,\"three\":3}}", output);
        }

        [Test]
        public void NullParseTest1()
        {
            JSonTestObject1 nullObj = null;
            var output = _format.SafeSerialize(nullObj);
            Assert.AreEqual("null", output);
        }

        [Test]
        public void DataDtoParseTest1()
        {
            var output = _format.SafeSerialize(_dataDto);
            Assert.AreEqual("{\"Domain\":null,\"Schema\":null,\"TablePrefix\":null,\"DataString\":\"Data String\",\"DataBool\":true,\"DataInt\":9,\"DataObject\":null,\"DataDateTime\":1/1/0001 12:00:00 AM,\"DataList\":[{\"ResourceName\":null,\"Script\":null,\"ParsedScript\":null,\"DateChanged\":12/25/2017 6:00:00 AM,\"MacroParameters\":{\"Defs\":{},\"Vars\":{}}}],\"DataMap\":{\"String1\":{\"MyString\":\"ROB IS badbad\",\"MyNullString\":null}},\"MachineName\":null,\"Pid\":0}", output);
        }

        [Test]
        public void DataDtoParseTest2()
        {
            var output = _format.SafeSerialize(_dataDto2);
            Assert.AreEqual("{\"Domain\":null,\"Schema\":null,\"TablePrefix\":null,\"DataString\":\"Data String\",\"DataBool\":true,\"DataInt\":9,\"DataObject\":null,\"DataDateTime\":1/1/0001 12:00:00 AM,\"DataList\":[{\"MyDateTime\":9/14/2015 8:30:00 AM,\"MyString\":\"TestString\",\"MyNullString\":null,\"MyInt\":42,\"MyMemStream\":\"new setting value\"  Stream Length 17},{\"MyDateTime\":12/25/2017 6:00:00 AM,\"MyString\":\"TestString2\",\"MyNullString\":null,\"MyInt\":4200,\"MyMemStream\":\"new setting value  more more moar\"  Stream Length 33},{\"MyDateTime\":12/25/2017 6:00:00 AM,\"MyString\":\"x:\\\\testing\\\\test.txt\",\"MyNullString\":null,\"MyInt\":100,\"MyMemStream\":\"zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz\"  Stream Length 117}],\"DataMap\":{},\"MachineName\":null,\"Pid\":0}", output);
        } 
    }
}
