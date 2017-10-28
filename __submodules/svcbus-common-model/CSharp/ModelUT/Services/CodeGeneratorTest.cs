using System.Linq;
using Sovos.SvcBus.Common.Model.Services;
using NUnit.Framework;

namespace ModelUT.Services
{
    [TestFixture]
    public class CodeGeneratorTest
    {
        [Test]
        public void GenerateRegCode()
        {
            var regCodeGenerator = new CodeGenerator("R", 5, 6);
            var regCode = regCodeGenerator.GenerateCode();

            Assert.IsTrue(regCode.StartsWith("R"));
            Assert.AreEqual(13, regCode.Length);
            Assert.AreEqual("-", regCode.Substring(6, 1));

            var alphaNumArr = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
            var firstPart = regCode.Substring(1, 5);
            foreach (var curChar in firstPart.ToCharArray())
                Assert.IsTrue(alphaNumArr.Contains(curChar));

            var secondPart = regCode.Substring(7, 6);
            foreach (var curChar in secondPart.ToCharArray())
                Assert.IsTrue(alphaNumArr.Contains(curChar));
        }
    }
}
