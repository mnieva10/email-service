using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Web.Script.Serialization;

namespace Sovos.SvcBus.Common.Model.Services
{
    public class TypeConverter
    {
        private JavaScriptTypeResolver Resolver { get; set; }

        public TypeConverter()
        {
        }

        public TypeConverter(JavaScriptTypeResolver resolver)
        {
            Resolver = resolver;
        }

        public object DeserializeJson(string jsonString, Type type)
        {
            return type != typeof (ExpandoObject) 
                ? new JavaScriptSerializer(Resolver).Deserialize(jsonString, type) 
                : ConvertDynamic((IDictionary<string, object>)new JavaScriptSerializer(Resolver).Deserialize<object>(jsonString));
        }

        public T ConvertDynamic<T>(IDictionary<string, object> dictionary)
        {
            var jsSerializer = new JavaScriptSerializer(Resolver);
            var obj = jsSerializer.ConvertToType<T>(dictionary);
            return obj;
        }

        public object ConvertDynamic(IDictionary<string, object> dictionary, Type targetType)
        {
            var jsSerializer = new JavaScriptSerializer(Resolver);
            var obj = jsSerializer.ConvertToType(dictionary, targetType);
            return obj;
        }

        public ExpandoObject ConvertDynamic(IDictionary<string, object> dictionary)
        {
            var eo = new ExpandoObject();
            var eoColl = (ICollection<KeyValuePair<string, object>>)eo;
            foreach (var kvp in dictionary)
                eoColl.Add(kvp);
            return eo;
        }
    }
}
