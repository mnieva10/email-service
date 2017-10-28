using Sovos.SvcBus.Common.Model.Infrastructure.Logging;

namespace Sovos.SvcBus.Common.Model.Strategies
{
    public class FileLogResponseStrategy : IResponseProcessingStrategy
    {
        public FileLogger Logger { get; private set; }

        public FileLogResponseStrategy(FileLogger logger)
        {
            Logger = logger;
        }

        public void ProcessResponse(Message response)
        {
            Logger.WriteLogEntry(LogLevel.Info, string.Format("Sent Response: {0}", response.bson.ToJson()));
        }
    }
}
