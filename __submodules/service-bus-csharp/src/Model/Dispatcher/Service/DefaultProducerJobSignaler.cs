using System;
using System.Threading;

namespace Sovos.SvcBus
{
  internal class DefaultProducerJobSignaler : IProducerJobSignaler, IDisposable
  {
    private readonly ManualResetEvent _workerAvailableEvent;
    private int _workingJobsCount;
    private bool _disposed;

    public int MaxJobsCount { set; get; }

    public DefaultProducerJobSignaler()
    {
      _workerAvailableEvent = new ManualResetEvent(true);
      _workingJobsCount = 0;
    }

    ~DefaultProducerJobSignaler()
    {
      Dispose(false);
    }

    public void SignalFinishedJob()
    {
      Interlocked.Decrement(ref _workingJobsCount);
      _workerAvailableEvent.Set();
    }

    public bool WaitUntilWorkerAvailable(int timeout)
    {
      return _workingJobsCount < MaxJobsCount || _workerAvailableEvent.WaitOne(timeout);
    }

    public void NotifyNewJobCreated()
    {
      if (Interlocked.Increment(ref _workingJobsCount) >= MaxJobsCount)
        _workerAvailableEvent.Reset();
    }

    public void Dispose(bool disposing)
    {
      if (_disposed) return;
      _workerAvailableEvent.Dispose();
      _disposed = true;
    }

    public void Dispose()
    {
      Dispose(true);
    }
  }
}