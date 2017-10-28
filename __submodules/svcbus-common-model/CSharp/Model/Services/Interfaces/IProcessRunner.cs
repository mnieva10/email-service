using System.Diagnostics;
using Sovos.SvcBus.Common.Model.Operation;

namespace Sovos.SvcBus.Common.Model.Services.Interfaces
{
    public interface IProcessRunner
    {
        ProcessResponse Run(string parameters);
        bool RunSync(string parameters, int timeout = 0, bool stopOnCompletion = false);
        Process RunAsync(string parameters); //out string error, out string output);
        void Stop(Process process, bool stopOnCompletion = false);
    }
}
