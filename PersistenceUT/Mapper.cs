using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using Sovos.Crypt.Model.Services;
using IBatisNet.DataMapper;
using IBatisNet.DataMapper.Configuration;

namespace PersistenceUT
{
    public class Mapper
    {
        private static string connString;

        public static string ConnectionString
        {
            get { return connString ?? LoginUtility.Decrypt(ConfigurationManager.AppSettings["iBatis.ConnectionString"]); }
            set { connString = value; }
        }

        public static ISqlMapper Instance
        {
            get { return CreateMapper(ConnectionString); } 
        }

        private static ISqlMapper CreateMapper(string connectionString)
        {
            SetApplicationBase();
            var properties = new NameValueCollection { { "connectionString", connectionString } };
            var mapperBuilder = new DomSqlMapBuilder { ValidateSqlMapConfig = true, Properties = properties };
            return mapperBuilder.Configure(@"config/sqlMap.config");
        }

        private static void SetApplicationBase()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var applicationBase = assembly.CodeBase.Substring(0, assembly.CodeBase.Length - assembly.ManifestModule.Name.Length);
            AppDomain.CurrentDomain.SetData("APPBASE", applicationBase);
        }
    }

}
