using Sovos.SvcBus.Common.Model.Infrastructure.Logging;

namespace Sovos.SvcBus.Common.Model.Strategies
{
    public class ShortFileLogResponseStrategy : IResponseProcessingStrategy
    {
        public FileLogger Logger { get; private set; }

        public ShortFileLogResponseStrategy(FileLogger logger)
        {
            Logger = logger;
        }

        public void ProcessResponse(Message response)
        {
            Logger.WriteLogEntry(LogLevel.Info, "Sent Response");
        }
    }
}