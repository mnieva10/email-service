using System;

namespace Sovos.SvcBus.Common.Model.Operation
{
    public class AcaXmitPause
    {
        public string Command { get; set; }

        public DateTime ResumeOnDateTime { get; set; }

        public string Username { get; set; }
    }
}