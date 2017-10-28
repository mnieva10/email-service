using System.Threading;

namespace Sovos.SvcBus
{
  internal interface IProducerJobSignaler
  {
    int MaxJobsCount { get; set; }

    void SignalFinishedJob();
    bool WaitUntilWorkerAvailable(int timeout = Timeout.Infinite);
    void NotifyNewJobCreated();
  }
}