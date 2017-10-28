using System;
using Sovos.SvcBus.Common.Model.MicroserviceConsole.Exceptions;

namespace Sovos.SvcBus.Common.Model.Exceptions
{
#if !NETCORE
    [Serializable]
#endif
    public class FileSystemServiceException : Exception
    {
        public FileSystemServiceException(string message) : base(message) { }
    }

#if !NETCORE
    [Serializable]
#endif
    public class FileAlreadyExistsException : MessageBuilderException
    {
        public FileAlreadyExistsException(string file) : base(string.Format("The File '{0}' already exists.", file)) { }
    }
}
