using Sovos.SvcBus.Common.Model.Infrastructure.Pooling;
using NUnit.Framework;

namespace ModelUT.Infrastructure.Pooling
{
    [TestFixture]
    public class PoolsTest
    {
        private Pools<string> _testPools;
        private static int itemIndex = 0;

        private static string Factory(object[] parameters = null)
        {
            return string.Format("String: {0}", ++itemIndex);
        }

        [SetUp]
        public void SetUp()
        {
            itemIndex = 0;
            _testPools = new Pools<string>(Factory);
        }

        [Test]
        public void InvalidFactory()
        {
            Assert.Throws<PoolFactoryException>(() => new Pools<string>(null));
        }

        [Test]
        public void AquireNewPool()
        {
            Assert.AreEqual(0, _testPools.PoolMap.Count);

            var pool = _testPools.Acquire("N1");
            Assert.IsNotNull(pool);
            Assert.AreEqual(1, _testPools.PoolMap.Count);
            Assert.AreEqual(0, pool.ItemStore.Count);
            var poolItem = pool.Acquire();
            pool.Release(poolItem);
            Assert.AreEqual(1, pool.ItemStore.Count);

            pool = _testPools.Acquire("N2");
            Assert.IsNotNull(pool);
            Assert.AreEqual(2, _testPools.PoolMap.Count);
        }

        [Test]
        public void AquireExisting()
        {
            Assert.AreEqual(0, _testPools.PoolMap.Count);

            var newPool = new Pool<string>(Factory, 60000);
            _testPools.PoolMap.Add("N1", newPool);
            Assert.AreEqual(0, newPool.ItemStore.Count);
            var item = Factory();
            newPool.Release(item);
            item = Factory();
            newPool.Release(item);
            Assert.AreEqual(2, newPool.ItemStore.Count);
            Assert.AreEqual(1, _testPools.PoolMap.Count);

            newPool = new Pool<string>(Factory, 60000);
            _testPools.PoolMap.Add("N2", newPool);
            Assert.AreEqual(2, _testPools.PoolMap.Count);

            var foundPool = _testPools.Acquire("N1");
            Assert.AreEqual(2, foundPool.ItemStore.Count);
        }
    }
}
