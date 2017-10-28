using Sovos.SvcBus;
using NUnit.Framework;

namespace SvcBusTests
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture, GTestStyleConsoleOutputAttribute]
  class SvcBusMutexTests
  {
    private Mutex mutex;
    private PipeService ps;

    [SetUp]
    public void SetUp()
    {
      using (var scope = new Scope())
      {
        ps = Builder.newPipeService(Constants.ConnectionString);
        mutex = scope.newMutex(ps, "THE_MUTEX", "THE_SERVICE");
        mutex.remove();
        mutex = Builder.newMutex(ps, "THE_MUTEX", "THE_SERVICE");
      }
    }

    [TearDown]
    public void TearDown()
    {
      mutex.Dispose();
      ps.Dispose();
      mutex = null;
      ps = null;
    }

    [Test]
    public void TestCreateMutex()
    {
      Assert.AreNotEqual(null, mutex);
    }

    [Test]
    public void TestCheckMutexHandle()
    {
      Assert.AreNotEqual(0, mutex.Handle);
    }

    [Test]
    public void TestAcquireAndRelease()
    {
      Assert.IsTrue(mutex.acquire());
      Assert.IsTrue(mutex.release());
    }

    [Test]
    public void TestAcquireAnonymousMutex_Failure()
    {
    Assert.That(()=>
      {
        using (var mutex2 = Builder.newMutex(ps))
        {
          Assert.IsTrue(mutex2.acquire());
        }
      }, Throws.TypeOf<MutexException>());
    }

    [Test]
    public void TestAcquireIterateLockedAndRelease()
    {
      Assert.IsTrue(mutex.acquire());
      mutex.lockedMutexQueryInit();
      try
      {
        Bson curr;
        bool found = false;
        while(!found && mutex.lockedMutexQueryNext(out curr))
        {
          var it = curr.find("mutex_name");
          Assert.IsTrue(curr != null);
          found = "THE_MUTEX" == (string) it;
        }
        Assert.IsTrue(found);
      }
      finally
      {
        mutex.lockedMutexQueryDone();
      }
      Assert.IsTrue(mutex.release());
    }

    [Test]
    public void TestAcquireIterateLockedAndForceRelease()
    {
      Assert.IsTrue(mutex.acquire());
      mutex.lockedMutexQueryInit();
      try
      {
        Bson curr;
        bool found = false;
        while (!found && mutex.lockedMutexQueryNext(out curr))
        {
          var it = curr.find("mutex_name");
          Assert.IsTrue(curr != null);
          found = "THE_MUTEX" == (string)it;
          if (!found) continue;
          var computerName = (string)curr.find("computer_name");
          var procName = (string)curr.find("curproc_name");
          var procId = (int)curr.find("curproc_id");
          using (var mutexToRelease = Builder.newMutex(ps, (string)it, "THE_SERVICE"))
          {
            Assert.IsTrue(mutexToRelease.forceRelease(computerName, procName, procId));
          }
        }
        Assert.IsTrue(found);
      }
      finally
      {
        mutex.lockedMutexQueryDone();
      }
      Assert.IsFalse(mutex.release()); // Mutex already forceReleased
    }
  }
}
