using System;
using System.Diagnostics;
using NUnit.Framework;
using Sovos.SvcBus;
using Sovos.SvcBus.Common.Model.Capability;
using Sovos.SvcBus.Common.Model.Strategies;

namespace ModelUT.Services
{
    public class TestUsingDispatcher
    {
        public const string MongoConnectionString = "mongodb://localhost:27017/testdb";

        protected Scope scope;
        protected PipeService ps;

        public virtual void SetUp()
        {
            scope = new Scope();
            ps = scope.newPipeService(MongoConnectionString);
        }

        public virtual void TearDown()
        {
            scope.Dispose();
        }
    }

    [TestFixture]
    public class DestinationDispatcherConfigurableTest : TestUsingDispatcher
    {
        private DispatchInterfaceConsumer _dic;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            var dispatcherSvc = Builder.newService("DestinationDispatcherConfigurableTest",
                svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);

            var dispatcher = scope.newDispatcher("DestinationDispatcherConfigurableTest", dispatcherSvc, ps,
                typeof(DispatchInterface));
            dispatcher.Logger = new SimpleConsoleLogger();
            dispatcher.RequestProcessingStrategy = new DestinationRequestProcessingStrategy();
            dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);

            _dic = scope.newDispatchInterfaceConsumer(ps, "DestinationDispatcherConfigurableTest",
                dispatcherSvc,
                svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public bool Ping(string machineName = "", int pid = -1)
        {
            var req = new DestinationDispatcherConfigurable();
            if (machineName != "")
                req.MachineName = machineName;
            if (pid != -1)
                req.Pid = pid;
            try
            {
                _dic.SendAndWait("Ping", req);
                return true;
            }
            catch
            {
                return false;
            }
        }

        [Test]
        public void PingRequestWithDifferentPid_ShouldSkip()
        {
            Assert.False(Ping(Environment.MachineName, Process.GetCurrentProcess().Id + 1));
        }

        [Test]
        public void PingRequestWithDifferentMachineName_ShouldSkip()
        {
            Assert.False(Ping("Invalid machine name"));
        }

        [Test]
        public void PingRequestWithMatchingMachineNameAndPid_ShouldSendResponse()
        {
            Assert.True(Ping(Environment.MachineName, Process.GetCurrentProcess().Id));
        }
    }
}
