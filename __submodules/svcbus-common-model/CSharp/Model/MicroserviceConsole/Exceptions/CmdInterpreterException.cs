using System;

namespace Sovos.SvcBus.Common.Model.MicroserviceConsole.Exceptions
{
#if !NETCORE
    [Serializable]
#endif
    public class CmdInterpreterException : Exception
    {
        public CmdInterpreterException(string message) : base(message) {}
    }
#if !NETCORE
    [Serializable]
#endif
    public class DispatchInterfaceCreateInstanceException : CmdInterpreterException
    {
        public DispatchInterfaceCreateInstanceException(string message, string diType) : base(string.Format("Error instantiating Dispatch Interface of type {1}: {0}", message, diType)) { }
    }

#if !NETCORE
    [Serializable]
#endif
    public class DispatchInterfaceMethodNotFoundException : CmdInterpreterException
    {
        public DispatchInterfaceMethodNotFoundException(string method) : base(string.Format("Method '{0}' was not found in DispatchInterface.", method)) { }
    }
}