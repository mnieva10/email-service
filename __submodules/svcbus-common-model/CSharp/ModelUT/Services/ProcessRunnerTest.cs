using System;
using System.Diagnostics;
using Sovos.SvcBus.Common.Model.Exceptions;
using Sovos.SvcBus.Common.Model.Services;
using Sovos.SvcBus.Common.Model.Services.Interfaces;
using NUnit.Framework;
using System.IO;
using System.Threading;

namespace ModelUT.Services
{
    //[TestFixture]
    public class ProcessRunnerTest
    {
        private static readonly string EmptyParameters = string.Empty;

        //The test program "AppAboutNothing" runs indefinitely unless supplied with an argument of -s#### for timeout in seconds.

        //[Test]
        //[ExpectedException(typeof(ProcessRunnerException))]
        //public void RunAsyncProcessWithWrongExe()
        //{
        //    IProcessRunner processRunner = new ProcessRunner("Fake.exe");
        //    //string a, b;
        //    processRunner.RunAsync(EmptyParameters); //out a, out b);
        //}

        //[Test]
        //[ExpectedException(typeof(ProcessRunnerException))]
        //public void RunSyncProcessWithWrongExe()
        //{
        //    IProcessRunner processRunner = new ProcessRunner("Fake.exe");
        //    processRunner.RunSync(EmptyParameters);
        //}

        //[Test]
        //public void RunSyncProcessTimeout()
        //{
        //    const string parameters = "-s10";
        //    IProcessRunner processRunner = new ProcessRunner(TestProcessLocation());
        //    var result = processRunner.RunSync(parameters, 4000);
        //    Assert.AreEqual(false, result);
        //}

        //[Test]
        //public void RunSyncProcessCompleteTimeout()
        //{
        //    IProcessRunner processRunner = new ProcessRunner(TestProcessLocation());
        //    var result = processRunner.RunSync(EmptyParameters, 4000);
        //    Assert.AreEqual(false, result);
        //}

        //[Test]
        //public void RunSyncProcessCompleteNoTimeout()
        //{
        //    IProcessRunner processRunner = new ProcessRunner("rundll32");
        //    var result = processRunner.RunSync(EmptyParameters);
        //    Assert.AreEqual(true, result);
        //}

        //[Test]
        //public void RunSyncProcessNoParameters()
        //{
        //    IProcessRunner processRunner = new ProcessRunner(TestProcessLocation());
        //    bool result = processRunner.RunSync(EmptyParameters, 4000);
        //    Assert.AreEqual(false, result);
        //}

        //[Test]
        //public void RunSyncProcessWithParameters()
        //{
        //    const string parameters = "-s1";
        //    IProcessRunner processRunner = new ProcessRunner(TestProcessLocation());
        //    var result = processRunner.RunSync(parameters, 4000);
        //    Assert.AreEqual(true, result);
        //}

        //[Test]
        //public void RunAsyncProcess()
        //{
        //    const string parameters = "-s2";
        //    IProcessRunner processRunner = new ProcessRunner(TestProcessLocation());
        //    //string a, b;
        //    var process = processRunner.RunAsync(parameters); //out a, out b);
        //    Assert.NotNull(process);
        //    Assert.AreEqual("System.Diagnostics.Process (AppAboutNothing)", process.ToString());
        //    processRunner.Stop(process, true);
        //}

        //[Test]
        //public void RunAsyncProcessNoParameters()
        //{
        //    IProcessRunner processRunner = new ProcessRunner(TestProcessLocation());
        //    //string a, b;
        //    var process = processRunner.RunAsync(EmptyParameters); //out a, out b);
        //    Assert.AreNotEqual(null, process);
        //    Assert.AreEqual(false, process.HasExited);
        //    Thread.Sleep(1500);
        //    Assert.AreEqual(false, process.HasExited);
        //    processRunner.Stop(process, true);
        //}

        //[Test]
        //public void RunAsyncProcessWithParameters()
        //{
        //    const string parameters = "-s1";
        //    IProcessRunner processRunner = new ProcessRunner(TestProcessLocation());
        //    //string a, b;
        //    var process = processRunner.RunAsync(parameters); //out a, out b);
        //    Assert.AreNotEqual(null, process);
        //    Assert.AreEqual(false, process.HasExited);
        //    Thread.Sleep(2000);
        //    Assert.AreEqual(true, process.HasExited);
        //}

        //[Test]
        //public void RunSyncMultiple()
        //{
        //    const string parameters = "10000";
        //    const int testCount = 10;
        //    IProcessRunner processRunner = new ProcessRunner(TestProcessLocation());

        //    var threads = new Thread[testCount];
        //    for (var processNdx = 0; processNdx < testCount; processNdx++)
        //    {
        //        threads[processNdx] = new Thread(() => processRunner.RunSync(parameters));
        //        threads[processNdx].Start();
        //    }
        //    // The threads start but RunSync may not have started. Sleep to allow each process to start.
        //    Thread.Sleep(1000);

        //    var processes = Process.GetProcessesByName("AppAboutNothing");
        //    Assert.AreEqual(testCount, processes.Length);
        //    foreach (var t in processes)
        //        processRunner.Stop(t, true);
        //}
        
        //[Test]
        //public void RunAsyncMultiple()
        //{
        //    const string parameters = "10000";
        //    const int testCount = 10;
        //    IProcessRunner processRunner = new ProcessRunner(TestProcessLocation());
        //    var processes = new Process[testCount];
        //    for (var processNdx = 0; processNdx < testCount; processNdx++)
        //    {
        //        //string a, b;
        //        processes[processNdx] = processRunner.RunAsync(parameters); //out a, out b);
        //    }
        //    // See that they're all running.
        //    var runningCount = 0;
        //    for (var processNdx = 0; processNdx < testCount; processNdx++)
        //        if (!processes[processNdx].HasExited)
        //        {
        //            runningCount++;
        //            processRunner.Stop(processes[processNdx], true);
        //        }
        //    Assert.AreEqual(testCount, runningCount);
        //}

        private string TestProcessLocation()
        {
            var systemAssembly = GetType().Assembly;
            var codeBase = systemAssembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            path = string.Format("{0}\\AppAboutNothing.EXE", Path.GetDirectoryName(path));
            if (!File.Exists(path))
                throw new FileNotFoundException(string.Format("File not found '{0}'", path));
            return path;
        }
    }
}