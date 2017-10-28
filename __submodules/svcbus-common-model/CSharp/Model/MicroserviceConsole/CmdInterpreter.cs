using System;
using System.Collections.Generic;
using System.Linq;
using Sovos.SvcBus.Common.Model.Extensions;
using Sovos.SvcBus.Common.Model.MicroserviceConsole.Exceptions;

namespace Sovos.SvcBus.Common.Model.MicroserviceConsole
{
    public interface ICmdInterpreter
    {
        object ProcessCommand(string command);
    }

    public class CmdInterpreter<T> : ICmdInterpreter where T : DispatchInterface
    {
        public MessageBuilder MessageBuilder { get; private set; }
        private readonly T _dispatchInterface;

        public CmdInterpreter(object userData, MessageBuilder messageBuilder)
        {
            MessageBuilder = messageBuilder ?? new MessageBuilder();
            try
            {
                _dispatchInterface = (T)Activator.CreateInstance(typeof(T), userData);
            }
            catch (Exception ex)
            {
                throw new DispatchInterfaceCreateInstanceException(ex.Message, typeof(T).FullName);
            }
        }

        public object ProcessCommand(string command)
        {
            var arguments = new List<string> { "Help" }; 
            if (!string.IsNullOrEmpty(command))
                arguments = command.SplitCommandLine().ToList();

            var mi = _dispatchInterface.GetType().GetMethod(arguments[0]);
            if (mi == null)
                throw new DispatchInterfaceMethodNotFoundException(arguments[0]);

            if (arguments.Count > 1)
            {
                if (MessageBuilder == null)
                    throw new MessageBuilderNullException();
                return mi.Invoke(_dispatchInterface, new object[] { MessageBuilder.Build(arguments) });
            }
            return mi.Invoke(_dispatchInterface, arguments[0] == "Help" ? new object[] { null } : null);
        }
    }
}
