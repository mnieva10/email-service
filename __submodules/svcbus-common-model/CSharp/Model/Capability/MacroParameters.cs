using System;
using System.Collections.Generic;

namespace Sovos.SvcBus.Common.Model.Capability
{
    [SvcBusBuildable]
    public class MacroParameters : IComparable<MacroParameters>
    {
        public int Priority { get; set; }

        [SvcBusSerializable]
        public Dictionary<string, string> Defs { get; set; }

        [SvcBusSerializable]
        public Dictionary<string, string> Vars { get; set; }

        public MacroParameters()
        {
            Priority = 0;
            Defs = new Dictionary<string, string>();
            Vars = new Dictionary<string, string>();
        }

        public int CompareTo(MacroParameters other)
        {
            return other == null ? 1 : Priority.CompareTo(other.Priority);
        }
    }
}
