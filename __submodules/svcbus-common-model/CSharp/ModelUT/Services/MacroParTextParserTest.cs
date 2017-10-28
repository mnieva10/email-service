using Sovos.SvcBus.Common.Model.Capability;
using Sovos.SvcBus.Common.Model.Services;
using ModelUT.Services.Stubs;
using NUnit.Framework;

namespace ModelUT.Services
{
    [TestFixture]
    public class MacroParTextParserTest
    {
        [Test]
        public void ParseMacroParametersToMap()
        {
            var factory = new ScriptFactory();
            var parser = new MacroParTextParser();

            var data = parser.ParseMacroParametersToMap(factory.ParametersData[0]);
            Assert.AreEqual(4, data.Count);

            data = parser.ParseMacroParametersToMap(factory.ParametersData[1]);
            Assert.AreEqual(8, data.Count);

            data = parser.ParseMacroParametersToMap(factory.ParametersData[2]);
            Assert.AreEqual(16, data.Count);
        }

        [Test]
        public void ParseDefaultMacroParameters()
        {
            var factory = new ScriptFactory();
            var parser = new MacroParTextParser();

            var data = parser.ParseDefaultMacroParameters(factory.DefaultParams[0]);
            Assert.AreEqual(9, data.Vars.Count);
            Assert.AreEqual(5, data.Defs.Count);

            data = parser.ParseDefaultMacroParameters(factory.DefaultParams[1]);
            Assert.AreEqual(13, data.Vars.Count);
            Assert.AreEqual(8, data.Defs.Count);
        }

        [Test]
        public void ParseMacroParameters()
        {
            var factory = new ScriptFactory();
            var parser = new MacroParTextParser();

            var data = parser.ParseMacroParameters(factory.PreParsedScripts[0], Constants.VarsSuffix);
            Assert.AreEqual(5, data.Vars.Count);
            Assert.AreEqual(5, data.Priority);

            data = parser.ParseMacroParameters(factory.PreParsedScripts[2], Constants.VarsSuffix);
            Assert.AreEqual(7, data.Vars.Count);
            Assert.AreEqual(7, data.Priority);

            data = parser.ParseMacroParameters(factory.PreParsedScripts[3], Constants.VarsSuffix);
            Assert.AreEqual(3, data.Vars.Count);
            Assert.AreEqual(0, data.Priority);

            data = parser.ParseMacroParameters(factory.PreParsedScripts[4], Constants.VarsSuffix);
            Assert.AreEqual(4, data.Vars.Count);
            Assert.AreEqual(0, data.Priority);
        }
    }
}
