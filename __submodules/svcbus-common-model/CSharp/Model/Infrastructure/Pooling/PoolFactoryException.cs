using System;

namespace Sovos.SvcBus.Common.Model.Infrastructure.Pooling
{
#if !NETCORE
    [Serializable]
#endif
    public class PoolFactoryException : Exception
    {
        public PoolFactoryException() : base("Pool Factory cannot be null.") { }
    }
}
