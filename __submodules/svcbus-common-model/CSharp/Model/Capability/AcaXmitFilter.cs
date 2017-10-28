using System;

namespace Sovos.SvcBus.Common.Model.Capability
{
    public class AcaXmitFilter
    {
        public string FileName { get; set; }
        public string Environment { get; set; }
        public int? Status { get; set; }
        public string Prefix { get; set; }
        public DateTime? SubmittedOnMin { get; set; }
        public DateTime? SubmittedOnMax { get; set; }
        public DateTime? RetrievedOnMin { get; set; }
        public DateTime? RetrievedOnMax { get; set; }
    }
}
