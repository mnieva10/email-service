using System.Threading;
using Sovos.SvcBus;
using Sovos.SvcBus.Common.Model.Strategies;
using NUnit.Framework;

namespace ModelUT.Services
{
    internal class PingStrategyTestDispatchInterface : DispatchInterface
    {
        public static ManualResetEvent FinishLongJobEvent = new ManualResetEvent(false);

        public PingStrategyTestDispatchInterface(object userData) : base(userData) {}

        public object LongJob(Message msg)
        {
            FinishLongJobEvent.WaitOne();
            return true;
        }
    }

    [TestFixture]
    public class PingRequestStrategyTest : TestUsingDispatcher
    {
        private DispatchInterfaceConsumer _dic;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            PingStrategyTestDispatchInterface.FinishLongJobEvent.Reset();

            var dispatcherSvc = Builder.newService("PingRequestStrategyTest",
                svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);

            var dispatcher = scope.newDispatcher("PingRequestStrategyTest", dispatcherSvc, ps,
                typeof (PingStrategyTestDispatchInterface));
            dispatcher.YieldTimeout = 0;
            dispatcher.RequestProcessingStrategy = new PingRequestStrategy();
            dispatcher.Logger = new SimpleConsoleLogger();
            dispatcher.Start(1, DispatchMode.SingletonDispatchInterface);

            _dic = scope.newDispatchInterfaceConsumer(ps, "PingRequestStrategyTest",
                dispatcherSvc,
                svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
        }

        [TearDown]
        public override void TearDown()
        {
            PingStrategyTestDispatchInterface.FinishLongJobEvent.Set();
            base.TearDown();
        }

        [Test]
        public void PingRequestReceivedWhenAllWorkersBusy_ShouldRespondFromMainThread()
        {
            _dic.Logger = new SimpleConsoleLogger();
            _dic.Consumer.ResponseTimeout = 500;
            _dic.Send("LongJob", new object());
            Assert.Throws<ConsumerException>(() => _dic.SendAndWait("LongJob", new object()));
            Assert.DoesNotThrow(() => _dic.SendAndWait("Ping", new object()));
        }
    }
}
