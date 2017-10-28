using Sovos.SvcBus.Common.Model.Infrastructure.Logging;

namespace Sovos.SvcBus.Common.Model.Strategies
{
    public class FileLogRequestStrategy : ChainableRequestProcessingStrategy
    {

        public FileLogRequestStrategy(FileLogger logger, ChainableRequestProcessingStrategy next)
            : base(next)
        {
            Logger = logger;
        }
        public FileLogger Logger { get; private set; }

        public FileLogRequestStrategy(FileLogger logger)
        {
            Logger = logger;
        }

        public override void Process(Message msg)
        {
            Logger.WriteLogEntry(LogLevel.Info, string.Format("Got Request: {0}", msg.bson.ToJson()));
            if (Next != null)
                Next.Process(msg);
        }
    }
}
