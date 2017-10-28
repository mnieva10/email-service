using Sovos.<%= namespace %>.Model.Repositories;

namespace ModelUT.Services.Stubs
{
    public class RepositoryFactory: IRepositoryFactory
    {
        public string DefaultDomain { get; set; }

        public ISecurityAnswerRepository CreateSecurityAnswerRepository()
        {
            return new SecurityAnswerRepository();
        }

        public void BeginTransaction()
        {
        }

        public void Commit()
        {
        }

        public void RollBack()
        {
        }
    }
}
