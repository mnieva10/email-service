using System;
using System.Configuration;
using Sovos.SvcBus.Common.Model.Capability;
using Sovos.SvcBus.Common.Model.Exceptions;
using Sovos.SvcBus.Common.Model.Infrastructure.Logging;
using Sovos.SvcBus.Common.Model.Operation;
using Sovos.SvcBus.Common.Model.Services.Interfaces;

namespace Sovos.SvcBus.Common.Model.Services
{
    public class RemoteLoggingService : IRemoteLoggingService
    {
        private readonly IRemoteMicroservice _loggingRemoteMicroservice;

        public RemoteLoggingService(IRemoteMicroservice remoteMicroservice)
        {
            _loggingRemoteMicroservice = remoteMicroservice;
        }
        
        public RemoteLoggingService(PipeService ps, Service svc, string consumerName, Scope scope, ILogger logger)
        {
            _loggingRemoteMicroservice = new RemoteMicroservice(ps, svc, logger, consumerName);
            scope.add((IDisposable)_loggingRemoteMicroservice);
        }

        public bool AddLogEntry(User user, string description, LogCodes logCode)
        {
            if (user==null)
                throw new InvalidParametersException();

            var appName = ConfigurationManager.AppSettings["ServiceName"];
            
            var logEntry = new LogEntry
            {
                Schema = user.Schema,
                AppName = appName,
                Description = description,
                Domain = user.Domain,
                IPAddress = user.IPAddress,
                Username = string.IsNullOrEmpty(user.Username) ? user.LiteUsername : user.Username,
                LogCode = (int)logCode
            };

            var response = (DataDto)_loggingRemoteMicroservice.SendCommand("AddLogEntry", logEntry);

            return response.DataBool;
        }

        public int FindMostRecentLogEntryByLogCode(User user, LogCodes logCode)
        {
            if (user == null)
                throw new InvalidParametersException();

            var logEntry = new LogEntry
            {
                Schema = user.Schema,
                Username = user.Username,
                LogCode = (int)logCode
            };

            var response = (DataDto)_loggingRemoteMicroservice.SendCommand("FindMostRecentLogEntryByLogCode", logEntry);

            return response.DataInt;
        }

        public LogCodes? FindMostRecentPasswordResetTokenLogCode(User user)
        {
            if (user == null)
                throw new InvalidParametersException();

            var logEntry = new LogEntry
            {
                Schema = user.Schema,
                Username = user.Username
            };

            var response = (DataDto)_loggingRemoteMicroservice.SendCommand("FindMostRecentPasswordResetTokenLogCode", logEntry);

            return (LogCodes)response.DataInt;
        }
    }
}
