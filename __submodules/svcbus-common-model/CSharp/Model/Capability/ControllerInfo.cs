using System;

namespace Sovos.SvcBus.Common.Model.Capability
{
#if !NETCORE
    [Serializable]
#endif
    public class ControllerInfo : DestinationDispatcherConfigurable
    {
        static ControllerInfo()
        {
            var t = typeof(ControllerInfo);
            BuildableSerializableTypeMapper.Mapper.Register(t.Name, t);
        }
    }
}
