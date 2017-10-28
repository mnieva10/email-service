using Sovos.SvcBus.Common.Model.Extensions;
using ModelUT.Extensions.Stubs;
using NUnit.Framework;

namespace ModelUT.Extensions
{
    [TestFixture]
    public class EnumExtensionsTest
    {
        [Test]
        public void ToDescription()
        {
            Assert.AreEqual("Success", EnumStub.Success.ToDescription());
            Assert.AreEqual("Failure: Invalid state", EnumStub.InvalidState.ToDescription());
        }
    }
}
