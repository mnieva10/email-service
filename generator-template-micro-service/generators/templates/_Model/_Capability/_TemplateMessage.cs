using System.ComponentModel;

namespace Sovos.<%= namespace %>.Model.Capability
{
    public enum <%= namespace %>Message
    {
        [Description("Success")]
        Success = 0,

        [Description("Invalid username")]
        InvalidUsername = 1,

        [Description("Invalid schema: Cannot be null or empty")]
        InvalidSchema = 2,

        [Description("Invalid Table Prefix: Cannot be null")]
        InvalidTablePrefix = 3,

        [Description("<%= namespace %> Service Failure: Repository factory not specified")]
        RepositoryFactory = 90,
    }
}
