using System;
using System.Threading;
using Sovos.SvcBus;
using NUnit.Framework;

namespace SvcBusTests
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture, GTestStyleConsoleOutputAttribute]
  class SvcBusStatsCollectorTests
  {
    private StatsCollector statsCollector;
    private PipeService ps;
    private Producer statsProducer;
    private Service statsService;

    [SetUp]
    public void SetUp()
    {
      GC.Collect();
      ps = Builder.newPipeService(Constants.ConnectionString);
      ps.startAsyncWorkPool(1, 2);
      statsCollector = ps.statsCollector;
      statsCollector.FlushInterval = 1000;
      statsService = new _Service("svcbus_stats", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE, 3 * 1024 * 1024, 1 * 1024 * 1024);
      statsProducer = new _Producer(ps, "Stats_Producer", statsService);
      statsProducer.RequestTimeout = 4000;
      try
      {
        while (statsProducer.waitAndTake() != null) {}
      }
      catch (ProducerException e)
      {
        if (e.ErrorCode != svc_bus_producer_err_t.SERVICE_BUS_PRODUCER_WAIT_EXCEEDED) throw;
      }
    }

    [TearDown]
    public void TearDown()
    {
      statsProducer.Dispose();
      ps.Dispose();
      GC.Collect();
    }

    [Test]
    public void TestStatsCollectorCreated()
    {
      Assert.AreNotEqual(null, statsCollector);
    }

    [Test]
    public void TestReportStat()
    {
      bool found = false;
      statsCollector.SendReport("TEST_TAG", "TEST_VALUE");
      while (true)
      {
        Message msg;
        try
        {
          msg = statsProducer.wait();
        }
        catch (ProducerException)
        {
          break;
        }
        Assert.IsTrue(statsProducer.take(msg));
        if (found) continue;
        var it = msg.bson.find("params.metrics.0.stat_tags");
        Assert.AreNotEqual(null, it);
        found = ((string)it).Contains("TEST_TAG");
      }
      Assert.IsTrue(found);
    }

    [Test]
    public void TestReportStatByTimeElapsed()
    {
      bool found = false;
      var reportId = statsCollector.ReportInit("TEST_TAG_TIME", "TEST_VALUE_TIME=1", 10000);
      Thread.Sleep(1000);
      statsCollector.ReportCompleteByTimeElapsed(reportId, "HELLO=1");
      while (true)
      {
        Message msg;
        try
        {
          msg = statsProducer.wait();
        }
        catch (ProducerException)
        {
          break;
        }
        Assert.IsTrue(statsProducer.take(msg));
        if (found) continue;
        var it = msg.bson.find("params.metrics.0.stat_tags");
        Assert.AreNotEqual(null, it);
        var itValues = msg.bson.find("params.metrics.0.values");
        Assert.AreNotEqual(null, itValues);
        found = ((string)it).Contains("TEST_TAG_TIME") && 
                ((string)itValues).Contains("elapsed_time=") &&
                ((string)itValues).Contains("TEST_VALUE_TIME=1,HELLO=1");
      }
      Assert.IsTrue(found);
    }

    [Test]
    public void TestReportStatByTimeElapsedTimeout()
    {
      bool found = false;
      var reportId = statsCollector.ReportInit("TEST_TAG_TIME_TIMEOUT", "TEST_VALUE_TIME=1", 10000);
      Thread.Sleep(1000);
      statsCollector.ReportCompleteTimeout(reportId, "HELLO=1");
      while (true)
      {
        Message msg;
        try
        {
          msg = statsProducer.wait();
        }
        catch (ProducerException)
        {
          break;
        }
        Assert.IsTrue(statsProducer.take(msg));
        if (found) continue;
        var it = msg.bson.find("params.metrics.0.stat_tags");
        Assert.AreNotEqual(null, it);
        var itValues = msg.bson.find("params.metrics.0.values");
        Assert.AreNotEqual(null, itValues);
        found = ((string)it).Contains("TEST_TAG_TIME_TIMEOUT") && 
                ((string)itValues).Contains("elapsed_time=-1") && 
                ((string)itValues).Contains("TEST_VALUE_TIME=1,HELLO=1");
      }
      Assert.IsTrue(found);
    }
    
    [Test]
    public void TestReportStatDisabled()
    {
      bool found = false;
      statsCollector.Enabled = false;
      statsCollector.SendReport("TEST_TAG", "TEST_VALUE");
      while (true)
      {
        Message msg;
        try
        {
          msg = statsProducer.wait();
        }
        catch (ProducerException)
        {
          break;
        }
        Assert.IsTrue(statsProducer.take(msg));
        if (found) continue;
        var it = msg.bson.find("params.metrics.0.stat_tags");
        Assert.AreNotEqual(null, it);
        found = ((string)it).Contains("TEST_TAG");
      }
      Assert.IsFalse(found);
    }

    [Test]
    public void TestCancelReportStat()
    {
      var reportId = statsCollector.ReportInit("TEST_TAG_CANCEL", "TEST_VALUE_TIME=1", 10000);
      statsCollector.CancelReport(reportId);
      statsCollector.ReportCompleteByTimeElapsed(reportId);
      Thread.Sleep(1500);
      try
      {
        statsProducer.wait();
        Assert.Fail("Code should never reach this point");
      }
      catch (ProducerException e)
      {
        Assert.AreEqual(svc_bus_producer_err_t.SERVICE_BUS_PRODUCER_WAIT_EXCEEDED, e.ErrorCode);
      }
    }
  }
}
