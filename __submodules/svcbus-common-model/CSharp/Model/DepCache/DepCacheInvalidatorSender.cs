using System;

namespace Sovos.SvcBus.Common.Model.Services
{

    /// <summary>Implements a simple way to send invalidation messages(with dependency strings) over svcbus to the TDepCacheInvalidatorListener.</summary>
    public class DepCacheInvalidatorSender : IDisposable
    {
        private readonly Scope _scope = new Scope();
        private readonly Consumer _consumer;

        public DepCacheInvalidatorSender(PipeService ps)
        {
            _consumer = _scope.newConsumer(ps, "DepCacheInvalidatorSender", Builder.newService(DepInvalidatorConstants.SERVICE_NAME, svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE));
        }

        ~DepCacheInvalidatorSender()
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
            if (disposing)
                _scope.Dispose();
        }

        /// <summary>Send a request to the listener to invalidate dep-cache objects with the supplied dependencies.</summary>
        public void RequestInvalidation(string[] dependencies)
        {
          var msg = Builder.newMessage();
          var arr = msg.bson.appendArrayBegin("dependencies");
          foreach (var dep in dependencies)
            arr.append("", dep);
          msg.bson.appendArrayEnd(arr);
          _consumer.send(msg);
        }

        /// <summary>Send a request to the listener to invalidate dep-cache objects with the supplied dependencies.</summary>
        public void RequestInvalidation(string dependency)
        {
            RequestInvalidation(new[] { dependency });
        }
    }

}