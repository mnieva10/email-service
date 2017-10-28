using System;
using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.LongRunningJob;
using NUnit.Framework;

namespace ModelUT.LongRunningJob
{
    [TestFixture]
    public class JobTest
    {
        private static readonly JobFactory JobFactory = new JobFactory();

        private Job _mainJob = JobFactory.CreateJob(10, JobStatus.InProgress);
        private Job _failedJob = JobFactory.CreateJob(20, JobStatus.Failed);
        private Job _cancelledJob = JobFactory.CreateJob(30, JobStatus.Cancelled);
        private Job _completeJob = JobFactory.CreateJob(40, JobStatus.Complete);
        private Job _newJob = JobFactory.CreateJob(50, JobStatus.New);

        [SetUp]
        public void SetUp()
        {
            _cancelledJob.AddJob(JobFactory.CreateJob(31, JobStatus.Complete));
            _cancelledJob.AddJob(JobFactory.CreateJob(32, JobStatus.InProgress));
        }

        [TearDown]
        public void TearDown()
        {
            _mainJob.Steps = new List<BaseJob>();
            _mainJob.Status = (int)JobStatus.InProgress;
        }

        [Test]
        public void AddJob()
        {
            Assert.AreEqual(0, _mainJob.Steps.Count);
            _mainJob.AddJob(_completeJob);
            Assert.AreEqual(1, _mainJob.Steps.Count);
            _mainJob.AddJob(_newJob);
            Assert.AreEqual(2, _mainJob.Steps.Count);
            _mainJob.AddJob(_completeJob);
            Assert.AreEqual(2, _mainJob.Steps.Count);
        }

        [Test]
        public void RemoveJob()
        {
            Assert.AreEqual(0, _mainJob.Steps.Count);
            _mainJob.AddJob(_completeJob);
            Assert.AreEqual(1, _mainJob.Steps.Count);
            _mainJob.AddJob(_newJob);
            Assert.AreEqual(2, _mainJob.Steps.Count);

            _mainJob.RemoveJob(60);
            Assert.AreEqual(2, _mainJob.Steps.Count);
            _mainJob.RemoveJob(50);
            Assert.AreEqual(1, _mainJob.Steps.Count);
            _mainJob.RemoveJob(40);
            Assert.AreEqual(0, _mainJob.Steps.Count);
        }

        [Test]
        public void GetJob()
        {
            _mainJob.AddJob(_completeJob);
            _mainJob.AddJob(_newJob);
            _mainJob.AddJob(_cancelledJob);

            var found = _mainJob.GetJob(60);
            Assert.IsNull(found);
            
            found = _mainJob.GetJob(50);
            Assert.IsNotNull(found);
            Assert.AreEqual(JobStatus.New, (JobStatus)found.Status);

            found = _mainJob.GetJob(30);
            Assert.IsNotNull(found);
            Assert.AreEqual(2, ((Job)found).Steps.Count);

            found = _mainJob.GetJob(32);
            Assert.IsNotNull(found);
            Assert.AreEqual(JobStatus.InProgress, (JobStatus)found.Status);

            found = _mainJob.GetJob(35);
            Assert.IsNull(found);
        }

        [Test]
        public void IsExpired()
        {
            Assert.IsFalse(_mainJob.IsExpired());
            _mainJob.ExpiresOn = DateTime.Now.AddHours(-1);
            Assert.IsTrue(_mainJob.IsExpired());
        }
    }

    public class JobFactory
    {
        public Job CreateJob(int id, JobStatus status)
        {
            return new Job(id, 0, 0, (int) status, DateTime.Now, DateTime.Now, DateTime.MaxValue, "UT_SVC", "UT_REQUEST", "UT_COMMAND");
        }
    }
}
