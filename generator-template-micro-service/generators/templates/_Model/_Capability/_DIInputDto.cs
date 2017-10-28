using Sovos.<%= namespace %>.Model.Repositories;

namespace Sovos.<%= namespace %>.Model.Capability
{
    public class DIInputDto
    {
        public IRepositoryFactory RepositoryFactory { get; set; }
        public int MyParameter { get; set; }
    }
}
