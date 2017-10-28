using Sovos.SvcBus.Common.Model.Services.Interfaces;

namespace Sovos.SvcBus.Common.Model.Capability
{
    public class DIBaseInput
    {
        public object RepositoryFactory { get; set; }
        public IRemoteMicroservice Microservice { get; set; }
    }
}