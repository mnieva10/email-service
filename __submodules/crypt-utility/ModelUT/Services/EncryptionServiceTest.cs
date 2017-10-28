using System;
using Convey.Crypt.Model.Services;
using NUnit.Framework;

namespace ModelUT.Services
{
    [TestFixture] 
    public class EncryptionServiceTest
    {
        private EncryptionService _svc;

        [SetUp]
        public void SetUp()
        {
            _svc = new EncryptionService();
        }

        [Test]
        public void GenerateSalt()
        {
            var salt = _svc.Salt(10);
            Assert.AreEqual(10, salt.Length);
        }

        #region HMACSHA1

        [Test]
        public void Hash_HMACSHA1()
        {
            const string text = "Sample text to hash";
            const string key = "373c7093-ed7d-4c2c-9b1a-cf1d39fddfef";

            var hash1 = _svc.Hash_HMACSHA1(text, key);
            var hash2 = _svc.Hash_HMACSHA1(text, key);

            Assert.AreEqual(hash1, hash2);
            Assert.AreEqual("A8jjWx8i0Dkofjhwi73gXUnLQhA=", hash2);
            Assert.AreEqual(28, hash1.Length, "HMACSHA1 encoded as Base64 string should be 28 characters [4 * (int)Math.Ceiling(160 / 8 / 3.0)]");
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [Test]
        public void Hash_HMACSHA1_InvalidInput([Values(null, "", " ")] string text, [Values(null, "", " ")] string key)
        {
            _svc.Hash_HMACSHA1(text, key);
        }
        
        #endregion

        #region RMD160

        [Test]
        public void Hash_RMD160()
        {
            const string text = @"RESOURCES.SECURITY.ACCESS CONTROL.DEVELOPER.LIMITS.~PREVARIABLES @ 4/15/2004 10:43:41 AM;RESOURCES.SECURITY.ACCESS CONTROL.DEVELOPER.~PREVARIABLES @ 10/17/2002 7:18:00 PM;RESOURCES.SECURITY.ACCESS CONTROL.POWER_USER.LIMITS.~PREDEFINES @ 5/29/2013 3:02:17 PM;RESOURCES.SECURITY.ACCESS CONTROL.POWER_USER.LIMITS.~PREVARIABLES @ 5/29/2013 3:02:17 PM;RESOURCES.SECURITY.ACCESS CONTROL.POWER_USER.~PREDEFINES @ 2/26/2013 1:49:52 PM;RESOURCES.SECURITY.ACCESS CONTROL.POWER_USER.~PREVARIABLES @ 2/26/2013 1:49:52 PM;RESOURCES.SECURITY.ACCESS CONTROL.TESTUPDATE.LIMITS.~PREVARIABLES @ 7/15/2014 1:54:17 PM;RESOURCES.SECURITY.ACCESS CONTROL.TESTUPDATE.~PREDEFINES @ 9/17/2014 3:25:40 PM;RESOURCES.SECURITY.ACCESS CONTROL.TESTUPDATE.~PREVARIABLES @ 9/17/2014 3:25:41 PM;RESOURCES.SECURITY.ACCESS CONTROL.TINCOMP.LIMITS.~PREVARIABLES @ 4/15/2004 10:43:49 AM;RESOURCES.SECURITY.ACCESS CONTROL.TINCOMP.~PREDEFINES @ 6/3/2005 10:11:00 AM;RESOURCES.SECURITY.ACCESS CONTROL.TINCOMP.~PREVARIABLES @ 6/3/2005 10:11:01 AM;RESOURCES.SECURITY.ACCESS CONTROL.TINCOMP_ADMIN.LIMITS.~PREVARIABLES @ 4/15/2004 10:43:57 AM;RESOURCES.SECURITY.ACCESS CONTROL.WPMARCO.LIMITS.~PREDEFINES @ 3/16/2012 10:47:23 AM;RESOURCES.SECURITY.ACCESS CONTROL.WPMARCO.LIMITS.~PREVARIABLES @ 3/16/2012 10:47:23 AM;RESOURCES.SECURITY.ACCESS CONTROL.WPMARCO.~PREDEFINES @ 2/7/2014 8:28:13 AM;RESOURCES.SECURITY.ACCESS CONTROL.WPMARCO.~PREVARIABLES @ 2/7/2014 8:28:13 AM;";

            var hash1 = _svc.Hash_RIPEMD160(text);
            var hash2 = _svc.Hash_RIPEMD160(text);

            Assert.AreEqual(hash1, hash2);
            Assert.AreEqual(28, hash1.Length, "RIPEMD160 encoded as Base64 string should be 28 characters [4 * (int)Math.Ceiling(160 / 8 / 3.0)]");
        }

        #endregion

        #region SHA256

        [Test]
        public void Hash_SHA256()
        {
            const string text = @"RESOURCES.SECURITY.ACCESS CONTROL.DEVELOPER.LIMITS.~PREVARIABLES @ 4/15/2004 10:43:41 AM;RESOURCES.SECURITY.ACCESS CONTROL.DEVELOPER.~PREVARIABLES @ 10/17/2002 7:18:00 PM;RESOURCES.SECURITY.ACCESS CONTROL.POWER_USER.LIMITS.~PREDEFINES @ 5/29/2013 3:02:17 PM;RESOURCES.SECURITY.ACCESS CONTROL.POWER_USER.LIMITS.~PREVARIABLES @ 5/29/2013 3:02:17 PM;RESOURCES.SECURITY.ACCESS CONTROL.POWER_USER.~PREDEFINES @ 2/26/2013 1:49:52 PM;RESOURCES.SECURITY.ACCESS CONTROL.POWER_USER.~PREVARIABLES @ 2/26/2013 1:49:52 PM;RESOURCES.SECURITY.ACCESS CONTROL.TESTUPDATE.LIMITS.~PREVARIABLES @ 7/15/2014 1:54:17 PM;RESOURCES.SECURITY.ACCESS CONTROL.TESTUPDATE.~PREDEFINES @ 9/17/2014 3:25:40 PM;RESOURCES.SECURITY.ACCESS CONTROL.TESTUPDATE.~PREVARIABLES @ 9/17/2014 3:25:41 PM;RESOURCES.SECURITY.ACCESS CONTROL.TINCOMP.LIMITS.~PREVARIABLES @ 4/15/2004 10:43:49 AM;RESOURCES.SECURITY.ACCESS CONTROL.TINCOMP.~PREDEFINES @ 6/3/2005 10:11:00 AM;RESOURCES.SECURITY.ACCESS CONTROL.TINCOMP.~PREVARIABLES @ 6/3/2005 10:11:01 AM;RESOURCES.SECURITY.ACCESS CONTROL.TINCOMP_ADMIN.LIMITS.~PREVARIABLES @ 4/15/2004 10:43:57 AM;RESOURCES.SECURITY.ACCESS CONTROL.WPMARCO.LIMITS.~PREDEFINES @ 3/16/2012 10:47:23 AM;RESOURCES.SECURITY.ACCESS CONTROL.WPMARCO.LIMITS.~PREVARIABLES @ 3/16/2012 10:47:23 AM;RESOURCES.SECURITY.ACCESS CONTROL.WPMARCO.~PREDEFINES @ 2/7/2014 8:28:13 AM;RESOURCES.SECURITY.ACCESS CONTROL.WPMARCO.~PREVARIABLES @ 2/7/2014 8:28:13 AM;";

            var hash1 = _svc.Hash_SHA256(text);
            var hash2 = _svc.Hash_SHA256(text);

            Assert.AreEqual(hash1, hash2);
            Assert.AreEqual(44, hash1.Length, "SHA256 encoded as Base64 string should be 44 characters [4 * (int)Math.Ceiling(256 / 8 / 3.0)]");
        }

        [Test]
        public void Salt_SHA256AreEqual()
        {
            for (var i = 0; i < 1000; i++)
            {
                var plainText = TestHelper.GetRandomString(50);
                var hash = _svc.Salt_SHA256(plainText);
                Assert.IsTrue(_svc.Salt_SHA256AreEqual(plainText, hash));
            }
        }

        [Test]
        public void Salt_SHA256()
        {
            var hash1 = _svc.Salt_SHA256("test1");
            var hash2 = _svc.Salt_SHA256("test1");
            Assert.AreNotEqual(hash1, hash2, "salted values should be different");

            Assert.AreEqual(88, hash1.Length, "Salt+SHA256 encoded as Base64 string should be 88 characters [4 * (int)Math.Ceiling((256 + 256) / 8 / 3.0)]");
        }

        #endregion

        #region SHA512

        [Test]
        public void Hash_SHA512()
        {
            const string text = @"RESOURCES.SECURITY.ACCESS CONTROL.DEVELOPER.LIMITS.~PREVARIABLES @ 4/15/2004 10:43:41 AM;RESOURCES.SECURITY.ACCESS CONTROL.DEVELOPER.~PREVARIABLES @ 10/17/2002 7:18:00 PM;RESOURCES.SECURITY.ACCESS CONTROL.POWER_USER.LIMITS.~PREDEFINES @ 5/29/2013 3:02:17 PM;RESOURCES.SECURITY.ACCESS CONTROL.POWER_USER.LIMITS.~PREVARIABLES @ 5/29/2013 3:02:17 PM;RESOURCES.SECURITY.ACCESS CONTROL.POWER_USER.~PREDEFINES @ 2/26/2013 1:49:52 PM;RESOURCES.SECURITY.ACCESS CONTROL.POWER_USER.~PREVARIABLES @ 2/26/2013 1:49:52 PM;RESOURCES.SECURITY.ACCESS CONTROL.TESTUPDATE.LIMITS.~PREVARIABLES @ 7/15/2014 1:54:17 PM;RESOURCES.SECURITY.ACCESS CONTROL.TESTUPDATE.~PREDEFINES @ 9/17/2014 3:25:40 PM;RESOURCES.SECURITY.ACCESS CONTROL.TESTUPDATE.~PREVARIABLES @ 9/17/2014 3:25:41 PM;RESOURCES.SECURITY.ACCESS CONTROL.TINCOMP.LIMITS.~PREVARIABLES @ 4/15/2004 10:43:49 AM;RESOURCES.SECURITY.ACCESS CONTROL.TINCOMP.~PREDEFINES @ 6/3/2005 10:11:00 AM;RESOURCES.SECURITY.ACCESS CONTROL.TINCOMP.~PREVARIABLES @ 6/3/2005 10:11:01 AM;RESOURCES.SECURITY.ACCESS CONTROL.TINCOMP_ADMIN.LIMITS.~PREVARIABLES @ 4/15/2004 10:43:57 AM;RESOURCES.SECURITY.ACCESS CONTROL.WPMARCO.LIMITS.~PREDEFINES @ 3/16/2012 10:47:23 AM;RESOURCES.SECURITY.ACCESS CONTROL.WPMARCO.LIMITS.~PREVARIABLES @ 3/16/2012 10:47:23 AM;RESOURCES.SECURITY.ACCESS CONTROL.WPMARCO.~PREDEFINES @ 2/7/2014 8:28:13 AM;RESOURCES.SECURITY.ACCESS CONTROL.WPMARCO.~PREVARIABLES @ 2/7/2014 8:28:13 AM;";

            var hash1 = _svc.Hash_SHA512(text);
            var hash2 = _svc.Hash_SHA512(text);

            Assert.AreEqual(hash1, hash2);
            Assert.AreEqual(88, hash1.Length, "SHA512 encoded as Base64 string should be 84 characters [4 * (int)Math.Ceiling(512 / 8 / 3.0)]");
        }

        [Test]
        public void Salt_SHA512()
        {
            var hash1 = _svc.Salt_SHA512("test1");
            var hash2 = _svc.Salt_SHA512("test1");
            Assert.AreNotEqual(hash1, hash2, "salted values should be different");

            Assert.AreEqual(172, hash1.Length, "Salt+SHA512 encoded as Base64 string should be 44 characters [4 * (int)Math.Ceiling(256 / 8 / 3.0)]");
        }

        [Test]
        public void Salt_SHA512AreEqual()
        {
            for (var i = 0; i < 1000; i++)
            {
                var plainText = TestHelper.GetRandomString(50);
                var hash = _svc.Salt_SHA512(plainText);
                Assert.IsTrue(_svc.Salt_SHA512AreEqual(plainText, hash));
            }
        }

        [Test]
        public void Salt_SHA512AreEqual_Const()
        {
            var hash = "mi+DsugKawBtieO60yVKGHE+dOn3LDm08SQgEbK5ZlWV8BTrdg2mW0v+R2mwVYyUeTKa4euqokfwXlVXD70iKT3aDjZJRyoSCmCzYyWdMvOoQAS/r4AEIXa9Cwd9nxPb0MyJWO1GHFRYgIDl0JeyOZ0pzqXtjvXM3ucI6fEuJUQ=";
            Assert.IsTrue(_svc.Salt_SHA512AreEqual("test1", hash));
            hash = "WOsgUWrAcPeFxli/asopOjZ6POyrSMIndBylLB9HO6VB4EiEzP7ysNv+GKnG389HrshzW56NJCXx9G+FZDwUmQ5mxvD+DS2iIoP3BsL/LCkjlzTszt5wX/CYVBmHFtU+g8mvY/zW14F8aYPVnFWRjm1r1InKLoktzZGLYvEU45w=";
            Assert.IsTrue(_svc.Salt_SHA512AreEqual("锅贴套餐", hash));
        }

        [Test]
        public void Salt_SHA512AreEqual_NotEqual()
        {
            var hash = _svc.Salt_SHA512("a");
            Assert.IsFalse(_svc.Salt_SHA512AreEqual("b", hash));
            var plainText = TestHelper.GetRandomString(50);
            hash = _svc.Salt_SHA512(plainText);
            Assert.IsFalse(_svc.Salt_SHA512AreEqual(plainText + "_", hash));
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [Test]
        public void Salt_SHA512AreEqual_NullHash()
        {
            _svc.Salt_SHA512AreEqual("text", null);
        }

        [Test]
        public void Salt_SHA512AreEqual_EmptyHash()
        {
            Assert.IsFalse(_svc.Salt_SHA512AreEqual("text", ""));
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [Test]
        public void Salt_SHA512AreEqual_NullText()
        {
            _svc.Salt_SHA512AreEqual(null, "hash");
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [Test]
        public void Salt_SHA512AreEqual_EmptyText()
        {
            _svc.Salt_SHA512AreEqual("", "hash");
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [Test]
        public void Hash_SHA512_EmptyText()
        {
            _svc.Hash_SHA512("");
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [Test]
        public void Hash_SHA512_NullText()
        {
            _svc.Hash_SHA512(null);
        }

        #endregion

        #region RIPEMD160

        [Test]
        public void Salt_RIPEMD160AreEqual()
        {
            for (var i = 0; i < 1000; i++)
            {
                var plainText = TestHelper.GetRandomString(50);
                var hash = _svc.Salt_RIPEMD160(plainText);
                Assert.IsTrue(_svc.Salt_RIPEMD160AreEqual(plainText, hash));
            }
        }

        [Test]
        public void Salt_RIPEMD160()
        {
            var hash1 = _svc.Salt_RIPEMD160("test1");
            var hash2 = _svc.Salt_RIPEMD160("test1");
            Assert.AreNotEqual(hash1, hash2, "salted values should be different");

            Assert.AreEqual(56, hash1.Length, "Salt+RIPEMD160 encoded as Base64 string should be 56 characters [4 * (int)Math.Ceiling((160 + 160) / 8 / 3.0)]");
        }

        #endregion
    }
}
