using System;
using System.Collections.Generic;

namespace Sovos.SvcBus.Common.Model.Capability
{
    [SvcBusBuildable]
    public class DataDto : DestinationDispatcherConfigurable
    {
        [SvcBusSerializable]
        public string Domain { get; set; }
        [SvcBusSerializable]
        public string Schema { get; set; }
        [SvcBusSerializable]
        public string TablePrefix { get; set; }

        [SvcBusSerializable]
        public string DataString { get; set; }
        [SvcBusSerializable]
        public bool DataBool { get; set; }
        [SvcBusSerializable]
        public int DataInt { get; set; }
        [SvcBusSerializable]
        public object DataObject { get; set; }

        [SvcBusSerializable]
        public DateTime DataDateTime { get; set; }

        [SvcBusSerializable]
        public List<object> DataList { get; set; }
        [SvcBusSerializable]
        public Dictionary<string, object> DataMap { get; set; }

        public DataDto()
        {
            DataList = new List<object>();
            DataMap = new Dictionary<string, object>();
        }
    }
}
