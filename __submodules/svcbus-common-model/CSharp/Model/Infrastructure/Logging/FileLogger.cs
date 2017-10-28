using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Sovos.SvcBus.Common.Model.Infrastructure.Logging
{
    public sealed class FileLogger : ILogger, IDisposable
    {
        private const int AsyncWaitMilliseconds = 100;
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private readonly ManualResetEvent _terminating = new ManualResetEvent(false);
        private readonly ManualResetEvent _terminated = new ManualResetEvent(false);
        private readonly StreamWriter _writer;
        private FileStream _file;
        public string Name { get { return _file.Name; } }

        private const string LogMask = "{0}: Status:{1} -- Details: Thread Id {2}. {3}";

        public FileLoggerConfig Config { get; private set; }

        private string AppendBeforeConfigFilenameExtension(string additional)
        {
            var dir = Path.GetDirectoryName(Config.TargetFile);
            if (string.IsNullOrEmpty(dir))
                dir = ".";
            return string.Format("{0}({1}){2}", Path.Combine( dir,
                Path.GetFileNameWithoutExtension(Config.TargetFile)),
                additional, Path.GetExtension(Config.TargetFile));
        }

        private void CreateFilestream(string name)
        {
            var i = 1;
            var createName = name;
            while (true)
                try
                {
                    _file = new FileStream(createName, FileMode.Append);
                    break;
                }
                catch (IOException)
                {
                    if (i >= Config.MaxLogFiles)
                        throw;
                    createName = AppendBeforeConfigFilenameExtension((++i).ToString());
                }
        }

        private void WriteLogs()
        {
            string logEntry;
            while (_logQueue.TryDequeue(out logEntry))
                _writer.WriteLine(logEntry);
        }

        private void LogQueueHandler()
        {
            try
            {
                while (!_terminating.WaitOne(AsyncWaitMilliseconds))
                    WriteLogs();

                // flush the remaining logs before disposal
                WriteLogs();
            }
            finally
            {
                _terminated.Set();
            }
        }

        public FileLogger(FileLoggerConfig config)
        {
            Config = config;
            CreateFilestream(Config.TargetFile);
            _writer = new StreamWriter(_file) {AutoFlush = true};
            new Thread(LogQueueHandler).Start();
            WriteHeaderText();
        }

        public void WriteLogEntry(LogLevel logLevel, string message)
        {
            if (logLevel > Config.LogLevel) return;

            var s = string.Format(LogMask, DateTime.Now, logLevel, Thread.CurrentThread.ManagedThreadId, message);
            _logQueue.Enqueue(s);
        }

        public void WriteLogEntry(object sender, Exception e, string msg, LogLevel logLevel)
        {
            var sb = new StringBuilder();
            sb.AppendLine(msg);
            sb.Append("\tSender: ").AppendLine(sender.ToString());
            sb.Append("\tException type: ").AppendLine(e.GetType().Name);
            sb.Append("\tException message: ").AppendLine(e.Message);
            sb.Append("\tException stack trace: ").AppendLine(e.StackTrace);

            WriteLogEntry(logLevel, sb.ToString());
        }

        private void WriteHeaderText()
        {
            _logQueue.Enqueue(string.Format("==================== Initializing logger for PID ({0}) with LOG_LEVEL=({1}) ====================",
                Process.GetCurrentProcess().Id, Config.LogLevel));
        }

        public void Dispose()
        {
            _terminating.Set();
            _terminated.WaitOne();
            _writer.Dispose();
            _file.Dispose();
        }
    }
}
