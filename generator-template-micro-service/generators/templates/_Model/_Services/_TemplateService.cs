using Sovos.SvcBus.Common.Model.Operation;
using Sovos.<%= namespace %>.Model.Exceptions;
using Sovos.<%= namespace %>.Model.Repositories;

namespace Sovos.<%= namespace %>.Model.Services
{
    public class <%= namespace %>Service : I<%= namespace %>Service
    {
        private ISecurityAnswerRepository SecurityAnswerRepository { get; set; }
        
        public <%= namespace %>Service(IRepositoryFactory factory)
        {
            if (factory == null)
                throw new RepositoryFactoryException();

            SecurityAnswerRepository = factory.CreateSecurityAnswerRepository();
        }
        
        public SecurityAnswer FindUserAnswer(SecurityAnswer answer)
        {
            if (answer == null || string.IsNullOrEmpty(answer.Username))
                throw new InvalidUsernameException(string.Empty);
            if (string.IsNullOrWhiteSpace(answer.Schema))
                throw new SchemaException();
            if (answer.TablePrefix == null)
                throw new TablePrefixException();

            return SecurityAnswerRepository.Find(answer) ?? new SecurityAnswer();
        }
    }
}
