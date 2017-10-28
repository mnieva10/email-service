using IBatisNet.DataMapper;

namespace Sovos.Template.Persistence
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
