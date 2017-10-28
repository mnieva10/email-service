using System;
using System.Collections.Generic;
using System.Threading;
using Sovos.DependencyCache;

namespace Sovos.SvcBus.Common.Model.Services
{
    public static class DepInvalidatorConstants
    {
        public const string SERVICE_NAME = "dep_cache_invalidator_listener";
    }

    /// <summary>A class that keeps a list of TDepCache objects and listens for messages over svcbus. The messages contain a
    /// list of dependency strings. When messages are received the class invalidates the objects inside the caches that have
    /// such dependencies.</summary>
    public class DepCacheInvalidatorListener : IDisposable
    {
        private readonly Object _locker = new Object();
        private readonly Scope _scope = new Scope();
        private readonly Producer _producer;
        private readonly List<DepCacheBase> _caches = new List<DepCacheBase>();
        private readonly Thread _listenerThread;
        private readonly ManualResetEvent _pauseEvent = new ManualResetEvent(false);
        public Exception FatalException;
        private volatile bool _threadRunning;

        public DepCacheInvalidatorListener(PipeService ps)
        {
            _producer = _scope.newProducer(ps, "DepCacheInvalidatorListener", Builder.newService(DepInvalidatorConstants.SERVICE_NAME, svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE));
            _producer.RequestTimeout = 1000;
            _listenerThread = new Thread(ListenForMessages);
            _listenerThread.Start();
            _threadRunning = true;
        }

        ~DepCacheInvalidatorListener()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _threadRunning = false;
            try
            {
                _pauseEvent.Set();
                _listenerThread.Join();
            }
            finally
            {
                if (disposing)
                    _scope.Dispose();
            }
        }

        /// <summary>Add a dependency cache object to the list that will be invalidated when a message is received.</summary>
        public void Add(DepCacheBase cache)
        {
            lock (_locker) {
                if (_caches.Contains(cache))
                    throw new Exception("The same dependency cache object is already added to the invalidator");
                _caches.Add(cache);
                if (_caches.Count == 1)
                    _pauseEvent.Set();
            }
        }

        /// <summary>Remove a dependency cache object from the list that will be invalidated when a message is received.</summary>
        public void Remove(DepCacheBase cache)
        {
            lock (_locker) {
                _caches.Remove(cache);
                if (_caches.Count == 0)
                    _pauseEvent.Reset();
            }
        }

        private void ListenForMessages()
        {
            while (_threadRunning)
                try
                {
                    _pauseEvent.WaitOne();
                    if (!_threadRunning)
                        return;

                    Message msg = _producer.waitAndTake();
                    Iterator it = msg.bson.find("dependencies");
                    if (it == null)
                      continue;

                    lock (_locker) {
                        var deps = new _Iterator(it);
                        while (deps.next())
                            foreach (var cache in _caches)
                                cache.RemoveByDep((string) deps);
                    }
                }
                catch (ProducerException e)
                {
                    if (e.ErrorCode != svc_bus_producer_err_t.SERVICE_BUS_PRODUCER_WAIT_EXCEEDED)
                    {
                        FatalException = e;
                        break;
                    }
                }
                catch (Exception e)
                {
                    FatalException = e;
                    break;
                }
        }
    }

}