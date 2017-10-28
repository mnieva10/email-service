using System;
using NUnit.Framework;
using ModelUT.Services;
using Sovos.SvcBus;
using Sovos.SvcBus.Common.Model.Capability;
using Sovos.SvcBus.Common.Model.Strategies;
using DotNetThread = System.Threading.Thread;

namespace ModelUT.Strategies
{
    public class MutexedRequestProcessingTestDispatchInterface : DispatchInterface
    {
        private static readonly object LockObj = new object();

        public MutexedRequestProcessingTestDispatchInterface(object userData) : base(userData)
        {
        }

        public object SomeJob(Message msg)
        {
            MutexedRequestProcessingStrategyTest.TimeProcessed = DateTime.Now;
            lock (LockObj)
            {
                MutexedRequestProcessingStrategyTest.ProcessedCount++;
            }
            return null;
        }
    }

    [TestFixture]
    public class MutexedRequestProcessingStrategyTest : TestUsingDispatcher, IMutexInfoProvider
    {
        private Service _dispatcherService;
        private Dispatcher _dispatcher;
        private DispatchInterfaceConsumer _dic;
        private Mutex _mutex;
        public static DateTime TimeProcessed { get; set; }
        public static int ProcessedCount { get; set; }

        public MutexedRequestProcessingStrategyTest()
        {
            TimeProcessed = DateTime.MinValue;
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mutex = Builder.newMutex(ps, "hello", GetServiceName(null));
            _mutex.remove();
            _mutex = Builder.newMutex(ps, "hello", GetServiceName(null));
            _dispatcherService = Builder.newService("TestMutexedRequestProcesingStrategy", svc_bus_queue_mode_t.SERVICE_BUS_VOLATILE);

            _dispatcher = new Dispatcher("MutexRequestProcessingStrategyTest", _dispatcherService, ps,
                typeof (MutexedRequestProcessingTestDispatchInterface))
            {
                RequestProcessingStrategy = new MutexedRequestProcessingStrategy(ps, this, null)
            };
            _dispatcher.Logger = new SimpleConsoleLogger();

            _dispatcher.Start(2, DispatchMode.SingletonDispatchInterface);

            _dic = new DispatchInterfaceConsumer(ps, "MutexRequestProcessingStrategyTest", _dispatcherService, svc_bus_consumer_options_t.SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE);
        }

        [TearDown]
        public override void TearDown()
        {
          _dic.Dispose();
          _dispatcher.Dispose();
          _mutex.Dispose();
          _mutex = null;
          base.TearDown();
        }

        public string GetMutexName(Message msg)
        {
            var retVal = string.Empty;

            if (msg != null)
            {
                object dataDto = new DataDto();
                DispatchInterface.DeserializeRequest(msg, ref dataDto);
                var deserialized = (DataDto) dataDto;
                if (!string.IsNullOrEmpty(deserialized.DataString))
                    retVal = deserialized.DataString;
            }

            return retVal;
        }

        public string GetServiceName(Message msg)
        {
            return "TEST_MODEL_SERVICE";
        }

        [Test]
        public void TestMutexedCommand()
        {
            var mutexState = _mutex.acquire();
            _dic.Consumer.ResponseTimeout = 10000;
            var startTime = DateTime.Now;
            var objToSend = new DataDto{ DataString = "hello"};

            _dic.Send("SomeJob", objToSend);
            _dic.Send("SomeJob", objToSend);

            System.Threading.Thread.Sleep(2000);
            _mutex.release();

            while (TimeProcessed == DateTime.MinValue)
            {
                System.Threading.Thread.Sleep(1);
            }

            Assert.Greater(Math.Abs((TimeProcessed - startTime).TotalMilliseconds), 2000, "Time processing was too fast");


            while (ProcessedCount < 2)
                System.Threading.Thread.Sleep(1);
            
            Assert.AreEqual(2, ProcessedCount);
        }

        [Test]
        public void TestNonMutexedCommand()
        {
            _mutex.acquire();
            _dic.Consumer.ResponseTimeout = 10000;
            var startTime = DateTime.Now;
            var dataDto = new DataDto();
            _dic.Send("SomeJob", dataDto);
            DotNetThread.Sleep(20);

            while (TimeProcessed == DateTime.MinValue)
            {
                DotNetThread.Sleep(1);
            }

            Assert.LessOrEqual(Math.Abs((TimeProcessed - startTime).Milliseconds), 100, "Time processing was too slow");
        }
    }
}
