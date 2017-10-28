using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime;
#if NETCORE
using Newtonsoft.Json;

namespace System.Web.Script.Serialization
 {
     public class JavaScriptSerializer
     {
        public JavaScriptSerializer() {}
        public JavaScriptSerializer(JavaScriptTypeResolver resolver) {}

        public object Deserialize(string input, Type targetType)
        {
            return JsonConvert.DeserializeObject(input, targetType);
        }

        public string Serialize(object obj)
        {
             return JsonConvert.SerializeObject(obj);
        }

        public T Deserialize<T>(string input)
        {
            return JsonConvert.DeserializeObject<T>(input);
        }

        public T ConvertToType<T>(object obj)
        {
            string intermediate = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(intermediate);
        }

        public object ConvertToType(object obj, Type targetType)
        {
            string intermediate = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject(intermediate, targetType);
        }
    }
 }

namespace System.Web.Script.Serialization
{
    public class SimpleTypeResolver: JavaScriptTypeResolver
    {
        public SimpleTypeResolver() {}
        public override Type ResolveType(string id)
        {
            return id.GetType();
        }
        public override string ResolveTypeId(Type type)
        {
            return "";
        }
    }

    public abstract class JavaScriptTypeResolver
    {
        protected JavaScriptTypeResolver() {}
        public abstract Type ResolveType(string id);
        public abstract string ResolveTypeId(Type type);
    }
}
namespace System.Security.AccessControl
{
    public sealed class FileSecurity
    {
        public FileSecurity() {}
        public FileSecurity(string fileName, AccessControlSections includeSections) {}
    }

    [Flags]
    public enum AccessControlSections
    {
        None = 0,
        Audit = 1,
        Access = 2,
        Owner = 4,
        Group = 8,
        All = 15
    }
}

namespace System.ServiceProcess
{
    public class ServiceBase
    {
        public string ServiceName { get; set; }
        public static void Run(ServiceBase[] services) { }
        protected virtual void OnStart(string[] args) { }
        protected virtual void OnStop() { }
        protected virtual void Dispose(bool disposing) { }
    }
}
#endif
