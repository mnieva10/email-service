using System.Threading;
using NUnit.Framework;
using Sovos.SvcBus.Common.Model.Infrastructure.Pooling;

namespace ModelUT.Infrastructure.Pooling
{
    [TestFixture]
    public class PoolTest
    {
        private Pool<string> _testPool;
        private static int itemIndex = 0;

        private static string Factory(object[] parameters = null)
        {
            return string.Format("String: {0}", ++itemIndex);
        }

        [SetUp]
        public void SetUp()
        {
            itemIndex = 0;
            _testPool = new Pool<string>(Factory, 60000000);
        }

        [Test]
        public void InvalidFactory()
        {
            Assert.Throws<PoolFactoryException>(() => new Pool<string>(null, 100));
        }

        [Test]
        public void AcquireNewItems()
        {
            Assert.AreEqual(0, _testPool.ItemStore.Count);

            var item = _testPool.Acquire();
            Assert.IsNotNull(item);
            Assert.IsTrue(item.Contains("String: 1"));
            Assert.AreEqual(0, _testPool.ItemStore.Count);

            item = _testPool.Acquire();
            Assert.IsNotNull(item);
            Assert.IsTrue(item.Contains("String: 2"));
            Assert.AreEqual(0, _testPool.ItemStore.Count);
        }

        [Test]
        public void AcquireExisting()
        {
            Assert.AreEqual(0, _testPool.ItemStore.Count);

            var item = Factory();
            Assert.IsNotNull(item);
            Assert.IsTrue(item.Contains("String: 1"));
            _testPool.Release(item);
            Assert.AreEqual(1, _testPool.ItemStore.Count);

            item = Factory();
            Assert.IsNotNull(item);
            Assert.IsTrue(item.Contains("String: 2"));
            _testPool.Release(item);
            Assert.AreEqual(2, _testPool.ItemStore.Count);

            item = _testPool.Acquire();
            Assert.IsNotNull(item);
            Assert.IsTrue(item.Contains("String: 2"));
            Assert.AreEqual(1, _testPool.ItemStore.Count);
        }

        [Test]
        public void Release()
        {
            Assert.AreEqual(0, _testPool.ItemStore.Count);

            var item = _testPool.Acquire();
            Assert.IsNotNull(item);
            Assert.IsTrue(item.Contains("String: 1"));
            Assert.AreEqual(0, _testPool.ItemStore.Count);

            _testPool.Release(item);
            Assert.AreEqual(1, _testPool.ItemStore.Count);

            item = Factory();
            Assert.IsNotNull(item);
            Assert.IsTrue(item.Contains("String: 2"));
            Assert.AreEqual(1, _testPool.ItemStore.Count);

            _testPool.Release(item);
            Assert.AreEqual(2, _testPool.ItemStore.Count);
        }

        [Test]
        public void PurgeAll()
        {
            Assert.AreEqual(0, _testPool.ItemStore.Count);
            for (var i = 0; i < 5; i++)
            {
                var item = Factory();
                _testPool.Release(item);
            }
            Assert.AreEqual(5, _testPool.ItemStore.Count);

            _testPool.Purge(true);
            Assert.AreEqual(0, _testPool.ItemStore.Count);
        }

        [Test]
        public void RunTtlCheck()
        {
            const int n = 100;
            Assert.AreEqual(0, _testPool.ItemStore.Count);
            for (var i = 0; i < n; i++)
            {
                var item = Factory();
                _testPool.Release(item);
                Thread.Sleep(100);
            }
            Assert.AreEqual(n, _testPool.ItemStore.Count);
            _testPool.Purge(false);

            Assert.Less(_testPool.ItemStore.Count, n);
        }

        [Test]
        public void RunTtlCheckZeroTtl()
        {
            _testPool = new Pool<string>(Factory, 0);
            const int n = 100;
            Assert.AreEqual(0, _testPool.ItemStore.Count);
            for (var i = 0; i < n; i++)
            {
                var item = Factory();
                _testPool.Release(item);
                Thread.Sleep(100);
            }
            Assert.AreEqual(n, _testPool.ItemStore.Count);
            _testPool.Purge(false);

            Assert.AreEqual(_testPool.ItemStore.Count, n);
        }

        [Test]
        public void RunTtlCheckLowTtl()
        {
            _testPool = new Pool<string>(Factory, 1);
            const int n = 100;
            Assert.AreEqual(0, _testPool.ItemStore.Count);
            for (var i = 0; i < n; i++)
            {
                var item = Factory();
                _testPool.Release(item);
            }
            Assert.AreEqual(n, _testPool.ItemStore.Count);
            Thread.Sleep(100);

            _testPool.Purge(false);

            Assert.AreEqual(_testPool.ItemStore.Count, 0);
        }
    }
}