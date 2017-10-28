using Sovos.SvcBus;
using NUnit.Framework;

namespace SvcBusTests
{
  [TestFixture, GTestStyleConsoleOutput]
  class ServiceTest
  {
    [Test]
    public void ParamsProvided_GettersReturnsSame()
    {
      const string name = "service_test";
      const svc_bus_queue_mode_t mode = svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE;
      const uint volatileSize = 1024 * 1024;
      const uint responsePipeSize = 1024;

      var svc = new _Service(name, mode, volatileSize, responsePipeSize);
      Assert.AreEqual(name, svc.Name);
      Assert.AreEqual(mode, svc.Mode);
      Assert.AreEqual(volatileSize, svc.VolatileSize);
      Assert.AreEqual(responsePipeSize, svc.ResponsePipeSize);
    }

    [Test]
    public void InvalidNameProvided_ShouldThrow()
    {
      try
      {
        Builder.newService("", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);
        Assert.Fail("ServiceException expected");
      }
      catch (ServiceException e)
      {
        Assert.AreEqual((int)service_err.NULL_SERVICE_NAME, (int)e.ErrorCode);
        Assert.AreEqual("constructor [1 : attempted to initialize SvcBusService object with NULL name or blank string]", e.Message);
      }
    }
  }
}
