using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.Operation;
using Sovos.Template.Persistence;
using NUnit.Framework;

namespace PersistenceUT
{
    [TestFixture]
    public class SecurityAnswerRepositoryTest : BaseTest
    {
        private readonly SecurityAnswerRepository _repository = new SecurityAnswerRepository { Mapper = Mapper.Instance };
        private static string _user1 = "AUTH_UT1";
        private SecurityAnswer _answer1 = new SecurityAnswer(_user1, 1, "answer1.1") { Schema = TestSchema };
        private SecurityAnswer _answer5 = new SecurityAnswer(_user1, 5, "answer1.5") { Schema = TestSchema };
        private static string _user2 = "AUTH_UT2";
        private SecurityAnswer _answer2 = new SecurityAnswer(_user2, 1, "answer2.1") { Schema = TestSchema };
        private SecurityAnswer _answer3 = new SecurityAnswer(_user2, 2, "answer2.2") { Schema = TestSchema };
        private SecurityAnswer _answer4 = new SecurityAnswer(_user2, 3, "answer2.3") { Schema = TestSchema };
        private List<SecurityAnswer> _answers = new List<SecurityAnswer>();

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _repository.Save(_answer1);
            _answers.Add(_answer1);
            _repository.Save(_answer5);
            _answers.Add(_answer5);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            foreach (var answer in _answers)
                _repository.Delete(answer);
        }

        [Test]
        public void Find()
        {
            Assert.IsNotNull(_repository.Find(_answer1));
            Assert.IsNull(_repository.Find(_answer2));
        }

        [Test]
        public void SaveAdd()
        {
            Assert.IsNull(_repository.Find(_answer3));
            _repository.Save(_answer3);
            _answers.Add(_answer3);
            Assert.IsNotNull(_repository.Find(_answer3));
        }

        [Test]
        public void SaveUpdate()
        {
            var answer1 = _repository.Find(_answer1);
            Assert.IsNotNull(answer1);
            Assert.AreEqual("answer1.1", answer1.Answer);
            _answer1.Answer = "new answer1.1";
            _repository.Save(_answer1);
            answer1 = _repository.Find(_answer1);
            Assert.IsNotNull(answer1);
            Assert.AreEqual(_answer1.Answer, answer1.Answer);
        }

        [Test]
        public void Delete()
        {
            Assert.IsNull(_repository.Find(_answer4));
            _repository.Save(_answer4);
            Assert.IsNotNull(_repository.Find(_answer4));
            _repository.Delete(_answer4);
            Assert.IsNull(_repository.Find(_answer4));
        }
    }
}
