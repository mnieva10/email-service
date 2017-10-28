using System.ComponentModel;
namespace Sovos.SvcBus.Common.Model.Capability
{
    public enum RequestProcessingStrategyMessage
    {
        [Description("Success")]
        Success = 0,

        [Description("mutex info provider cannot be null")]
        MutexInfoProviderNull = 1,

        [Description("message is null")]
        MessageNullException = 2
    }
}