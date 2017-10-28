using Sovos.SvcBus.Common.Model.Operation;
using Sovos.<%= namespace %>.Model.Exceptions;
using Sovos.<%= namespace %>.Model.Services;
using ModelUT.Services.Stubs;
using NUnit.Framework;

namespace ModelUT.Services
{
    [TestFixture] 
    public class <%= namespace %>ServiceTest
    {
        private I<%= namespace %>Service _service;
        private const int _myParameter = 2;

        [SetUp]
        public void SetUp()
        {
            _service = new <%= namespace %>Service(new RepositoryFactory());
        }

        [Test]
        [ExpectedException(typeof(InvalidUsernameException))]
        public void FindUserAnswerWithInvalidUsername([Values(null, "")] string username)
        {
            _service.FindUserAnswer(new SecurityAnswer(username, 1, string.Empty) { TablePrefix = "DefaultTablePrefix", Schema = "DefaultSchema" });
        }

        [Test]
        [ExpectedException(typeof(SchemaException))]
        public void FindUserAnswerWithInvalidSchema([Values(null, "")] string schema)
        {
            _service.FindUserAnswer(new SecurityAnswer("username", 1, string.Empty) { TablePrefix = "DefaultTablePrefix", Schema = schema });
        }

        [Test]
        [ExpectedException(typeof(TablePrefixException))]
        public void FindUserAnswerWithInvalidTablePrefix()
        {
            _service.FindUserAnswer(new SecurityAnswer("username", 1, string.Empty) { TablePrefix = null, Schema = "DefaultSchema" });
        }

        [Test]
        public void FindUserAnswer()
        {
            var answer = _service.FindUserAnswer(new SecurityAnswer("username", 1, string.Empty) { TablePrefix = "DefaultTablePrefix", Schema = "DefaultSchema" });
            Assert.IsNotNull(answer);   
        }
    }
}
