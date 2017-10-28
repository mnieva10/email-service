using System.Collections.Concurrent;
using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.Exceptions;

namespace Sovos.SvcBus.Common.Model.Strategies
{
    public interface IMutexInfoProvider
    {
        string GetMutexName(Message chainMessage);
        string GetServiceName(Message chainMessage);
    }

    public class MutexedRequestProcessingStrategy : ChainableRequestProcessingStrategy
    {
        private PipeService _ps;
        private IMutexInfoProvider _mutexInfoProvider;
        private ConcurrentDictionary<string, Mutex> _messagesToMutexes;
        private List<Message> _onHoldMessages;

        public MutexedRequestProcessingStrategy(PipeService ps, IMutexInfoProvider mutexInfoProvider,
            ChainableRequestProcessingStrategy next)
            :base(next)
        {
            if (ps == null)
                throw new PipeServiceIsNullException();

            if (mutexInfoProvider == null)
                throw new MutexInfoProviderNullException();

            _ps = ps;
            _mutexInfoProvider = mutexInfoProvider;
            
            _messagesToMutexes = new ConcurrentDictionary<string, Mutex>();
            _onHoldMessages = new List<Message>();
        }

        ~MutexedRequestProcessingStrategy()
        {
            foreach (var curKVP in _messagesToMutexes)
                curKVP.Value.Dispose();
        }

        private bool AcquireMutex(Message chainMsg, string mutexName, string svcName)
        {
            var mutex = Builder.newMutex(_ps, mutexName, svcName);
            _messagesToMutexes.TryAdd(chainMsg._id.ToString(), mutex);
            return mutex.acquire();
        }

        private Mutex GetMutex(Message msg)
        {
            var id = msg._id.ToString();
            if (_messagesToMutexes.ContainsKey(id))
                return _messagesToMutexes[id];
            return null;
        }

        public override Message GetMessage()
        {
            Message retVal;
            foreach (var curMsg in _onHoldMessages)
            {
                retVal = curMsg;
                var mutex = GetMutex(retVal);

                if (curMsg == null)
                    throw new MessageNullException("found a null message in _onHoldMessages");

                if (mutex.acquire())
                {
                    _onHoldMessages.Remove(curMsg);
                    return retVal;
                }
            }

            retVal = base.GetMessage();
            
            return retVal;
        }

        public override void RequestComplete(Message msg)
        {
            var mutex = GetMutex(msg);
            if (mutex != null)
            {
                Mutex removedMutex;
                if(_messagesToMutexes.TryRemove(msg._id.ToString(), out removedMutex))
                {
                    mutex.release();
                    mutex.Dispose();
                }
            }
        }

        public override bool TakeMessage(Message msg)
        {
            var mutex = GetMutex(msg);
            if (mutex != null)
                return true;

            if (!base.TakeMessage(msg))
                return false;

            var mutexName = _mutexInfoProvider.GetMutexName(msg);
            var svcName = _mutexInfoProvider.GetServiceName(msg);

            if (!string.IsNullOrEmpty(mutexName) && !string.IsNullOrEmpty(svcName) &&
                !AcquireMutex(msg, mutexName, svcName))
            {
                _onHoldMessages.Add(msg);
                return false;
            }

            return true;
        }
    }
}
