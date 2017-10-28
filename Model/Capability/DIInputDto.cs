using Sovos.Template.Model.Repositories;

namespace Sovos.Template.Model.Capability
{
    public class DIInputDto
    {
        public IRepositoryFactory RepositoryFactory { get; set; }
        public int MyParameter { get; set; }
    }
}
