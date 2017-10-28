using Sovos.SvcBus.Common.Model.Capability;
using Sovos.SvcBus.Common.Model.Infrastructure.Logging;
using Sovos.SvcBus.Common.Model.Operation;

namespace Sovos.SvcBus.Common.Model.Repositories
{
    public interface ILogRepository
    {
        void Save(User user, LogCodes logCode);
        void Save(User user, SecurityAnswer securityAnswer, LogCodes logCode);
        void Save(User user, LogCodes logCode, SvcBusException ex);
        void Save(User user, SecurityQuestion securityQuestion, LogCodes logCode);
        void Save(LogEntry entry);
        int FindMostRecentSecurityQuestionId(User user);
        int FindEntryCountByUsername(User user);
        void DeleteAllByUsername(User user);
        LogCodes? FindMostRecentTokenLogCode(User user);
    }
}
