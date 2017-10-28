using System;
using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class AcaXmitLogEntry
    {
        [SvcBusSerializable]
        public int Id { get; set; }
        [SvcBusSerializable]
        public int LogLevel { get; set; }
        [SvcBusSerializable]
        public AcaXmitLogCodes LogCode { get; set; }
        [SvcBusSerializable]
        public DateTime StartedOn { get; set; }
        [SvcBusSerializable]
        public string Machine { get; set; }
        [SvcBusSerializable]
        public string Message { get; set; }

        public AcaXmitLogEntry() { }

        public AcaXmitLogEntry(int id, int logLevel, int logCode, DateTime startedOn, string machine, string message)
        {
            Id = id;
            LogLevel = logLevel;
            LogCode = (AcaXmitLogCodes)logCode;
            StartedOn = startedOn;
            Machine = machine;
            Message = message;
        }
    }
}
