using System;
using Convey.Crypt.Model.Services;
using NUnit.Framework;

namespace ModelUT.Services
{
    [TestFixture]
    public class LoginUtilityTest
    {
        [Test]
        public void Decrypt()
        {
            var str1 = LoginUtility.Decrypt("CGPLQEHOTFHMV5L504DAXFIVISRN12QD22XCJ2P");
            Assert.IsTrue(string.Equals("authenticate", str1));
            var str2 = LoginUtility.Decrypt("RMQ1UEBL43F4OA3TOMSGVTYBYESZN4YDXFTMG2P");
            Assert.IsTrue(string.Equals("login utility", str2));
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [Test]
        public void DecryptEmptyString()
        {
            LoginUtility.Decrypt("");
        }

        [Test]
        public void EncryptRnd()
        {
            var str1 = LoginUtility.EncryptRnd("authenticate");
            Assert.IsNotEmpty(str1);
            var str2 = LoginUtility.EncryptRnd("login utility");
            Assert.IsNotEmpty(str2);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [Test]
        public void EncryptRndEmptyString()
        {
            LoginUtility.EncryptRnd("");
        }

        [Test]
        public void Encrypt()
        {
            var str1 = LoginUtility.Encrypt("authenticate");
            Assert.IsNotEmpty(str1);
            Assert.AreEqual("3DRE4ZXTH2H4QZTRRAD0O20QVLIUGJBXFXVGYSP", str1);
            var str2 = LoginUtility.Encrypt("login utility");
            Assert.IsNotEmpty(str2);
            Assert.AreEqual("3DRE4ZXTH505LJFTV4DMQYPBJT10QZFA1TNGYSP", str2);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [Test]
        public void EncryptEmptyString()
        {
            LoginUtility.Encrypt("");
        }

        [Test]
        public void EncryptRndAndDecryptText()
        {
            for (var i = 0; i < 100; ++i)
            {
                var text = TestHelper.GetRandomString(i * 200 + 10);
                var encrypted = LoginUtility.EncryptRnd(text);
                var decrypted = LoginUtility.Decrypt(encrypted);
                Assert.IsTrue(string.Equals(text, decrypted, StringComparison.Ordinal));
            }
        }

        [Test]
        public void EncryptAndDecryptText()
        {
            for (var i = 0; i < 100; ++i)
            {
                var text = TestHelper.GetRandomString(i * 200 + 10);
                var encrypted = LoginUtility.Encrypt(text);
                var decrypted = LoginUtility.Decrypt(encrypted);
                Assert.IsTrue(string.Equals(text, decrypted, StringComparison.Ordinal));
            }
        }

        [Test]
        public void Rmd160()
        {
            var str1 = LoginUtility.Rmd160("authenticate");
            Assert.IsNotEmpty(str1);
            Assert.AreEqual("3:17063?402<<?3766956<>2;;0>6=0<92;:6:36", str1);
            var str2 = LoginUtility.Rmd160("login utility");
            Assert.IsNotEmpty(str2);
            Assert.AreEqual("0<55373>4042=1<321;90664=>>?4931838;=4;;", str2);
        }

        [Test]
        public void Rmd160Legacy()
        {
            var str1 = LoginUtility.Rmd160Legacy("authenticate");
            Assert.IsNotEmpty(str1);
            Assert.AreEqual(40, str1.Length);
            Assert.AreEqual("3CD10BFD34C0EAA003C69E482D419C005DA3ED75", str1);
            var str2 = LoginUtility.Rmd160Legacy("login utility");
            Assert.IsNotEmpty(str2);
            Assert.AreEqual(40, str2.Length);
            Assert.AreEqual("38E9928AF744CF9261012798D297AE9B6F2C28BF", str2);
        }

        [Test]
        public void Hash()
        {
            var str1 = LoginUtility.Hash("authenticate");
            Assert.IsNotEmpty(str1);
            Assert.AreEqual(LoginUtility.Decrypt("3DRE4ZXTHXG5MDQOVIG3GXRPNR0LKB35QEYGM1XTKS3ICOICXRLXROALCDL5F0GUAZNJT3YEDFXKKNDLKMCZYPEGYSP"), LoginUtility.Decrypt(str1));
            var str2 = LoginUtility.Hash("login utility");
            Assert.IsNotEmpty(str2);
            Assert.AreEqual(LoginUtility.Decrypt("3DRE4ZXTH3UKGZQVPODK4SJMB5AHYKWVE5MW4TRARZQ4JGAJJVUSKXT2QM33FQN5TSBJCCQXAECIS2XTEG0SG4EGYSP"), LoginUtility.Decrypt(str2));
            var str3 = LoginUtility.Hash("");
            Assert.IsNotEmpty(str3);
            Assert.AreEqual(LoginUtility.Decrypt("3DRE4ZXTHLYUW4MISDES0KKOUZUVY3ACSDVYPLUFBRA5ENP24U2R1O1GRQLHVL4DLANILFC2J5ZIJJ0ZSNOBVQYGYSP"), LoginUtility.Decrypt(str3));
        }
    }
}
