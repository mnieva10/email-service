using System;
using System.Diagnostics;
using System.Threading;
using Sovos.SvcBus.Common.Model.Exceptions;
using Sovos.SvcBus.Common.Model.Operation;
using Sovos.SvcBus.Common.Model.Services.Interfaces;
using Environment = System.Environment;

namespace Sovos.SvcBus.Common.Model.Services
{
    public class ProcessRunner : IProcessRunner
    {
        public string ExePath { get; private set; }

        public ProcessRunner(string exePath)
        {
            ExePath = exePath;
        }

        private ProcessStartInfo SetUpProcess(string parameters)
        {
            return new ProcessStartInfo
            {
#if !NETCORE
                Domain = Environment.CurrentDirectory,
#endif
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = ExePath,
                Arguments = parameters
            };
        }

        public ProcessResponse Run(string parameters)
        {
            string processOutput;
            string processError;

            try
            {
                var startInfo = SetUpProcess(parameters);

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                        throw new ProcessRunnerException(startInfo.FileName);

                    using (var error = process.StandardError)
                        processError = error.ReadToEnd();

                    using (var output = process.StandardOutput)
                        processOutput = output.ReadToEnd();

                    process.WaitForExit();
                    process.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new ProcessRunnerException(ex.Message);
            }

            return new ProcessResponse(processError, processOutput);
        }

        public bool RunSync(string parameters, int timeout = 0, bool stopOnCompletion = false)
        {
            try
            {
                var startInfo = SetUpProcess(parameters);
                return RunProcess(startInfo, timeout, stopOnCompletion);
            }
            catch (Exception ex)
            {
                throw new ProcessRunnerException(ex.Message);
            }
        }

        public Process RunAsync(string parameters) //out string error, out string output)
        {
            try
            {
                var startInfo = SetUpProcess(parameters);
                var process = Process.Start(startInfo);
                //todo: reading error seems to close the process
                //when functionality is needed -- need to investigate
                //error = "Not Implemented"; //process.StandardError.ReadToEnd();
                //output = "Not Implemented"; //process.StandardOutput.ReadToEnd();

                return process;
            }
            catch (Exception ex)
            {
                throw new ProcessRunnerException(ex.Message);
            }
        }

        private bool RunProcess(ProcessStartInfo startInfo, int timeoutMs = 0, bool stopOnCompletion = false)
        {
            const int refreshIntervalMs = 2000;
            var timeElapsed = 0;

            var process = Process.Start(startInfo);
            if (process == null)
                throw new ProcessRunnerException(string.Empty);

            // Continue while running and not timed out.
            while (!process.HasExited && (timeoutMs == 0 || timeElapsed < timeoutMs))
            {
                Thread.Sleep(refreshIntervalMs);
                timeElapsed += refreshIntervalMs;
            }
            bool finished = process.HasExited;
            Stop(process, stopOnCompletion);

            return finished;
        }

        public void Stop(Process process, bool stopOnCompletion)
        {
            try
            {
                if (process.HasExited) return;
#if !NETCORE
                if (process.CloseMainWindow())
                    process.Dispose();
#endif
                else if (stopOnCompletion)
                    process.Kill();
            }
            catch (Exception ex)
            {
                throw new ProcessRunnerException(ex.Message);
            }
        }
    }
}
