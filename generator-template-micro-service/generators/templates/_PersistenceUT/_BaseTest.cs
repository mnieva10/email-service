using System.Configuration;
using NUnit.Framework;

namespace PersistenceUT
{
    public class BaseTest
    {
        private static string testSchema;
        public static string TestSchema
        {
            get { return testSchema ?? ConfigurationManager.AppSettings["TestSchema"]; }
            set { testSchema = value; }
        }

        private static string testDomain;
        public static string TestDomain
        {
            get { return testDomain ?? ConfigurationManager.AppSettings["TestDomain"]; }
            set { testDomain = value; }
        }

        [SetUp]
        public virtual void SetUp()
        {
            //Mapper.Instance.BeginTransaction();
        }

        [TearDown]
        public virtual void TearDown()
        {
            //if (Mapper.Instance.IsSessionStarted)
            //    Mapper.Instance.RollBackTransaction();
        }
    }
}
