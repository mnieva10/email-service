using System;

namespace Sovos.SvcBus.Common.Model.MicroserviceConsole.Capability
{
    public class CmdParameter : IComparable<CmdParameter>
    {
        public string[] Properties { get; set; }
        public string Value { get; set; }

        public int CompareTo(CmdParameter other)
        {
            return Properties.Length.CompareTo(other.Properties.Length);
        }
    }
}
