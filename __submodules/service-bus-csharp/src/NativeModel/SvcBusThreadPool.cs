using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Sovos.SvcBus;

/* 
 * Set of classes that implement non-singleton based ThreadPooling as opposed to default C# implementation based on WinApi thread-pools
 * See http://msdn.microsoft.com/en-us/library/windows/desktop/ms686760(v=vs.85).aspx for reference 
 */

namespace Sovos.WinApiThreadPool
{
  using SvcBusThreadPool = IntPtr;
  using SvcBusThreadWork = IntPtr;
  using SvcBusThreadPoolTimer = IntPtr;

  public delegate void ThreadWorkDelegate(object context);

  /*
   * This class represents WinApi ThreadPool see http://msdn.microsoft.com/en-us/library/windows/desktop/ms682456(v=vs.85).aspx
   */
  public class ThreadPool : IDisposable
  {
    private const int DisposeWorkInterval = 2000;

    private SvcBusThreadPool _pool;
    private readonly ConcurrentDictionary<ThreadWork, bool> _threadWorks;
    private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
    private readonly ThreadWork _disposeWork;

    public ThreadPool(uint minThreads, uint maxThreads)
    {
      _pool = NativeMethods.SvcBusThreadPool_alloc();
      NativeMethods.SvcBusThreadPool_init(_pool, minThreads, maxThreads + 1);
      _threadWorks = new ConcurrentDictionary<ThreadWork, bool>();

      _disposeWork = new ThreadWork(this, DisposeWorks, _stopEvent);
      _disposeWork.Submit();
    }

    ~ThreadPool()
    {
      Dispose(false);
    }

    private void DisposeWorks(Object context)
    {
      var stopEvent = (ManualResetEvent) context;
      while (!stopEvent.WaitOne(DisposeWorkInterval))
      {
        var disposedWorkList = new List<ThreadWork>();
        try
        {
          foreach (var work in _threadWorks.Keys)
          {
            if (!work.WorkCompleted) continue;
            if (_threadWorks[work])
            {
              work.Dispose();
              disposedWorkList.Add(work);
            }
            else
            {
              /* 
               * we have to do the following trick because normal Dictionary 
               * doesn't like value-part of k/v pair modified in iterator 
               */
              _threadWorks[work] = true;
            }
          }
        }
        finally
        {
          foreach (var work in disposedWorkList)
          {
            bool tmp;
            _threadWorks.TryRemove(work, out tmp);
          }
        }
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
      _stopEvent.Set();
      if (disposing)
      {
          foreach (var work in _threadWorks.Keys)
          {
            work.Wait();
            work.Dispose();
          }
        _disposeWork.Wait();
        _disposeWork.Dispose();
        _stopEvent.Dispose();
      }
      if (_pool == SvcBusThreadPool.Zero) return;
      NativeMethods.SvcBusThreadPool_destroy(_pool);
      NativeMethods.SvcBusThreadPool_dealloc(_pool);
      _pool = SvcBusThreadPool.Zero;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public void QueueUserWorkItem(ThreadWorkDelegate callBack, Object context)
    {
      var work = new ThreadWork(this, callBack, context);
      _threadWorks.GetOrAdd(work, false);
      work.Submit();
    }

    public uint ActiveWorkItemsCount {
      get
      {
        var count = 0u;
          foreach (var work in _threadWorks.Keys)
          {
            if (!work.WorkCompleted) count++;
          }
        return count;
      }
    }

    public SvcBusThreadPool Handle {
      get {
        return _pool;
      }
    }
  }

  /*
   * This class represents WinApi ThreadWork see http://msdn.microsoft.com/en-us/library/windows/desktop/ms682478(v=vs.85).aspx
   */
  public class ThreadWork : IDisposable
  {
    private SvcBusThreadWork _work;
    private readonly ThreadWorkDelegate _workDelegate;
    private readonly ThreadPool _pool;
    private readonly object _context;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly NativeMethods.SvcBusThreadPoolCallbackDelegate _nativeWorkDelegate;

    public ThreadWork(ThreadPool pool, ThreadWorkDelegate workDelegate, object context)
    {
      _pool = pool;
      _context = context;
      _workDelegate = workDelegate;
      _work = NativeMethods.SvcBusThreadWork_alloc();
      /* 
       * IMPORTANT: don't remove _nativeWorkDelegate from the class passing to NativeMethods.SvcBusThreadWork_init the method NativeWorkCallback directly.
       * If that's done it will cause the temporary delegate created to receive the callback to be garbage collected (potentially) before the callback
       * is executed causing a hard exception.
       * See article http://msdn.microsoft.com/en-us/library/vstudio/at4fb09f(v=vs.100).aspx remarks
       */
      _nativeWorkDelegate = NativeWorkCallback;
      NativeMethods.SvcBusThreadWork_init(_work, pool.Handle, _nativeWorkDelegate, IntPtr.Zero);
      WorkCompleted = false;
    }

    ~ThreadWork()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
      if (_work == SvcBusThreadWork.Zero) return;
      NativeMethods.SvcBusThreadWork_destroy(_work);
      NativeMethods.SvcBusThreadWork_dealloc(_work);
      _work = SvcBusThreadWork.Zero;
    }

    // we need to ensure all exceptions caught in current method
    // because current method called from native code we can't allow exception go upper
    private void NativeWorkCallback(IntPtr context)
    {
      if (_workDelegate == null)
        return;

      try
      {
        _workDelegate(_context);
      }
      catch (Exception e)
      {
        Console.WriteLine("Unexpeted exception in threadpool worker: {0}", e);
      }
      finally
      {
        WorkCompleted = true;
      }
    }

    public void Submit()
    {
      NativeMethods.SvcBusThreadWork_submit(_work, _pool.Handle);
      GC.KeepAlive(this);
    }

    public void Wait()
    {
      NativeMethods.SvcBusThreadWork_wait(_work);
      GC.KeepAlive(this);
    }

    public bool WorkCompleted { get; private set; }
  }

  /*
   * This class represents WinApi TimerQueueTimer see http://msdn.microsoft.com/en-us/library/windows/desktop/ms682485(v=vs.85).aspx
   */
  public class ThreadPoolTimer : IDisposable
  {
    private readonly ThreadPool _pool;
    private SvcBusThreadPoolTimer _timer;
    private readonly ThreadWorkDelegate _workDelegate;
    private readonly object _context;
    private readonly NativeMethods.SvcBusThreadPoolCallbackDelegate _nativeWorkDelegate;

    public ThreadPoolTimer(ThreadPool pool, ThreadWorkDelegate workDelegate, object context)
    {
      _pool = pool;
      _workDelegate = workDelegate;
      _context = context;
      _timer = NativeMethods.SvcBusThreadPoolTimer_alloc();
      NativeMethods.SvcBusThreadPoolTimer_init(_timer);
      /* 
       * IMPORTANT: don't remove _nativeWorkDelegate from the class passing to NativeMethods.SvcBusThreadPoolTimer_start the method NativeWorkCallback directly.
       * If that's done it will cause the temporary delegate created to receive the callback to be garbage collected (potentially) before the callback
       * is executed causing a hard exception
       * See article http://msdn.microsoft.com/en-us/library/vstudio/at4fb09f(v=vs.100).aspx remarks
       */
      _nativeWorkDelegate = NativeWorkCallback;
    }

    ~ThreadPoolTimer()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
      if (_timer == SvcBusThreadPoolTimer.Zero) return;
      NativeMethods.SvcBusThreadPoolTimer_destroy(_timer);
      NativeMethods.SvcBusThreadPoolTimer_dealloc(_timer);
      _timer = SvcBusThreadPoolTimer.Zero;
    }

    public void Start(uint millis)
    {
      NativeMethods.SvcBusThreadPoolTimer_start(_timer, _pool.Handle, _nativeWorkDelegate, IntPtr.Zero, millis);
      GC.KeepAlive(this);
    }

    public void Stop()
    {
      NativeMethods.SvcBusThreadPoolTimer_stop(_timer);
      GC.KeepAlive(this);
    }

    // we need to ensure all exceptions caught in current method
    // because current method called from native code we can't allow exception go upper
    private void NativeWorkCallback(IntPtr context)
    {
      if (_workDelegate == null)
        return;

      try
      {
        _workDelegate(_context);
      }
      catch (Exception e)
      {
        Console.WriteLine("Unexpeted exception in threadpool worker: {0}", e);
      }
    }
  }
}
