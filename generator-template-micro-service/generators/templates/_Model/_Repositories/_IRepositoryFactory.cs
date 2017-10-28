namespace Sovos.<%= namespace %>.Model.Repositories
{
    public interface IRepositoryFactory
    {
        ISecurityAnswerRepository CreateSecurityAnswerRepository();

        void BeginTransaction();
        void Commit();
        void RollBack();
    }
}
