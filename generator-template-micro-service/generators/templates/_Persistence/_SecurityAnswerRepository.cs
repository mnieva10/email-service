using Sovos.SvcBus.Common.Model.Operation;
using Sovos.<%= namespace %>.Model.Repositories;

namespace Sovos.<%= namespace %>.Persistence
{
    public class SecurityAnswerRepository : BaseGateway, ISecurityAnswerRepository
    {
        public SecurityAnswer Find(SecurityAnswer securityAnswer)
        {
            return mapper.QueryForObject<SecurityAnswer>("FindSecurityAnswer", securityAnswer);
        }

        public void Save(SecurityAnswer securityAnswer)
        {
            var answer = Find(securityAnswer);
            if (answer != null)
                mapper.Update("UpdateSecurityAnswer", securityAnswer);
            else
                mapper.Insert("AddSecurityAnswer", securityAnswer);
        }

        public void Delete(SecurityAnswer securityAnswer)
        {
            mapper.Delete("DeleteSecurityAnswer", securityAnswer);
        }
    }
}
