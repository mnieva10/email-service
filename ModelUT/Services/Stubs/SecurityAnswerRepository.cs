using Sovos.SvcBus.Common.Model.Operation;
using Sovos.Template.Model.Repositories;

namespace ModelUT.Services.Stubs
{
    public class SecurityAnswerRepository : ISecurityAnswerRepository
    {
        public SecurityAnswer Find(SecurityAnswer securityAnswer)
        {
            return new SecurityAnswer(securityAnswer.Username, securityAnswer.QuestionId, "Your stored answer");
        }

        public void Save(SecurityAnswer securityAnswer)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(SecurityAnswer securityAnswer)
        {
            throw new System.NotImplementedException();
        }
    }
}
