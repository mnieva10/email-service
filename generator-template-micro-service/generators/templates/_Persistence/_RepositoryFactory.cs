using System.Collections.Specialized;
using Sovos.<%= namespace %>.Model.Repositories;
using IBatisNet.DataMapper;
using IBatisNet.DataMapper.Configuration;

namespace Sovos.<%= namespace %>.Persistence
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly ISqlMapper _mapper;

        public RepositoryFactory(string sqlMapPath, string connectionString)
        {
            _mapper = CreateMapper(sqlMapPath, connectionString);
        }

        public string DefaultDomain { get; set; }
        public ISecurityAnswerRepository CreateSecurityAnswerRepository() { return new SecurityAnswerRepository { Mapper = _mapper }; }

        public void BeginTransaction()
        {
            if (!_mapper.IsSessionStarted) _mapper.BeginTransaction();
        }

        public void Commit()
        {
            if (_mapper.IsSessionStarted) _mapper.CommitTransaction();
        }

        public void RollBack()
        {
            if (_mapper.IsSessionStarted) _mapper.RollBackTransaction();
        }

        private ISqlMapper CreateMapper(string configFile, string connectionString)
        {
            var properties = new NameValueCollection { { "connectionString", connectionString } };
            var mapperBuilder = new DomSqlMapBuilder { ValidateSqlMapConfig = true, Properties = properties };

            return mapperBuilder.Configure(configFile);
        }
    }
}
