using System;

namespace Sovos.SvcBus.Common.Model.MicroserviceConsole.Exceptions
{
#if !NETCORE
    [Serializable]
#endif
    public class MessageBuilderException : Exception
    {
        public MessageBuilderException(string message) : base(message) { }
    }

#if !NETCORE
    [Serializable]
#endif
    public class MessageBuilderNullException : MessageBuilderException
    {
        public MessageBuilderNullException() : base("Message Builder cannot be null.") { }
    }

#if !NETCORE
    [Serializable]
#endif
    public class TypeNotFoundException : MessageBuilderException
    {
        public TypeNotFoundException() : base("Message type parameter </t:object type> was not found.") { }
    }

#if !NETCORE
    [Serializable]
#endif
    public class TypeNotRegisteredException : MessageBuilderException
    {
        public TypeNotRegisteredException(string type) : base(string.Format("Type '{0}' is not registered.", type)) { }
    }
}
