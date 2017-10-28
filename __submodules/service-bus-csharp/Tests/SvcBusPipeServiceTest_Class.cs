using System;
using GChelpers;
using Sovos.SvcBus;
using NUnit.Framework;

namespace SvcBusTests
{
  [TestFixture, GTestStyleConsoleOutputAttribute]
  class PipeServiceTest
  {
    public void UnmanagedObjectHandlerException(UnmanagedObjectGCHelper<Type, IntPtr> obj, Exception exception,
      Type handleClass, IntPtr handle)
    {
      throw exception;
    }

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Utils.UnmanagedObjectTracker.OnUnregisterException = UnmanagedObjectHandlerException;
    }

    [Test]
    public void CreatePipeService()
    {
      using (var ps = new _PipeService(Constants.ConnectionString))
      {
        Assert.AreNotEqual(0, ps.Handle);
        ps.configureConnectionPoolCleanupTimer(1000, 1000);
      }
    }

    [Test]
    public void CreatePipeService_Fail()
    {
      Assert.Throws<PipeServiceException>(() => Builder.newPipeService("invalid conn str"));
    }

    [Test]
    public void ReleaseDeadResponsePipes()
    {
      using (var ps = new _PipeService(Constants.ConnectionString))
      {
        ps.releaseDeadResponsePipes(120*1000);
      }
    }

    [Test]
    public void StartStopAsyncWorkPool()
    {
      using (var ps = new _PipeService(Constants.ConnectionString))
      {
        ps.startAsyncWorkPool(2, 2);
        ps.stopAsyncWorkPool();
      }
    }
  }
}
