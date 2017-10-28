using System.Threading;
using Sovos.SvcBus;
using NUnit.Framework;

namespace SvcBusTests
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture, GTestStyleConsoleOutputAttribute]
  class DispatchInterfaceConsumerPoolTest
  {
    private Scope _scope;
    private PipeService _ps;
    private DispatchInterfaceConsumerPool _pool;

    [SetUp]
    public void SetUp()
    {
      _scope = new Scope();
      _ps = _scope.newPipeService(Constants.ConnectionString);
      _pool = _scope.newDispatchInterfaceConsumerPool(_ps, Builder.newService("SvcBusDispatchInterfaceConsumerPoolTest", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE), "Test");
      _pool.HeartbeatTimerInterval = 0;
    }

    [TearDown]
    public void TearDown()
    {
      _scope.Dispose();
      _ps = null;
      _pool = null;
    }

    [Test]
    public void AcquireIfNoReleased_shouldReturnNewInstance()
    {
      var dic1 = _pool.Acquire();
      try
      {
        var dic2 = _pool.Acquire();
        try
        {
          Assert.AreNotEqual(dic1, dic2);
        }
        finally
        {
          _pool.Release(dic2);
        }
      }
      finally
      {
        _pool.Release(dic1);
      }
    }

    [Test]
    public void AcquireIfReleased_shouldReturnSameInstance()
    {
      var dic1 = _pool.Acquire();
      var dic1Handle = dic1.Consumer.Handle;
      _pool.Release(dic1);
      var dic2 = _pool.Acquire();
      try
      {
        Assert.AreEqual(dic1Handle, dic2.Consumer.Handle);
      }
      finally
      {
        _pool.Release(dic2);
      }
    }

    [Test]
    public void ShouldSetLoggerForAcquiredObjects()
    {
      _pool.Logger = new LoggerSpy();

      var dic = _pool.Acquire();
      try
      {
        Assert.AreEqual(_pool.Logger, dic.Logger);
      }
      finally
      {
        _pool.Release(dic);
      }
    }
  }
}
