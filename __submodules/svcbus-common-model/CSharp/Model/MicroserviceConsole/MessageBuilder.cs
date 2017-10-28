using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Sovos.SvcBus.Common.Model.Extensions;
using Sovos.SvcBus.Common.Model.MicroserviceConsole.Capability;
using Sovos.SvcBus.Common.Model.MicroserviceConsole.Exceptions;
using Sovos.SvcBus.Common.Model.Services;

namespace Sovos.SvcBus.Common.Model.MicroserviceConsole
{
    /// <summary>
    /// Supported property mappings (base type (BT), custom type (CT)):
    /// /A:aaa - A of BT
    /// /A^:aaa - A of BT List 
    /// /A.B:bbb - A of CT with property B of BT
    /// /A.B^:bbb - A of CT with property B of BT List
    /// /A^.B:ccc - A of CT List with property B of BT
    /// /A.B^.C:ccc - A of CT with property B of CT List with property C of BT
    /// /A~:key~value - A of BT Dictionary
    /// /A.B~:key~value - A of CT with property B of BT Dictionary
    /// 
    /// /j:{json object}
    /// </summary>
    public class MessageBuilder
    {
        public Dictionary<string, Type> RegisteredTypes { get; set; }
        public object RequestObject { get; private set; }
        private TypeConverter _typeConverter;

        public MessageBuilder()
        {
            RegisteredTypes = new Dictionary<string, Type>();
            _typeConverter = new TypeConverter(new JsonTypeResolver(RegisteredTypes));
            BuildMap();
        }

        public void RegisterType(string name, Type type)
        {
            if (!RegisteredTypes.ContainsKey(name))
                RegisteredTypes.Add(name, type);
        }

        public object ParseObject(List<string> arguments, Type objectType)
        {
            var jsonString = ParseParameter(arguments, "/j:");
            object request;
            if (string.IsNullOrEmpty(jsonString))
            {
                dynamic obj = new ExpandoObject();
                var objMap = (IDictionary<string, object>)obj;

                BuildObjectMap(objMap, ParseArguments(arguments));
                request = _typeConverter.ConvertDynamic(obj, objectType);
            }
            else
                request = _typeConverter.DeserializeJson(jsonString, objectType);

            return request;
        }

        public Message Build(List<string> arguments)
        {
            RequestObject = new object();
            var objectType = ParseObjectType(arguments);
            RequestObject = ParseObject(arguments, objectType);

            var msg = Builder.newMessage(Builder.newOid());
            var b = msg.bson;

            var serializer = Builder.newBsonSerializer(objectType, RequestObject, b);
            serializer.Serialize(DispatcherConstants.Params);
            return msg;
        }

        #region Private Methods

        private void BuildMap()
        {
#if !NETCORE
            var assembly = Assembly.GetExecutingAssembly();
            var types = from type in assembly.GetTypes()
                        where Attribute.IsDefined(type, typeof(SvcBusBuildableAttribute))
#else
            var assembly = typeof(MessageBuilder).GetTypeInfo().Assembly;
            var types = from type in assembly.GetTypes()
                        where type.GetTypeInfo().IsDefined(typeof(SvcBusBuildableAttribute))
#endif
                        select type;
            foreach (var type in types)
                RegisteredTypes.Add(type.Name, type);

            var map = BuildableSerializableTypeMapper.Mapper;
            foreach (var type in RegisteredTypes)
                map.Register(type.Key, type.Value);
        }

        private void SetComplexNonEnumerable(IDictionary<string, object> objMap, string propertyName, ExpandoObject subObjMap)
        {
            if (objMap.ContainsKey(propertyName))
                objMap[propertyName] = subObjMap;
            else
                objMap.Add(propertyName, subObjMap);
        }

        private void SetComplexEnumerable(IDictionary<string, object> objMap, string propertyName, ExpandoObject subObjMap)
        {
            if (objMap.ContainsKey(propertyName))
                ((List<ExpandoObject>)objMap[propertyName]).Add(subObjMap);
            else
                objMap.Add(propertyName, new List<ExpandoObject> { subObjMap });
        }

        private void SetBaseDictionary(IDictionary<string, object> objMap, string propertyName, string value, ref object newValueObject)
        {
            var tokens = value.Split('~');
            if (tokens.Length == 2)
            {
                var mapKey = tokens[0].TrimMatchingQuotes('\"');
                var mapValue = tokens[1].TrimMatchingQuotes('\"');
                if (objMap.ContainsKey(propertyName))
                    ((Dictionary<object, object>)objMap[propertyName]).Add(mapKey, mapValue);
                else
                    newValueObject = new Dictionary<object, object> { { mapKey, mapValue } };
            }
        }

        private void SetBaseEnumerable(IDictionary<string, object> objMap, string propertyName, string value, ref object newValueObject)
        {
            if (objMap.ContainsKey(propertyName))
                ((List<object>)objMap[propertyName]).Add(value);
            else
                newValueObject = new List<object> { value };
        }

        #endregion

        protected void BuildObjectMap(IDictionary<string, object> objMap, List<CmdParameter> properties)
        {
            foreach (var prop in properties)
                ProcessProperty(objMap, prop, 1);
        }

        protected void ProcessProperty(IDictionary<string, object> objMap, CmdParameter prop, int index)
        {
            var propertyName = prop.Properties[index - 1];
            var isEnumerable = propertyName.EndsWith("^");
            var isDictionary = propertyName.EndsWith("~");
            var isBaseTypeProperty = index == prop.Properties.Length;

            if (isEnumerable || isDictionary)
                propertyName = propertyName.Substring(0, propertyName.Length - 1);

            if (isBaseTypeProperty)
            {
                var newValueObject = new object();

                if (isEnumerable)
                    SetBaseEnumerable(objMap, propertyName, prop.Value, ref newValueObject);
                else if (isDictionary)
                    SetBaseDictionary(objMap, propertyName, prop.Value, ref newValueObject);
                else
                    newValueObject = prop.Value;

                if (newValueObject.GetType() != typeof(object))
                    objMap.Add(propertyName, newValueObject);
            }
            else
            {
                var subObjMap = objMap.ContainsKey(propertyName) && !isEnumerable ? (ExpandoObject)objMap[propertyName] : new ExpandoObject();
                ProcessProperty(subObjMap, prop, index + 1);

                if (!isEnumerable)
                    SetComplexNonEnumerable(objMap, propertyName, subObjMap);
                else
                    SetComplexEnumerable(objMap, propertyName, subObjMap);
            }
        }

        protected List<CmdParameter> ParseArguments(List<string> arguments)
        {
            var list = (from arg in arguments
                        where arg.StartsWith("/") && !arg.StartsWith("/t:") && !arg.StartsWith("/j:")
                        let index = arg.IndexOf(":")
                        where index > 0 && arg.Length > index
                        select new CmdParameter { Properties = arg.Substring(1, index - 1).Split('.'), Value = arg.Substring(index + 1).TrimMatchingQuotes('\"') }).ToList();
            list.Sort();
            return list;
        }

        protected string ParseParameter(string parameterString, string key)
        {
            return parameterString.Remove(0, key.Length).Trim('"');
        }

        protected string ParseParameter(List<string> parameters, string key)
        {
            var parameterString = parameters.Find(s => s.StartsWith(key));

            return string.IsNullOrEmpty(parameterString) ? parameterString : ParseParameter(parameterString, key);
        }

        protected Type ParseObjectType(List<string> arguments)
        {
            var objectTypeStr = ParseParameter(arguments, "/t:");
            if (!string.IsNullOrEmpty(objectTypeStr) && !RegisteredTypes.ContainsKey(objectTypeStr))
                throw new TypeNotRegisteredException(objectTypeStr);
            return !string.IsNullOrEmpty(objectTypeStr) ? RegisteredTypes[objectTypeStr] : typeof(ExpandoObject);
        }
    }
}
