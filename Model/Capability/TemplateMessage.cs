using System.ComponentModel;

namespace Sovos.Template.Model.Capability
{
    public enum TemplateMessage
    {
        [Description("Success")]
        Success = 0,

        [Description("Invalid username")]
        InvalidUsername = 1,

        [Description("Invalid schema: Cannot be null or empty")]
        InvalidSchema = 2,

        [Description("Invalid Table Prefix: Cannot be null")]
        InvalidTablePrefix = 3,

        [Description("Template Service Failure: Repository factory not specified")]
        RepositoryFactory = 90,
    }
}
