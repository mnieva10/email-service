using System.ComponentModel;

namespace Sovos.SvcBus.Common.Model.Capability
{
    public enum CommonMessage
    {
        [Description("Success")]
        Success = 0,

        [Description("Connection string is null or has bad format")]
        InvalidConnectionString = 1000,

        [Description("Some required parameters are null")]
        InvalidParameters = 1001
    }
}
