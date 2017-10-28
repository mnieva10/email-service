using System.Linq;
using Sovos.SvcBus.Common.Model.Capability;
using Sovos.SvcBus.Common.Model.Extensions;
using Sovos.SvcBus.Common.Model.MicroserviceConsole;
using NUnit.Framework;

namespace ModelUT.Services
{
    [TestFixture]
    public class AppSettingsTest
    {
        [Test]
        public void ProcessAppSettings()
        {
            const string command = "/ServiceName:TestService /ThreadCount:6";
            var args = command.SplitCommandLine().ToArray();
            
            var messageBuilder = new MessageBuilder();
            var appSettings = (AppSettings)messageBuilder.ParseObject(args.ToList(), typeof(AppSettings));
            Assert.AreEqual(appSettings.MinThreads, 2);
            Assert.AreEqual(appSettings.ThreadCount, 6);
            Assert.AreEqual(appSettings.ServiceName, "TestService");
        }

        [Test]
        public void ProcessOraAppSettings()
        {
            const string command = "/ServiceName:TestService /IBatisCS:CS";
            var args = command.SplitCommandLine().ToArray();
            
            var messageBuilder = new MessageBuilder();
            var appSettings = (OraAppSettings)messageBuilder.ParseObject(args.ToList(), typeof(OraAppSettings));
            Assert.AreEqual(appSettings.ServiceName, "TestService");
            Assert.AreEqual(appSettings.IBatisCS, "CS");
        }
    }
}