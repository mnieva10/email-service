using Sovos.SvcBus.Common.Model.Operation;

namespace Sovos.<%= namespace %>.Model.Repositories
{
    public interface ISecurityAnswerRepository
    {
        SecurityAnswer Find(SecurityAnswer securityAnswer);
        void Save(SecurityAnswer securityAnswer);
        void Delete(SecurityAnswer securityAnswer);
    }
}
