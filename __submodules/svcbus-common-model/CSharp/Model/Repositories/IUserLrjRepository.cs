using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.Operation;

namespace Sovos.SvcBus.Common.Model.Repositories
{
    public interface IUserLrjRepository
    {
        void Add(UserLrj userLrj);
        List<UserLrj> FindClientJobs(string seghash);
        List<UserLrj> FindUserJobs(string username);
    }
}
