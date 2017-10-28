using System.IO;

namespace Sovos.SvcBus.Common.Model.Capability
{
    [SvcBusBuildable]
    public class BaseFileInfo
    {
        private string _fileName;
        private string _rfmFileName;

        [SvcBusSerializable]
        public Operation.Environment Environment { get; set; }

        /// <summary>
        /// Optional, for RFM logging differentiation
        /// </summary>
        [SvcBusSerializable]
        public string Username { get; set; }

        /// <summary>
        /// Optional, for RFM UTC mode explicit setting
        /// </summary>
        [SvcBusSerializable]
        public bool? IsUtc { get; set; }

        [SvcBusSerializable]
        public string FilePath { get; set; }

        [SvcBusSerializable]
        public string FileName
        {
            set { _fileName = value; }
            get { return (_fileName ?? Path.GetFileName(FilePath != null ? FilePath.Trim() : FilePath)) ?? string.Empty; }
        }

        [SvcBusSerializable]
        public byte[] FileBytes { get; set; }

        public string RfmFileName
        {
            set { _rfmFileName = value; }
            get { return _rfmFileName ?? string.Format(@"$:\{0}", FileName); }
        }
    }
}
