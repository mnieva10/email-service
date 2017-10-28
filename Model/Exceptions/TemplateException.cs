using System;
using Sovos.SvcBus;
using Sovos.SvcBus.Common.Model.Extensions;
using Sovos.Template.Model.Capability;

namespace Sovos.Template.Model.Exceptions
{
    [Serializable]
    public class TemplateException : SvcBusException
    {
        public TemplateException(string msg, int code, string sourceMsg) : base(msg, code, sourceMsg) { }
    }

    [Serializable]
    public class RepositoryFactoryException : TemplateException
    {
        public RepositoryFactoryException() : base(TemplateMessage.RepositoryFactory.ToDescription(), (int)TemplateMessage.RepositoryFactory, string.Format("Repository Factory cannot be null.")) { }
    }

    [Serializable]
    public class SchemaException : TemplateException
    {
        public SchemaException()
            : base(TemplateMessage.InvalidSchema.ToDescription(), (int)TemplateMessage.InvalidSchema,
                string.Format("Schema cannot be null or empty.")) { }
    }

    [Serializable]
    public class TablePrefixException : TemplateException
    {
        public TablePrefixException()
            : base(TemplateMessage.InvalidTablePrefix.ToDescription(), (int)TemplateMessage.InvalidTablePrefix,
                string.Format("Table Prefix cannot be null.")) { }
    }

    [Serializable]
    public class InvalidUsernameException : TemplateException
    {
        public InvalidUsernameException(string username) : base(TemplateMessage.InvalidUsername.ToDescription(), (int)TemplateMessage.InvalidUsername, string.Format("Username '{0}' was not found.", username)) { }
    }
}
