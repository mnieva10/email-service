using Sovos.SvcBus.Common.Model.Operation;

namespace Sovos.SvcBus.Common.Model.Repositories
{
    public interface IRequestMessageRepository
    {
        void AddMessage(Message message);
    }
}
