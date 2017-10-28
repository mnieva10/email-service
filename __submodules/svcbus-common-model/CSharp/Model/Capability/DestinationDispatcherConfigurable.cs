using System;
using System.Diagnostics;

namespace Sovos.SvcBus.Common.Model.Capability
{
#if !NETCORE
    [Serializable]
#endif
    [SvcBusBuildable]
    public class DestinationDispatcherConfigurable
    {
        [SvcBusSerializable]
        public string MachineName { get; set; }
        [SvcBusSerializable]
        public int Pid { get; set; }

        public bool MatchCurrent
        {
            get
            {
                if (!string.IsNullOrEmpty(MachineName) && MachineName != Current.MachineName)
                    return false;
                if (Pid != 0 && Pid != Current.Pid)
                    return false;
                return true;
            }
        }

        static DestinationDispatcherConfigurable()
        {
            var t = typeof (DestinationDispatcherConfigurable);
            BuildableSerializableTypeMapper.Mapper.Register(t.Name, t);
        }

        public static DestinationDispatcherConfigurable Current
        {
            get
            {
                return new DestinationDispatcherConfigurable
                {
                    MachineName = Environment.MachineName,
                    Pid = Process.GetCurrentProcess().Id
                };
            }
        }
    }
}
