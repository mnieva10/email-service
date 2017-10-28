using System;
using System.IO;
using Sovos.SvcBus.Common.Model.Extensions;
using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class SettingsData : IDisposable
    {
        private string _schema;

        [SvcBusSerializable]
        public string SegHash { get; set; }

        [SvcBusSerializable]
        public string Name { get; set; }

        [SvcBusSerializable]
        public string FullName { get; set; }

        [SvcBusSerializable]
        public MemoryStream DataStream { get; set; }

        [SvcBusSerializable]
        public string DataString { get; set; }

        [SvcBusSerializable]
        public string Schema
        {
            get { return _schema; }
            set { _schema = value == null ? null : value.ToUpper(); }
        }

        [SvcBusSerializable]
        public bool IsShared { get; set; }

        [SvcBusSerializable]
        public string ProfileName { get; set; }

        public string Priority { get; set; }

        public bool IsBlob { get; set; }
        public uint BlobId { get; set; }

        public SettingsData()
        {
            DataStream = new MemoryStream();
        }

        public SettingsData(string isShared, string paramStr, string paramIsBlob)
            : this()
        {
            IsShared = (isShared == Constants.OracleTrue);

            IsBlob = (paramIsBlob != Constants.OracleFalse);

            if (!IsBlob)
                DataStream = paramStr.ToMemoryStream();
            else
                BlobId = uint.Parse(paramStr);
        }

        public void Dispose()
        {
            if (DataStream != null)
                DataStream.Dispose();
        }
    }
}
