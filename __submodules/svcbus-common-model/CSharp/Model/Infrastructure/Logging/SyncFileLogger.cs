using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Text;

namespace Sovos.SvcBus.Common.Model.Infrastructure.Logging
{
    public sealed class SyncFileLogger : ILogger, IDisposable
    {
        private const int DefaultFlushEntriesBy = 1;
        private readonly object _lock = new object();
        private readonly StreamWriter _writer;
        private FileStream _file;
        private uint _notFlushedEntries;
        public string Name { get { return _file.Name; } }

        private const string LogMask = "{0}: Status:{1} -- Details: Thread Id {2}. {3}";

        public uint FlushEntriesBy { get; set; }

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

        public SyncFileLogger(FileLoggerConfig config)
        {
            Config = config;
            FlushEntriesBy = DefaultFlushEntriesBy;
            CreateFilestream(Config.TargetFile);
            _writer = new StreamWriter(_file);
            WriteHeaderText();
        }

        public void WriteLogEntry(LogLevel logLevel, string message)
        {
            if (logLevel > Config.LogLevel) return;

            var s = string.Format(LogMask, DateTime.Now, logLevel, Thread.CurrentThread.ManagedThreadId, message);
            lock (_lock)
            {
                _writer.WriteLine(s);
                _notFlushedEntries++;
                if (_notFlushedEntries >= FlushEntriesBy || logLevel <= LogLevel.Warning)
                    UnsafeFlush();
            }
        }

        private void WriteHeaderText()
        {
            _writer.WriteLine("==================== Initializing logger for PID ({0}) with LOG_LEVEL=({1}) ====================",
                Process.GetCurrentProcess().Id, Config.LogLevel);
        }

        void UnsafeFlush()
        {
            _writer.Flush();
            _notFlushedEntries = 0;
        }

        public void Flush()
        {
            lock (_lock)
                UnsafeFlush();
        }

        public void Dispose()
        {
            _writer.Dispose();
            _file.Dispose();
        }

        public void WriteLogEntry(object sender, Exception e, string msg, LogLevel logLevel)
        {
            var sb = new StringBuilder();
            sb.AppendLine(msg);
            sb.Append("\tSender: ").AppendLine(sender.ToString());
            sb.Append("\tException message: ").AppendLine(e.Message);
            sb.Append("\tException stack trace: ").AppendLine(e.StackTrace);

            WriteLogEntry(logLevel, sb.ToString());
        }
    }
}
