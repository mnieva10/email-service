using System;
using System.Collections.Generic;
using System.IO;
using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class AcaXmit
    {
        [SvcBusSerializable]
        public int Id { get; set; }
        [SvcBusSerializable]
        public int LogLevel { get; set; }
        [SvcBusSerializable]
        public string Environment { get; set; }
        [SvcBusSerializable]
        public AcaXmitStatus Status { get; set; }
        [SvcBusSerializable]
        public int StatusInt
        {
            get { return (int) Status; }
            set { }
        }

        [SvcBusSerializable]
        public int? IrsResponse { get; set; }

        public object CustomIrsResponse
        {
            get
            {
                if (CustomIrsResponseConversionMethod != null)
                    return CustomIrsResponseConversionMethod(IrsResponse);
                return null;
            }
        }

        public static Func<int?, object> CustomIrsResponseConversionMethod { get; set; }

        [SvcBusSerializable]
        public string Prefix { get; set; }
        [SvcBusSerializable]
        public string SegHash { get; set; }
        [SvcBusSerializable]
        public int TaxYear { get; set; }
        [SvcBusSerializable]
        public string TransmittalFileName { get; set; }
        [SvcBusSerializable]
        public string ManifestFileName { get; set; }
        [SvcBusSerializable]
        public int? XmitRun { get; set; }
        [SvcBusSerializable]
        public string Receipt { get; set; }
        [SvcBusSerializable]
        public string EmailAddress { get; set; }
        [SvcBusSerializable]
        public DateTime SubmitStartDate { get; set; }
        [SvcBusSerializable]
        public DateTime? SubmittedOn { get; set; }
        [SvcBusSerializable]
        public DateTime? RetrievedOn { get; set; }
        [SvcBusSerializable]
        public DateTime CreatedOn { get; set; }
        [SvcBusSerializable]
        public DateTime ModifiedOn { get; set; }
        [SvcBusSerializable]
        public int SubmitAttempts { get; set; }
        [SvcBusSerializable]
        public int RetrieveAttempts { get; set; }

        [SvcBusSerializable]
        public bool IsIrsResponseFileReceived { get; set; }

        private string _responseFileName;
        [SvcBusSerializable]
        public string ResponseFileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_responseFileName))
                {
                    var ext = Path.GetExtension(TransmittalFileName);
                    _responseFileName = !string.IsNullOrEmpty(ext)
                        ? TransmittalFileName.Replace(ext, "_RESPONSE" + ext)
                        : TransmittalFileName + "_RESPONSE";
                }
                return _responseFileName;
            }
            set { _responseFileName = value; }
        }

        [SvcBusSerializable]
        public List<AcaXmitLogEntry> LogEntries { get; set; }

        public DateTime? LockDate { get; set; }
        public long RecordVersion { get; set; }

        public AcaXmit()
        {
            Status = AcaXmitStatus.PreQueued;
            LogEntries = new List<AcaXmitLogEntry>();
        }

        public AcaXmit(int id, string environment, string prefix, string segHash, int taxYear, int? status,
                    string transmittalFileName, string manifestFileName, int? xmitRun, DateTime submitStartDate) 
        {
            Id = id;
            Environment = environment;
            Prefix = prefix;
            SegHash = segHash;
            TaxYear = taxYear;
            TransmittalFileName = transmittalFileName;
            ManifestFileName = manifestFileName;
            XmitRun = xmitRun;
            SubmitStartDate = submitStartDate;
            if (status != null)
                Status = (AcaXmitStatus)status;
            LogEntries = new List<AcaXmitLogEntry>();
        }
    }
}
