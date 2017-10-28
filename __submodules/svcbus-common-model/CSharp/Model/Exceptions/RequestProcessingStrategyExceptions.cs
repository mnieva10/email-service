using System;
using Sovos.SvcBus.Common.Model.Extensions;
using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.Exceptions
{
#if !NETCORE
    [Serializable]
#endif
    public class MutexInfoProviderNullException : SvcBusException
    {
        public MutexInfoProviderNullException()
            :base(RequestProcessingStrategyMessage.MutexInfoProviderNull.ToDescription(), (int)RequestProcessingStrategyMessage.MutexInfoProviderNull, "Mutex info provider is null") { }
    }

#if !NETCORE
    [Serializable]
#endif
    public class MessageNullException : SvcBusException
    {
        public MessageNullException(string msg)
            :base(RequestProcessingStrategyMessage.MessageNullException.ToDescription(), (int)RequestProcessingStrategyMessage.MessageNullException, msg) { }
    }
}