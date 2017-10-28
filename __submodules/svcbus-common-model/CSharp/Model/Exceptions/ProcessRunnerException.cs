using System;

namespace Sovos.SvcBus.Common.Model.Exceptions
{
#if !NETCORE
    [Serializable]
#endif
    public class ProcessRunnerException : Exception
    {
        public ProcessRunnerException(string message) : base(message) {}
    }
}
