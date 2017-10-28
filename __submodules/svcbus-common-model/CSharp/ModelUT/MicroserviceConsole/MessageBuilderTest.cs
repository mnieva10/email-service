using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sovos.SvcBus.Common.Model.Extensions;
using Sovos.SvcBus.Common.Model.MicroserviceConsole;
using Sovos.SvcBus.Common.Model.MicroserviceConsole.Exceptions;
using Sovos.SvcBus.Common.Model.Operation;
using Sovos.SvcBus.Common.Model.Services;
using ModelUT.Capability;
using ModelUT.Services.Stubs;
using NUnit.Framework;

namespace ModelUT.MicroserviceConsole
{
    [TestFixture]
    public class MessageBuilderTest
    {
        [Test]
        public void ParseParameter()
        {
            const string command = "a b c d /e:test /j:{\"some\":\"json string\", \"oneMore\":\"100\"} -f -g";
            var list = command.SplitCommandLine().ToList();
            var svc = new MessageBuilderExt();
            Assert.AreEqual("test", svc.ParseParameter(list, "/e:"));
            Assert.That(svc.ParseParameter(list, "/d:"), Is.Null.Or.Empty);
            Assert.AreEqual("{\"some\":\"json string\", \"oneMore\":\"100\"}", svc.ParseParameter(list, "/j:"));
        }

        [Test]
        public void ParseArguments()
        {
            const string command = "a b c d /e:test /f.j:\"some f test\" /g /j:{\"some\":\"json\"}";
            var list = command.SplitCommandLine().ToList();
            var svc = new MessageBuilderExt();

            var parsedArgs = svc.ParseArguments(list);
            Assert.AreEqual(2, parsedArgs.Count());
            Assert.AreEqual("test", parsedArgs[0].Value);
            Assert.AreEqual("some f test", parsedArgs[1].Value);
            Assert.AreEqual(2, parsedArgs[1].Properties.Count());
            Assert.AreEqual("f", parsedArgs[1].Properties[0]);
            Assert.AreEqual("j", parsedArgs[1].Properties[1]);
        }

        [Test]
        public void ConvertDynamicNoMatch()
        {
            var svc = new MessageBuilderExt();
            var map = new Dictionary<string, object>
                {
                    {"TestString", "test"},
                    {"TestBool", true},
                    {"TestInt", 123}
                };

            var obj = new TypeConverter().ConvertDynamic(map, typeof(User));
            Assert.IsNotNull(obj);
            Assert.IsEmpty(((User)obj).Profiles);
        }

        [Test]
        public void ConvertDynamic_BaseTypes()
        {
            var svc = new MessageBuilderExt();
            svc.RegisteredTypes.Add("TestClass", typeof(TestClass));

            var dt = DateTime.Now;
            var map = new Dictionary<string, object>
                {
                    {"TestString", "test"},
                    {"TestBool", true},
                    {"TestDateTime", dt},
                    {"TestInt", 123}
                };

            var obj = new TypeConverter().ConvertDynamic(map, typeof(TestClass));
            Assert.AreEqual("test", ((TestClass)obj).TestString);
            Assert.AreEqual(true, ((TestClass)obj).TestBool);
            Assert.AreEqual(dt, ((TestClass)obj).TestDateTime);
            Assert.AreEqual(123, ((TestClass)obj).TestInt);

            obj = new TypeConverter().ConvertDynamic<TestClass>(map);
            Assert.AreEqual("test", ((TestClass)obj).TestString);
            Assert.AreEqual(true, ((TestClass)obj).TestBool);
            Assert.AreEqual(dt, ((TestClass)obj).TestDateTime);
            Assert.AreEqual(123, ((TestClass)obj).TestInt);
        }

        [Test]
        public void ConvertDynamic_BaseList()
        {
            var svc = new MessageBuilderExt();
            svc.RegisteredTypes.Add("TestClass", typeof(TestClass));

            var map = new Dictionary<string, object>
                {
                    {"TestString", "test"},
                    {"ListOfString", new List<object> {"a", "b"}},
                };

            var obj = new TypeConverter().ConvertDynamic(map, typeof(TestClass));
            Assert.AreEqual(2, ((TestClass)obj).ListOfString.Count);
        }

        [Test]
        public void BuildObjectMap_BaseTypes()
        {
            const string command = "DoSmth /t:TestClass /TestString:\"my long string\" /TestBool:true /TestInt:12345 /TestDateTime:8/27/2014";
            var list = command.SplitCommandLine().ToList();
            var svc = new MessageBuilderExt();
            var parsedArgs = svc.ParseArguments(list);
            svc.RegisteredTypes.Add("TestClass", typeof(TestClass));

            var map = new Dictionary<string, object>();
            svc.BuildObjectMap(map, parsedArgs);

            Assert.AreEqual(4, map.Keys.Count);
            var obj = new TypeConverter().ConvertDynamic(map, typeof(TestClass));
            Assert.AreEqual("my long string", ((TestClass)obj).TestString);
            Assert.AreEqual(true, ((TestClass)obj).TestBool);
            Assert.AreEqual("8/27/2014", ((TestClass)obj).TestDateTime.ToString("M/d/yyyy"));
            Assert.AreEqual(12345, ((TestClass)obj).TestInt);
        }

        [Test]
        public void BuildObjectMap_BaseList()
        {
            const string command = "DoSmth /t:TestClass /TestInt:12345 /ListOfString^:a /ListOfString^:\"b b\"";
            var list = command.SplitCommandLine().ToList();
            var svc = new MessageBuilderExt();
            var parsedArgs = svc.ParseArguments(list);
            svc.RegisteredTypes.Add("TestClass", typeof(TestClass));

            var map = new Dictionary<string, object>();
            svc.BuildObjectMap(map, parsedArgs);

            Assert.AreEqual(2, map.Keys.Count);
            var obj = new TypeConverter().ConvertDynamic(map, typeof(TestClass));
            Assert.AreEqual(2, ((TestClass)obj).ListOfString.Count);
        }

        [Test]
        public void BuildObjectMap_CustomType()
        {
            const string command = "DoSmth /t:TestClass /TestInt:12345 /TestCustom.DataString:a /TestUser.Username:uuu /TestUser.Password:ppp";
            var list = command.SplitCommandLine().ToList();
            var svc = new MessageBuilderExt();
            var parsedArgs = svc.ParseArguments(list);
            svc.RegisteredTypes.Add("TestClass", typeof(TestClass));

            var map = new Dictionary<string, object>();
            svc.BuildObjectMap(map, parsedArgs);

           // Assert.AreEqual(2, map.Keys.Count);
            var obj = new TypeConverter().ConvertDynamic(map, typeof(TestClass));
            Assert.AreEqual("UUU", ((TestClass)obj).TestUser.Username);
            Assert.AreEqual("ppp", ((TestClass)obj).TestUser.Password);
        }

        [Test]
        public void BuildObjectMap_CustomList()
        {
            const string command = "DoSmth /t:TestClass /TestInt:12345 /TestCustomList^.DataString:a /TestCustomList^.DataString:abc";
            var list = command.SplitCommandLine().ToList();
            var svc = new MessageBuilderExt();
            var parsedArgs = svc.ParseArguments(list);
            svc.RegisteredTypes.Add("TestClass", typeof(TestClass));

            var map = new Dictionary<string, object>();
            svc.BuildObjectMap(map, parsedArgs);

            Assert.AreEqual(2, map.Keys.Count);
            var obj = new TypeConverter().ConvertDynamic(map, typeof(TestClass));
            Assert.AreEqual(2, ((TestClass)obj).TestCustomList.Count);
        }

        [Test]
        public void BuildObjectMap_ComplexCase()
        {
            const string command = @"DoSmth /t:TestClass /TestInt:12345 /TestUser.Username:aaa
                            /TestUser.Profiles^.ProfileName:P1 /TestUser.Profiles^.Priority:1 
                            /TestUser.Profiles^.ProfileName:P2 /TestUser.Profiles^.Priority:2 /TestUser.Schema:sss
                            /TestUserList^.Profiles^.ProfileName:P1list /TestUserList^.Username:uuu /TestUserList^.Profiles^.ProfileName:P2list";
            var list = command.SplitCommandLine().ToList();
            var svc = new MessageBuilderExt();
            var parsedArgs = svc.ParseArguments(list);
            svc.RegisteredTypes.Add("TestClass", typeof(TestClass));

            var map = new Dictionary<string, object>();
            svc.BuildObjectMap(map, parsedArgs);

            Assert.AreEqual(3, map.Keys.Count);
            var obj = new TypeConverter().ConvertDynamic(map, typeof(TestClass));
            Assert.AreEqual(4, ((TestClass)obj).TestUser.Profiles.Count);
        }

        [Test]
        public void BuildObjectMap_Dictionary()
        {
            const string command = "DoSmth /t:TestClass /TestCustomMap.TestMap~:tt~ttv /TestCustomMap.TestMap~:cc~ccv /TestInt:12345 /TestDictionary~:key~\"value 100\" /TestDictionary~:\"key 1\"~200";
            var list = command.SplitCommandLine().ToList();
            var svc = new MessageBuilderExt();
            var parsedArgs = svc.ParseArguments(list);
            svc.RegisteredTypes.Add("TestClass", typeof(TestClass));
            svc.RegisteredTypes.Add("TestDictionaryClass", typeof(TestClass));

            var map = new Dictionary<string, object>();
            svc.BuildObjectMap(map, parsedArgs);

            Assert.AreEqual(3, map.Keys.Count);
            var obj = new TypeConverter().ConvertDynamic(map, typeof(TestClass));
            Assert.AreEqual(2, ((TestClass)obj).TestCustomMap.TestMap.Count);
        }

        [Test]
        public void ParseObject_ComplexCase([Values("DoSmth /t:TestClass /TestInt:12345 /TestUser.Username:aaa /TestUser.Profiles^.ProfileName:P1 /TestUser.Schema:sss /TestUser.Profiles^.ProfileName:P2",
            "DoJson /t:TestClass /j:{\"TestInt\":\"12345\", \"TestUser\":{\"Username\":\"aaa\", \"Profiles\":[{\"ProfileName\":\"P1\"}, {\"ProfileName\":\"P2\"}], \"Schema\":\"sss\"}}")] string command)
        {
            var list = command.SplitCommandLine().ToList();
            var svc = new MessageBuilderExt();
            svc.RegisteredTypes.Add("TestClass", typeof(TestClass));

            Assert.AreEqual(typeof(TestClass), svc.ParseObjectType(list));
            var obj = (TestClass)svc.ParseObject(list, typeof(TestClass));
            Assert.AreEqual(2, obj.TestUser.Profiles.Count);
            Assert.AreEqual("AAA", obj.TestUser.Username);
            Assert.AreEqual(12345, obj.TestInt);
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), Test]
        //[ExpectedException(typeof(TypeNotFoundException))]
        public void Build_TypeNotFound()
        {
            var svc = new MessageBuilder();
            const string command = "DoSmth /p:params";
            svc.Build(command.SplitCommandLine().ToList());
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), Test]
        public void Build_TypeNotRegistered()
        {
            Assert.Throws<TypeNotRegisteredException>(() =>
            {
                var svc = new MessageBuilder();
                const string command = "DoSmth /p:params /t:NewClass";
                svc.Build(command.SplitCommandLine().ToList());
            });
        }

        [Test]
        public void Constructor_RegisterTypes()
        {
            var svc = new MessageBuilder();
            Assert.IsNotNull(svc.RegisteredTypes);
            Assert.Greater(svc.RegisteredTypes.Count, 5);
            Assert.IsFalse(svc.RegisteredTypes.ContainsKey("TestObject"));
        }
    }
}
