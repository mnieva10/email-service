using System;
using Sovos.SvcBus;
using Sovos.SvcBus.Common.Model.Extensions;
using Sovos.<%= namespace %>.Model.Capability;

namespace Sovos.<%= namespace %>.Model.Exceptions
{
    [Serializable]
    public class <%= namespace %>Exception : SvcBusException
    {
        public <%= namespace %>Exception(string msg, int code, string sourceMsg) : base(msg, code, sourceMsg) { }
    }

    [Serializable]
    public class RepositoryFactoryException : <%= namespace %>Exception
    {
        public RepositoryFactoryException() : base(<%= namespace %>Message.RepositoryFactory.ToDescription(), (int)<%= namespace %>Message.RepositoryFactory, string.Format("Repository Factory cannot be null.")) { }
    }

    [Serializable]
    public class SchemaException : <%= namespace %>Exception
    {
        public SchemaException()
            : base(<%= namespace %>Message.InvalidSchema.ToDescription(), (int)<%= namespace %>Message.InvalidSchema,
                string.Format("Schema cannot be null or empty.")) { }
    }

    [Serializable]
    public class TablePrefixException : <%= namespace %>Exception
    {
        public TablePrefixException()
            : base(<%= namespace %>Message.InvalidTablePrefix.ToDescription(), (int)<%= namespace %>Message.InvalidTablePrefix,
                string.Format("Table Prefix cannot be null.")) { }
    }

    [Serializable]
    public class InvalidUsernameException : <%= namespace %>Exception
    {
        public InvalidUsernameException(string username) : base(<%= namespace %>Message.InvalidUsername.ToDescription(), (int)<%= namespace %>Message.InvalidUsername, string.Format("Username '{0}' was not found.", username)) { }
    }
}
