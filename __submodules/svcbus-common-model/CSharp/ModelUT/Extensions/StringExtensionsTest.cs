using System.Collections.Generic;
using System.Linq;
using Sovos.SvcBus.Common.Model.Extensions;
using NUnit.Framework;

namespace ModelUT.Extensions
{
    [TestFixture]
    public class StringExtensionsTest
    {
        [Test]
        public void ToLower()
        {
            var testString = " Test String ";
            Assert.AreEqual(testString.ToLowerNullable(), " test string ");
        }

        [Test]
        public void ToLowerEmptyString()
        {
            var testString = string.Empty;
            Assert.AreEqual(testString.ToLowerNullable(), string.Empty);
            Assert.That(testString.ToLowerNullable(), Is.Null.Or.Empty);
        }

        [Test]
        public void ToLowerNullString()
        {
            string testString = null;
            Assert.IsNull(testString.ToLowerNullable());
        }

        [Test]
        public void Trim()
        {
            var testString = " Test String ";
            Assert.AreEqual(testString.TrimNullable(), "Test String");
        }

        [Test]
        public void TrimEmptyString()
        {
            var testString = string.Empty;
            Assert.AreEqual(testString.TrimNullable(), string.Empty);
            Assert.IsEmpty(testString.TrimNullable());
        }

        [Test]
        public void TrimNullString()
        {
            string testString = null;
            Assert.IsNull(testString.TrimNullable());
        }

        [Test]
        public void TrimMatchingQuotes()
        {
            Assert.AreEqual("test test", string.Format("{0}test test{0}", '\"').TrimMatchingQuotes('\"'));
            Assert.AreEqual("test test", string.Format("{0}test test{0}", '!').TrimMatchingQuotes('!'));
        }

        [Test]
        public void SplitCommandLine()
        {
            var command = "/t:tcommand -u:ucommand /z:\"some z command\"";
            var splitCommand = command.SplitCommandLine().ToList();
            Assert.AreEqual(3, splitCommand.Count);
            Assert.AreEqual("/t:tcommand", splitCommand[0]);
            Assert.AreEqual("-u:ucommand", splitCommand[1]);
            Assert.AreEqual("/z:\"some z command\"", splitCommand[2]);
        }

        [Test]
        public void ReplaceByTagMap()
        {
            var source = "Here is {{one}} and there is {{2}}. [[one]] and [[2]].";

            var tagMap = new Dictionary<string, string>{{"one", "1"}, {"2", "two"}};
            var res = source.ReplaceByTagMap(tagMap);
            Assert.AreEqual("Here is 1 and there is two. [[one]] and [[2]].", res);

            res = source.ReplaceByTagMap(tagMap, "[[", "]]");
            Assert.AreEqual("Here is {{one}} and there is {{2}}. 1 and two.", res);

            res = res.ReplaceByTagMap(tagMap);
            Assert.AreEqual("Here is 1 and there is two. 1 and two.", res);
        }
    }
}
