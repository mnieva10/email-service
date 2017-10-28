using Sovos.SvcBus.Common.Model.Operation;
using Sovos.Template.Model.Exceptions;
using Sovos.Template.Model.Repositories;

namespace Sovos.Template.Model.Services
{
    public class TemplateService : ITemplateService
    {
        private ISecurityAnswerRepository SecurityAnswerRepository { get; set; }
        
        public TemplateService(IRepositoryFactory factory)
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
