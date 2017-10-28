using System;
using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.Capability;
using Sovos.SvcBus.Common.Model.Operation;

namespace ModelUT.Capability
{
    public class TestClass
    {
        public string TestString { get; set; }
        public bool TestBool { get; set; }
        public int TestInt { get; set; }
        public DateTime TestDateTime { get; set; }

        public List<string> ListOfString { get; set; }

        public DataDto TestCustom { get; set; }
        public List<DataDto> TestCustomList { get; set; }

        public User TestUser{ get; set; }
        public List<User> TestUserList { get; set; }

        public Dictionary<object, object> TestDictionary { get; set; }
        public TestDictionaryClass TestCustomMap { get; set; }
    }

    public class TestDictionaryClass
    {
        public Dictionary<object, object> TestMap { get; set; }
    }
}
