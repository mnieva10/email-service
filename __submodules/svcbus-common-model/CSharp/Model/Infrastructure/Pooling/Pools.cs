using System;
using System.Collections.Generic;

namespace Sovos.SvcBus.Common.Model.Infrastructure.Pooling
{
    public class Pools<T> : IDisposable 
    {
        public const long DefaultPoolTtl = 60*10000000; //sec

        public Dictionary<string, Pool<T>> PoolMap { get; set; }
 
        private readonly Func<object[], T> _factory;
        private bool _isDisposed;
        private readonly long _ttl;

        public Pools(Func<object[], T> factory)
        {
            if (factory == null)
                throw new PoolFactoryException();

            _factory = factory;
            PoolMap = new Dictionary<string, Pool<T>>();
            _isDisposed = false;
            _ttl = DefaultPoolTtl;
        }

        public Pools(Func<object[], T> factory, long ttl) : this(factory)
        {
            _ttl = ttl;
        }

        public Pool<T> Acquire(string poolName, long? ttl = null)
        {
            lock (PoolMap)
            {
                if (!PoolMap.ContainsKey(poolName))
                    PoolMap.Add(poolName, new Pool<T>(_factory, ttl ?? _ttl));

                return PoolMap[poolName];
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
                foreach (var pool in PoolMap)
                    pool.Value.Dispose();
            _isDisposed = true;
        }
        
        ~Pools()
        {
            Dispose(false);
        }
    }
}