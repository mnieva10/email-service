using System;
using System.Collections.Generic;
using System.Threading;
using Sovos.SvcBus;

namespace SvcBusTests
{
  public class LoggerSpy : ILogger
  {
    private readonly object _lock = new object();
    private readonly List<string> _logEntries = new List<string>();

    public List<string> LogEntries
    {
      get
      {
        return _logEntries;
      }
    }

    public virtual void WriteLogEntry(LogLevel logLevel, string message)
    {
      lock (_lock)
        _logEntries.Add(string.Format("{0}: {1}", logLevel, message));
    }

    public virtual void WriteLogEntry(object sender, Exception e, string msg, LogLevel logLevel)
    {
      lock (_lock)
        _logEntries.Add(string.Format("{0}:{1}:{2}", e.Message, msg, logLevel));
    }
  }

  public sealed class AsyncLoggerSpy : LoggerSpy, IDisposable
  {
    private readonly ManualResetEvent _writtenEvent = new ManualResetEvent(false);

    public override void WriteLogEntry(LogLevel logLevel, string message)
    {
      base.WriteLogEntry(logLevel, message);
      _writtenEvent.Set();
    }

    public override void WriteLogEntry(object sender, Exception e, string msg, LogLevel logLevel)
    {
      base.WriteLogEntry(sender, e, msg, logLevel);
      _writtenEvent.Set();
    }

    public bool WaitForEntryWritten(int milisecondsTimeout)
    {
      return _writtenEvent.WaitOne(milisecondsTimeout);
    }

    public void Dispose()
    {
      _writtenEvent.Dispose();
    }
  }
}
