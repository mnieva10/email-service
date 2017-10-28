using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Sovos.SvcBus.Common.Model.Infrastructure.Logging;
using NUnit.Framework;
using Sovos.SvcBus;

namespace ModelUT.Infrastructure.Logging
{
    [TestFixture]
    public class FileLoggerTest
    {
        private const string FileLoggerCounsumerProcessPath = @"..\..\..\__submodules\svcbus-common-model\CSharp\ModelUT\testdata\TestFileLoggerConsumer.exe";
        private FileLoggerConfig _config;
        private const string Logfile = "FileLoggerTest.log";

        [SetUp]
        public void SetUp()
        {
            _config = new FileLoggerConfig {TargetFile = Path.GetTempFileName()};
        }

        [TearDown]
        public void TearDown()
        {
            KillFileLoggerConsumers();
            if (File.Exists(_config.TargetFile))
                File.Delete(_config.TargetFile);
            DeleteFileLoggerLogFiles();
        }

        [Test]
        public void Initialize_WithANonExistentPath_CreateInitializationLogsToNewFile()
        {
            using (new FileLogger(_config))
            {
            }
            var textFileContent = File.ReadAllText(_config.TargetFile);
            Assert.IsNotNull(textFileContent);
            Assert.IsNotEmpty(textFileContent);
        }

        [Test]
        public void Initialize_WithAExistentPath_AddInitializationLogs()
        {
            File.WriteAllText(_config.TargetFile, "Some fake content");

            var firstContent = File.ReadAllText(_config.TargetFile);
            using (new FileLogger(_config))
            {
            }
            Assert.AreNotEqual(firstContent, File.ReadAllText(_config.TargetFile));
        }

        [TestCase(LogLevel.Warning)]
        [TestCase(LogLevel.Fatal)]
        public void Initialize_WithANonExistingPathAndCustomLogLevel_AddAHeaderText(LogLevel level)
        {
            _config.LogLevel = level;
            var expectedHeaderText =
                string.Format("==================== Initializing logger for PID ({0}) with LOG_LEVEL=({1}) ====================",
                Process.GetCurrentProcess().Id.ToString(), _config.LogLevel);

            using (new FileLogger(_config))
            {
            }
            Assert.AreEqual(string.Format(expectedHeaderText, level), File.ReadLines(_config.TargetFile).Last());
        }

        [Test]
        public void Initialize_WithValidLogLevel_AssignLogLevel()
        {
            _config.LogLevel = LogLevel.Warning;
            using (var logger = new FileLogger(_config))
                Assert.AreEqual(LogLevel.Warning, logger.Config.LogLevel);
        }

        [Test]
        public void Initialize_WithoutLogLevel_AssignsGreatestValue()
        {
            using (var logger = new FileLogger(_config))
                Assert.AreEqual(LogLevel.Fatal, logger.Config.LogLevel);
        }

        [Test]
        public void Write_WithANewFile_WriteOnANewFileUsingAMask()
        {
            _config.LogLevel = LogLevel.Debug;
            using (var logger = new FileLogger(_config))
                logger.WriteLogEntry(LogLevel.Debug, "I'm a dummy log message");

            var now = DateTime.Now;
            var textFileContent = File.ReadLines(_config.TargetFile).ToList();
            var text = textFileContent.Last();

            Assert.AreEqual(2, textFileContent.Count());

            var indexToSplit = text.IndexOf(": Status:");
            var indexToLeftText = indexToSplit + ": Status".Length + 1;

            var currentDateTime = Convert.ToDateTime(text.Substring(0, indexToSplit));
            var timeSpan = now - currentDateTime;

            Assert.AreEqual(0, timeSpan.Hours);
            Assert.AreEqual(0, timeSpan.Days);
            Assert.LessOrEqual(timeSpan.Milliseconds, 1000);

            Assert.True(text.Substring(indexToLeftText).EndsWith("I'm a dummy log message"));
        }

        [Test]
        public void Write_WithInfoLogLevel_DontWriteFileUnderInfoLogLevel()
        {
            _config.LogLevel = LogLevel.Info;
            using (var logger = new FileLogger(_config))
            {
                logger.WriteLogEntry(LogLevel.Debug, "I'm a dummy log message on Debug Mode");
                logger.WriteLogEntry(LogLevel.Info, "I'm a dummy log message on Info Mode");
                logger.WriteLogEntry(LogLevel.Warning, "I'm a dummy log message on Warning Mode");
                logger.WriteLogEntry(LogLevel.Error, "I'm a dummy log message on Error Mode");
                logger.WriteLogEntry(LogLevel.Fatal, "I'm a dummy log message on Fatal Mode");
                logger.WriteLogEntry(LogLevel.Always, "I'm a dummy log message on Always Mode");
            }

            var expectedLoggedMessages = new[]
            {
                "I'm a dummy log message on Info Mode",
                "I'm a dummy log message on Warning Mode",
                "I'm a dummy log message on Error Mode",
                "I'm a dummy log message on Fatal Mode",
                "I'm a dummy log message on Always Mode"
            };

            var textFileContent = File.ReadLines(_config.TargetFile).ToList();

            Assert.AreEqual(expectedLoggedMessages.Length + 1, textFileContent.Count());

            foreach (var logMsg in textFileContent.Skip(1))
            {
                var indexOfDetailsText = logMsg.IndexOf("I'm a");
                Assert.Contains(logMsg.Substring(indexOfDetailsText), expectedLoggedMessages);
            }
        }

        [Test]
        public void Write_WithDebugLogLevel_DontWriteFileUnderInfoLogLevel()
        {
            _config.LogLevel = LogLevel.Debug;
            using (var logger = new FileLogger(_config))
            {
                logger.WriteLogEntry(LogLevel.Debug, "I'm a dummy log message on Debug Mode");
                logger.WriteLogEntry(LogLevel.Info, "I'm a dummy log message on Info Mode");
                logger.WriteLogEntry(LogLevel.Warning, "I'm a dummy log message on Warning Mode");
                logger.WriteLogEntry(LogLevel.Error, "I'm a dummy log message on Error Mode");
                logger.WriteLogEntry(LogLevel.Fatal, "I'm a dummy log message on Fatal Mode");
                logger.WriteLogEntry(LogLevel.Always, "I'm a dummy log message on Always Mode");
            }

            var expectedLoggedMessages = new[]
            {
                "I'm a dummy log message on Debug Mode",
                "I'm a dummy log message on Info Mode",
                "I'm a dummy log message on Warning Mode",
                "I'm a dummy log message on Error Mode",
                "I'm a dummy log message on Fatal Mode",
                "I'm a dummy log message on Always Mode"
            };

            var textFileContent = File.ReadLines(_config.TargetFile).ToList();

            Assert.AreEqual(expectedLoggedMessages.Length + 1, textFileContent.Count());

            foreach (var logMsg in textFileContent.Skip(1))
            {
                var indexOfDetailsText = logMsg.IndexOf("I'm a");
                Assert.Contains(logMsg.Substring(indexOfDetailsText), expectedLoggedMessages);
            }
        }

        [Test]
        public void Write_WithDefaulLogLevel_DontWriteFileUnderInfoLogLevel()
        {
            using (var logger = new FileLogger(_config))
            {
                logger.WriteLogEntry(LogLevel.Debug, "I'm a dummy log message on Debug Mode");
                logger.WriteLogEntry(LogLevel.Info, "I'm a dummy log message on Info Mode");
                logger.WriteLogEntry(LogLevel.Warning, "I'm a dummy log message on Warning Mode");
                logger.WriteLogEntry(LogLevel.Error, "I'm a dummy log message on Error Mode");
                logger.WriteLogEntry(LogLevel.Fatal, "I'm a dummy log message on Fatal Mode");
                logger.WriteLogEntry(LogLevel.Fatal, "I'm a dummy log message on Always Mode");
            }

            var expectedLoggedMessages = new[] 
            {
                "I'm a dummy log message on Fatal Mode",
                "I'm a dummy log message on Always Mode"
            };

            var textFileContent = File.ReadLines(_config.TargetFile).ToList();

            Assert.AreEqual(expectedLoggedMessages.Length + 1, textFileContent.Count());

            foreach (var logMsg in textFileContent.Skip(1))
            {
                var indexOfDetailsText = logMsg.IndexOf("I'm a");
                Assert.Contains(logMsg.Substring(indexOfDetailsText), expectedLoggedMessages);
            }
        }

        [Test]
        public void Write_FromMultipleThreads_Success()
        {
            const int threadCount = 100;
            const int logTimes = 1000;

            _config.LogLevel = LogLevel.Debug;
            using (var logger = new FileLogger(_config))
            {
                var threads = new Thread[threadCount];

                for (var i = 0; i < threadCount; i++)
                    threads[i] = new Thread(() =>
                    {
                        for (var j = 0; j < 1000; j++)
                        {
                            if (j % 45 == 0) Thread.Sleep(50);
                            logger.WriteLogEntry(LogLevel.Debug, "I'm the #" + Thread.CurrentThread.ManagedThreadId);
                        }
                    });

                foreach (var t in threads) t.Start();

                while (true)
                {
                    var statesRunningThreads = threads.Select(t => t.IsAlive).Distinct();
                    if (statesRunningThreads.Count() == 1 && !statesRunningThreads.First()) break;

                    Thread.Sleep(50);
                }

            }
            var textFileContent = File.ReadLines(_config.TargetFile);
            Assert.AreEqual(threadCount * logTimes + 1, textFileContent.Count());
        }

        [Test]
        public void Write_FromMultipleThreads_LessContentionInAsync()
        {
            const int threadCount = 10;
            const string DebugText =
                "Some verbose debugging text. Some verbose debugging text. Some verbose debugging text. Some verbose debugging text. Some verbose debugging text." + 
                "Some verbose debugging text. Some verbose debugging text. Some verbose debugging text. Some verbose debugging text. Some verbose debugging text." +
                "Some verbose debugging text. Some verbose debugging text. Some verbose debugging text. Some verbose debugging text. Some verbose debugging text.";
            
            _config.LogLevel = LogLevel.Debug;

            var sw = new Stopwatch();
            long elapsedSync;
            long elapsedAsync;

            using (var logger = new FileLogger(_config))
            {
                var threads = new Thread[threadCount];

                for (var i = 0; i < threadCount; i++)
                    threads[i] = new Thread(() =>
                    {
                        for (var j = 0; j < 10000; j++)
                            logger.WriteLogEntry(LogLevel.Debug, "I'm the #" + Thread.CurrentThread.ManagedThreadId + " - " + DebugText);
                    });

                sw.Start();

                foreach (var t in threads) t.Start();

                while (true)
                {
                    var statesRunningThreads = threads.Select(t => t.IsAlive).Distinct();
                    if (statesRunningThreads.Count() == 1 && !statesRunningThreads.First()) break;

                    Thread.Sleep(50);
                }

                sw.Stop();

                elapsedAsync = sw.ElapsedMilliseconds;
            }

            using (var logger = new SyncFileLogger(_config))
            {
                var threads = new Thread[threadCount];

                for (var i = 0; i < threadCount; i++)
                    threads[i] = new Thread(() =>
                    {
                        for (var j = 0; j < 10000; j++)
                            logger.WriteLogEntry(LogLevel.Debug, "I'm the #" + Thread.CurrentThread.ManagedThreadId + " - " + DebugText);
                    });

                sw.Reset();
                sw.Start();

                foreach (var t in threads) t.Start();

                while (true)
                {
                    var statesRunningThreads = threads.Select(t => t.IsAlive).Distinct();
                    if (statesRunningThreads.Count() == 1 && !statesRunningThreads.First()) break;

                    Thread.Sleep(50);
                }

                sw.Stop();

                elapsedSync = sw.ElapsedMilliseconds;
            }

            Assert.Less(elapsedAsync, elapsedSync, string.Format("Async time: {0} not less than sync time: {1}", elapsedAsync, elapsedSync));
        }

        [Test]
        public void MultipleProcessesUsingSameLogfile_ShouldTryMaxTimes()
        {
            Assert.Throws<IOException>(() =>
            {
                using (File.Open(_config.TargetFile, FileMode.OpenOrCreate))
                {
                    _config.MaxLogFiles = 2;
                    using (var logger = new FileLogger(_config)) { }
                    Assert.AreEqual(2, GetCountOfLogFiles(_config.TargetFile));

                    _config.MaxLogFiles = 1;
                    File.Open(_config.TargetFile, FileMode.OpenOrCreate);
                    using (var logger = new FileLogger(_config)) { }
                }
            });
        }

        [Test]
        public void MultipleProcessesUsingSameLogfile_ShouldAppendToFilename()
        {
            for (var i = 0; i < 2; i++)
                Process.Start(FileLoggerCounsumerProcessPath, Logfile);
            Thread.Sleep(500);
            Assert.AreEqual(2, GetCountOfLogFiles(Logfile));
        }

        private static int GetCountOfLogFiles(string logfile)
        {
            var dirPath = Path.GetDirectoryName(logfile);
            return Directory.GetFiles(string.IsNullOrEmpty(dirPath) ? "." : dirPath, Path.GetFileNameWithoutExtension(logfile) + "*").Length;
        }

        private static void DeleteFileLoggerLogFiles()
        {
            var dirPath = Path.GetDirectoryName(Logfile);
            foreach (
                var f in
                    Directory.GetFiles(string.IsNullOrEmpty(dirPath) ? "." : dirPath,
                        Path.GetFileNameWithoutExtension(Logfile) + "*"))
            {
                File.Delete(f);
            }           
        }

        private static void KillFileLoggerConsumers()
        {
            foreach (var p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(FileLoggerCounsumerProcessPath)))
            {
                p.Kill();
                p.WaitForExit();
            }
        }
    }
}