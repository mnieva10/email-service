using System.Diagnostics.CodeAnalysis;
using Sovos.SvcBus.Common.Model.Capability;
using Sovos.SvcBus.Common.Model.MicroserviceConsole;
using Sovos.SvcBus.Common.Model.MicroserviceConsole.Exceptions;
using ModelUT.Services.Stubs;
using NUnit.Framework;
using System.Runtime.InteropServices;

namespace ModelUT.MicroserviceConsole
{
    [TestFixture]
    public class CmdInterpreterTest
    {
        [Test]
        public void CmdInterpreterConstructor_InvalidDI()
        {
            Assert.Throws<DispatchInterfaceCreateInstanceException>(() => new CmdInterpreter<DiAbstractStub>(null, null));
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), Test]
        public void ProcessCommand_MethodNotFound()
        {
            Assert.Throws<DispatchInterfaceMethodNotFoundException>(() =>
            {
                var svc = new CmdInterpreter<DiStub>(null, null);
                svc.ProcessCommand("NoSuchMethod params");
            });
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), Test]
        public void ProcessCommand()
        {
            var svc = new CmdInterpreter<DiStub>(null, null);
            var result = svc.ProcessCommand("Help");
#if !NETCORE
            Assert.AreEqual("Help\r\n", result);
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Assert.AreEqual("Help\r\n", result);
            else
                Assert.AreEqual("Help\n", result);
#endif
            result = svc.ProcessCommand("TestMethod /t:DataDto /p:params");
            Assert.AreEqual("Pass", ((DataDto)result).DataString);
        }
    }
}
