using System;
using System.Collections.Generic;
using Sovos.SvcBus;

namespace ModelUT.Services.Stubs
{
    public class LoggerStub: ILogger
    {
        public List<string> LogEntries { get; set; }

        public LoggerStub()
        {
            LogEntries = new List<string>();
        }

        public void WriteLogEntry(LogLevel logLevel, string message)
        {
            LogEntries.Add(string.Format("{0}: {1}", logLevel, message));
        }

        public void WriteLogEntry(object sender, Exception e, string msg, LogLevel logLevel)
        {
            LogEntries.Add(string.Format("{0}: {1}. Exc: {2}", logLevel, msg, e.Message));
        }
    }
}
