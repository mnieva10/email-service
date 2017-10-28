using System.Linq;
using NUnit.Framework;

namespace PersistenceUT
{
    public class Asserter
    {
        /// <summary>Compare type, name, and value of all public properties</summary>
        public static void AreEqual(object expected, object actual)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.GetType(), actual.GetType(), "Object types do not match.");

            var expectedProperties = expected.GetType().GetProperties();
            var actualProperties = actual.GetType().GetProperties();

            Assert.AreEqual(expectedProperties.Count(p => p.CanRead), actualProperties.Count(p => p.CanRead),
                "Number of readable properties do not match");

            expectedProperties.ToList().ForEach(p =>
            {
                var actualProperty = actual.GetType().GetProperty(p.Name);

                Assert.IsNotNull(actualProperty, string.Format("Property {0} was not found", p.Name));
                Assert.AreEqual(actualProperty.PropertyType, p.PropertyType,
                                string.Format("Property {0} types mismatch", p.Name));

                var expectedValue = p.GetValue(expected, null);
                var actualValue = actualProperty.GetValue(actual, null);

                Assert.AreEqual(actualValue == null, expectedValue == null);
                if (expectedValue != null)
                    Assert.IsTrue(expectedValue.Equals(actualValue),
                        string.Format("Property {0} value mismatch", p.Name));
            });
        }
    }
}
