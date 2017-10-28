using System;

namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class LogEntry
    {
        [SvcBusSerializable]
        public string Schema { get; set; }
        [SvcBusSerializable]
        public string Username { get; set; }
        [SvcBusSerializable]
        public int LogCode { get; set; }
        [SvcBusSerializable]
        public DateTime LogDate { get; set; }
        [SvcBusSerializable]
        public string ComputerName { get; set; }
        [SvcBusSerializable]
        public string NetworkId { get; set; }
        [SvcBusSerializable]
        public string Domain { get; set; }
        [SvcBusSerializable]
        public string IPAddress { get; set; }
        [SvcBusSerializable]
        public string Description { get; set; }
        [SvcBusSerializable]
        public string LogId { get; set; }
        [SvcBusSerializable]
        public int EventLength { get; set; }
        [SvcBusSerializable]
        public string AppName { get; set; }
        [SvcBusSerializable]
        public string SessionId { get; set; }
    }
}
