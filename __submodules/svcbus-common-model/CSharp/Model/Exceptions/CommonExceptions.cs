using System;
using Sovos.SvcBus.Common.Model.Extensions;
using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.Exceptions
{
#if !NETCORE
    [Serializable]
#endif
    public class InvalidConnectionStringException : SvcBusException
    {
        public InvalidConnectionStringException(string msg, int code, string sourceMsg) : base(msg, code, sourceMsg) { }

        public InvalidConnectionStringException()
            : base(CommonMessage.InvalidConnectionString.ToDescription(), (int)CommonMessage.InvalidConnectionString,
                "Repository Factory cannot be null.") { }
    }

#if !NETCORE
    [Serializable]
#endif
    public class InvalidParametersException : SvcBusException
    {
        public InvalidParametersException(string msg, int code, string sourceMsg) : base(msg, code, sourceMsg) { }

        public InvalidParametersException()
            : base(CommonMessage.InvalidParameters.ToDescription(), (int)CommonMessage.InvalidParameters,
                "Required parameters cannot be null.") { }
    }
}
