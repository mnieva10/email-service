using System.Threading;
using Sovos.SvcBus.Common.Model.Infrastructure.Logging;

namespace TestFileLoggerConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var logfile = args[0];
                using (new FileLogger(new FileLoggerConfig {TargetFile = logfile}))
                    Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
