using System.Threading;
using NUnit.Framework;
using Sovos.DependencyCache;
using Sovos.SvcBus;
using Sovos.SvcBus.Common.Model.Services;

namespace ModelUT.DepCache
{
    public class DummyObject
    {
        public int I { get; set; }
        public string Text { get; set; }
    }

    public class DepCacheBuilderDummy : IDepCacheable
    {
        public object CreateObj(string[] args, Dependencies deps)
        {
            var result = new DummyObject {I = args.Length};
            foreach (var s in args)
                result.Text += s;
            deps.AddDependency("dep1");
            deps.AddDependency("dep2");
            return result;
        }

        public void DestroyObj(object obj)
        {
        }
    }

    [TestFixture]
    public class DepCacheInvalidatorTest
    {
        private const string MongoConnectionString = "mongodb://localhost:27017/testdb";
        private readonly string[] _args = { "11", "22", "33" };

        private PipeService _pipe;
        private DepCacheInvalidatorListener _listener;
        private DepCacheInvalidatorSender _sender;
        private DepCacheBase _cache;
        private Scope _scope;

        [SetUp]
        public void SetUp()
        {
            _scope = new Scope();
            _pipe = _scope.newPipeService(MongoConnectionString);
            _listener = new DepCacheInvalidatorListener(_pipe);
            _sender = new DepCacheInvalidatorSender(_pipe);
            _cache = new DepCachePooled<object>(new DepCacheBuilderDummy(), 0, (int)Flags.DEP_CACHE_NONE);
        }

        [TearDown]
        public virtual void TearDown()
        {
            _sender.Dispose();
            _listener.Dispose();
            if (_listener.FatalException != null)
                throw _listener.FatalException;

            _scope.Dispose();
            _cache.Dispose();
        }

        [Test]
        public void TestListenerCreate()
        {
            var listener = new DepCacheInvalidatorListener(_pipe);
            listener.Dispose();
            Assert.IsNotNull(listener, "No exceptions should be raised");
        }

        [Test]
        public void TestSenderCreate()
        {
            var sender = new DepCacheInvalidatorSender(_pipe);
            sender.Dispose();
            Assert.IsNotNull(sender, "No exceptions should be raised");
        }

        [Test]
        public void TestListenerAddDepCache()
        {
            _listener.Add(_cache);
            Assert.True(true, "No exceptions should be raised");
        }

        //[Test]
        public void TestMessageAndInvalidation()
        {
            _listener.Add(_cache);
            // Acquire and release object, then send a msg to the listener to invalidate this object
            // Give it some time and check the object has been invalidated (a new one should be acquired)
            var obj1 = _cache.Acquire(_args);
            _cache.Release(obj1);
            _sender.RequestInvalidation(new [] { "OtherDep", "dep1" });

            Thread.Sleep(2000);
            var obj2 = _cache.Acquire(_args);
            _cache.Release(obj2);

            Assert.AreNotEqual(obj1, obj2, "The two objects should be different");
        }
    }
}
