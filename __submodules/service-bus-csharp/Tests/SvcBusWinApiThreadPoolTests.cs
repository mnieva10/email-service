using System.Threading;
using NUnit.Framework;
using Sovos.WinApiThreadPool;
using ThreadPool = Sovos.WinApiThreadPool.ThreadPool;


namespace SvcBusTests
{
  [TestFixture, GTestStyleConsoleOutputAttribute]
  class SvcBusWinApiThreadPoolTests
  {
    private bool _callbackCalled;
    private object _passedContext;
    private volatile int _i;
    private readonly int[] _workOrders = new int[100];


    [SetUp]
    public void SetUp()
    {
      _i = 0;
      _callbackCalled = false;
      _passedContext = null;
    }

    [Test]
    public void CreateWinApiThreadPool()
    {
      using (var pool = new ThreadPool(2, 4))
      {
        Assert.AreNotEqual(null, pool);
      }
    }

    public void WorkCallback(object context)
    {
      _callbackCalled = true;
      _passedContext = context;
    }

    public void SpyOrderWorkCallback(object context)
    {
      Thread.Sleep(100);
      _workOrders[(int) context] = _i++;
    }

    [Test]
    public void WorksQueuedAllThreadsBusy_ShouldWait()
    {
      _i = 1;
      /* Only way to guarantee processing order in the test is using thread pool with 1 thread */
      using (var pool = new ThreadPool(1, 1))
      {
        for (var i = 0; i < 3; i++)
          pool.QueueUserWorkItem(SpyOrderWorkCallback, i);

        Thread.Sleep(1000);
        Assert.AreEqual(3, _workOrders[2], "last works should be processed last");
      }
    }

    [Test]
    public void ThreadPoolQueueUserWorkItem()
    {
      using (var pool = new ThreadPool(2, 4))
      {
        pool.QueueUserWorkItem(WorkCallback, this);
        Thread.Sleep(100);
        Assert.AreEqual(this, _passedContext);
        Assert.AreEqual(true, _callbackCalled);
        _callbackCalled = false;
        _passedContext = null;
        pool.QueueUserWorkItem(WorkCallback, this);
        Thread.Sleep(100);
        Assert.AreEqual(this, _passedContext);
        Assert.AreEqual(true, _callbackCalled);
        Thread.Sleep(3000);
        Assert.AreEqual(0, pool.ActiveWorkItemsCount);
      }
    }

    [Test]
    public void CreateWinApiThreadWork()
    {
      using (var pool = new ThreadPool(2, 4))
      {
        Assert.AreNotEqual(null, pool);
        using (var work = new ThreadWork(pool, WorkCallback, this))
        {
          Assert.AreNotEqual(null, work);
        }
      }
    }

    [Test]
    public void RunThreadWork()
    {
      using (var pool = new ThreadPool(2, 4))
      {
        Assert.AreNotEqual(null, pool);
        using (var work = new ThreadWork(pool, WorkCallback, this))
        {
          Assert.AreNotEqual(null, work);
          work.Submit();
          work.Wait();
          Assert.AreNotEqual(false, _callbackCalled);
          Assert.AreEqual(this, _passedContext);
        }
      }
    }

    [Test]
    public void ThreadPoolTimerExecute()
    {
      using (var pool = new ThreadPool(2, 4))
      {
        Assert.AreNotEqual(null, pool);
        using (var timer = new ThreadPoolTimer(pool, WorkCallback, this))
        {
          Assert.AreNotEqual(null, timer);
          timer.Start(50);
          Thread.Sleep(500);
          Assert.AreEqual(true, _callbackCalled);
          Assert.AreEqual(this, _passedContext);
          timer.Stop();
        }
      }
    }
  }
}
