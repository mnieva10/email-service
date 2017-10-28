using Sovos.SvcBus.Common.Model.Operation;
using Sovos.SvcBus.Common.Model.Repositories;

namespace Sovos.SvcBus.Common.Model.Strategies
{
    public class PersistRequestStrategy : ChainableRequestProcessingStrategy
    {
        private IRequestMessageRepository RequestMessageRepository { get; set; }
        
        public PersistRequestStrategy(IRequestMessageRepository messageRepository, ChainableRequestProcessingStrategy next)
            : base(next)
        {
            RequestMessageRepository = messageRepository;
        }

        public PersistRequestStrategy(IRequestMessageRepository repository)
        {
            RequestMessageRepository = repository;
        }

        public override void Process(Message msg)
        {
            RequestMessageRepository.AddMessage(msg);
            if (Next != null)
                Next.Process(msg);
        }
    }
}
