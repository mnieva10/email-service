using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.Operation;

namespace Sovos.SvcBus.Common.Model.Repositories
{
    public interface IEnvironmentRepository
    {
        string FindSchemaByDomain(string domain);
        Environment FindEnvironmentByDomain(string domain);
        Environment FindEnvironmentBySchema(string schema, string connector);
        Environment FindEnvironmentBySchemaBatchMode(string schema); // FindEnvironmentBySchemaBatchMode is used for those instance where cnaccess is not a view
        int FindMaxScheduleId();
        List<string> FindDomainsByConnector(string connector);
        string FindConnectionStringByDomain(string domain);
        string FindConnectionStringBySchema(string schema, string connector);
    }
}
