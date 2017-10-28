using IBatisNet.DataMapper;

namespace Sovos.<%= namespace %>.Persistence
{
    public class BaseGateway
    {
        protected ISqlMapper mapper;

        public ISqlMapper Mapper
        {
            set { mapper = value; }
        }
    }
}
