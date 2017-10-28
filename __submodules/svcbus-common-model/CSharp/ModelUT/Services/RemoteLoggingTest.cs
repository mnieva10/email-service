using System.Linq;
using NUnit.Framework;
using Sovos.SvcBus.Common.Model.Exceptions;
using Sovos.SvcBus.Common.Model.Infrastructure.Logging;
using Sovos.SvcBus.Common.Model.Operation;
using Sovos.SvcBus.Common.Model.Services;
using Sovos.SvcBus.Common.Model.Services.Interfaces;
using RemoteMicroservice = ModelUT.Services.Stubs.RemoteMicroservice;

namespace ModelUT.Services
{
    [TestFixture]    
    class RemoteLoggingTest
    {
        private IRemoteLoggingService _service;
        private RemoteMicroservice _remoteMicroservice;

        [SetUp]
        public void SetUp()
        {
            _remoteMicroservice = new RemoteMicroservice();
            _service = new RemoteLoggingService(_remoteMicroservice);
        }

        [Test]
        public void AddLogEntry()
        {
            var user = new User
            {
                Schema = "testSchema",
                Domain = "testDomain",
                IPAddress = "testIP",
                Username = "testUsername"
            };
            _service.AddLogEntry(user, "test description", LogCodes.LOG_LOGIN);

            var logEntry = (LogEntry)_remoteMicroservice.RequestsSent.Last().Item2;
            Assert.AreEqual(_remoteMicroservice.RequestsSent.Last().Item1, "AddLogEntry");
            Assert.AreEqual(logEntry.Schema, "testSchema");
            Assert.AreEqual(logEntry.Domain, "testDomain");
            Assert.AreEqual(logEntry.IPAddress, "testIP");
            Assert.AreEqual(logEntry.Username, "TESTUSERNAME");
            Assert.AreEqual(logEntry.LogCode, (int)LogCodes.LOG_LOGIN);
        }

        [Test]
        [ExpectedException(typeof(InvalidParametersException))]
        public void AddLogEntryInvalidUser()
        {
            _service.AddLogEntry(null, "test description", LogCodes.LOG_LOGIN);
        }

        [Test]
        public void FindMostRecentLogEntryByLogCode()
        {
            var user = new User
            {
                Schema = "testSchema",
                Username = "testUsername"
            };
            _service.FindMostRecentLogEntryByLogCode(user, LogCodes.LOG_LOGIN);

            var logEntry = (LogEntry)_remoteMicroservice.RequestsSent.Last().Item2;
            Assert.AreEqual(_remoteMicroservice.RequestsSent.Last().Item1, "FindMostRecentLogEntryByLogCode");
            Assert.AreEqual(logEntry.Schema, "testSchema");
            Assert.AreEqual(logEntry.Username, "TESTUSERNAME");
            Assert.AreEqual(logEntry.LogCode, (int)LogCodes.LOG_LOGIN);
        }

        [Test]
        [ExpectedException(typeof(InvalidParametersException))]
        public void FindMostRecentLogEntryByLogCodeInvalidUser()
        {
            _service.FindMostRecentLogEntryByLogCode(null, LogCodes.LOG_GENTOKENADMIN);
        }

        [Test]
        public void FindMostRecentPasswordResetTokenLogCode()
        {
            var user = new User
            {
                Schema = "testSchema",
                Username = "testUsername"
            };
            _service.FindMostRecentPasswordResetTokenLogCode(user);

            var logEntry = (LogEntry)_remoteMicroservice.RequestsSent.Last().Item2;
            Assert.AreEqual(_remoteMicroservice.RequestsSent.Last().Item1, "FindMostRecentPasswordResetTokenLogCode");
            Assert.AreEqual(logEntry.Schema, "testSchema");
            Assert.AreEqual(logEntry.Username, "TESTUSERNAME");
        }

        [Test]
        [ExpectedException(typeof(InvalidParametersException))]
        public void FindMostRecentPasswordResetTokenLogCodeInvalidUser()
        {
            _service.FindMostRecentPasswordResetTokenLogCode(null);
        }
    }
}