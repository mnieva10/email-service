using System;
using Sovos.SvcBus.Common.Model.Services.Interfaces;

namespace Sovos.SvcBus.Common.Model.Services
{
    public sealed class RemoteMicroservice : IRemoteMicroservice, IDisposable
    {
        public ILogger Logger { get; set; }
        public const int DefaultResponseTimeout = 10000;

        private DispatchInterfaceConsumerPool _pool;

        public uint ResponseTimeout
        {
            get { return _pool.ResponseTimeout; }
            set { _pool.ResponseTimeout = value; }
        }

        public RemoteMicroservice(PipeService ps, Service service, ILogger logger, string consumerName = "AnonymousRemoteMicroservice")
        {
            Logger = logger;
            _pool = new DispatchInterfaceConsumerPool(ps, service, consumerName) {Logger = logger};
            ResponseTimeout = DefaultResponseTimeout;

            BuildableSerializableTypeMapper.dictionarySerializationMode = DictionarySerializationMode.ForceComplex;
        }

        public object SendBroadcast(string command, object args)
        {
            var dic = _pool.Acquire();
            try
            {
                return dic.SendBroadcast(command, args);
            }
            finally
            {
                _pool.Release(dic);
            }
        }

        public object SendCommand(string command, object args, bool wait = true)
        {
            var dic = _pool.Acquire();
            try
            {
                return wait ? dic.SendAndWait(command, args) : dic.Send(command, args);
            }
            finally
            {
                _pool.Release(dic);
            }
        }

        public void SendCommandAsync(string command, object args, Message sourceMessage, IReplier replier,
            Consumer.OnAsyncWait callback,
            object callbackArgs = null)
        {
            var dic = _pool.Acquire();
            try
            {
                dic.sendAndWaitResponseAsync(command, sourceMessage, args, replier,
                    callback, callbackArgs);
            }
            finally
            {
                _pool.Release(dic);
            }
        }

        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}