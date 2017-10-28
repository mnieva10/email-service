using System.Collections.Generic;
using System.IO;
using Sovos.SvcBus.Common.Model.LongRunningJob;

namespace Sovos.SvcBus.Common.Model.Capability
{
    [SvcBusBuildable]
    public class WorkflowRequest : LrjRequest
    {
        [SvcBusSerializable]
        public string FilePath { get; set; }

        [SvcBusSerializable]
        public string WorkflowName { get; set; }

        [SvcBusSerializable]
        public string MapFileSettings { get; set; }

        [SvcBusSerializable]
        public string RequestorEmail { get; set; }

        [SvcBusSerializable]
        public string AuditPathExtension { get; set; }

        [SvcBusSerializable]
        public string Data { get; set; }

        [SvcBusSerializable]
        public string UrlExtension { get; set; }

        [SvcBusSerializable]
        public MemoryStream Stream { get; set; }

        [SvcBusSerializable]
        public Dictionary<object, object> SettingsTagMap { get; set; }

        public string CustomerWfName { get; set; }
        public string ConnectionString { get; set; }

        public WorkflowRequest()
        {
            SettingsTagMap = new Dictionary<object, object>();
        }
    }
}
