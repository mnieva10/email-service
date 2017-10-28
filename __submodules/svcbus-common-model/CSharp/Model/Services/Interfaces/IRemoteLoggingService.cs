using Sovos.SvcBus.Common.Model.Operation;
using Sovos.SvcBus.Common.Model.Infrastructure.Logging;

namespace Sovos.SvcBus.Common.Model.Services.Interfaces
{
    public interface IRemoteLoggingService
    {
        bool AddLogEntry(User user, string description, LogCodes logCode);
        int FindMostRecentLogEntryByLogCode(User user, LogCodes logCode);
        LogCodes? FindMostRecentPasswordResetTokenLogCode(User user);
    }
}