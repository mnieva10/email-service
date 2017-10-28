using System;
using System.Linq;

namespace Sovos.SvcBus.Common.Model.Infrastructure.Pooling
{
    public class Pool<T> : IDisposable
    {
        private readonly Func<object[], T> _factory;
        private readonly long? _ttl;
        private bool _isDisposed;

        public ItemStore<T> ItemStore { get; private set; }

        public Pool(Func<object[], T> factory, long? ttl)
        {
            if (factory == null)
                throw new PoolFactoryException();

            _factory = factory;
            ItemStore = new ItemStore<T>();
            _ttl = ttl ?? 0;
        }

        public T Acquire(object[] parameters = null)
        {
            RunTtlCheck();

            lock (ItemStore)
                if (ItemStore.Count > 0)
                    return ItemStore.Fetch();

            return _factory(parameters);
        }

        public void Release(T item)
        {
            lock (ItemStore)
                ItemStore.Store(item);
        }

        public void Purge(bool purgeAll)
        {
            var curTicks = DateTime.Now.Ticks;
            lock (ItemStore)
                if (purgeAll)
                    while (ItemStore.Count > 0)
                        ItemStore.RemoveFirst();
                else if (_ttl != 0)
                    foreach (var item in ItemStore.Where(item => Math.Abs(curTicks - item.LastUsed) > _ttl).ToList())
                        ItemStore.Remove(item);
        }

        public void RunTtlCheck()
        {
            Purge(false);
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
                if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
                    lock (ItemStore)
                        foreach (var item in ItemStore)
                            ((IDisposable)item.Object).Dispose();

            _isDisposed = true;
        }

        ~Pool()
        {
            Dispose(false);
        }
    }
}