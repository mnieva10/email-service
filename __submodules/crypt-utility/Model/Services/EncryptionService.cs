using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Sovos.Crypt.Model.Services
{
    public class EncryptionService
    {
        public string Hash_HMACSHA1(string key, string text)
        {
            return Hash_Algorithm(Hash_HMACSHA1, text, key);
        }

        public string Hash_SHA512(string text)
        {
            return Hash_Algorithm(Hash_SHA512, text);
        }

        public string Hash_SHA256(string text)
        {
            return Hash_Algorithm(Hash_SHA256, text);
        }

        public string Hash_RIPEMD160(string text)
        {
            return Hash_Algorithm(Hash_RIPEMD160, text);
        }

        public string Salt_SHA512(string text)
        {
            return Salt_Algorithm(Hash_SHA512, 512, text);
        }

        public string Salt_SHA256(string text)
        {
            return Salt_Algorithm(Hash_SHA256, 256, text);
        }

        public string Salt_RIPEMD160(string text)
        {
            return Salt_Algorithm(Hash_RIPEMD160, 160, text);
        }

        public bool Salt_SHA512AreEqual(string text, string hash)
        {
            return Salt_AlgorithmAreEqual(Hash_SHA512, 512, text, hash);
        }

        public bool Salt_SHA256AreEqual(string text, string hash)
        {
            return Salt_AlgorithmAreEqual(Hash_SHA256, 256, text, hash);
        }

        public bool Salt_RIPEMD160AreEqual(string text, string hash)
        {
            return Salt_AlgorithmAreEqual(Hash_RIPEMD160, 160, text, hash);
        }

        public string Encrypt_Aes(string text, string key, string iv)
        {
            if (text == null || text.Length <= 0)
                throw new ArgumentNullException("text");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("iv");

            string hash;
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (var ms = new MemoryStream())
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(cs))
                            swEncrypt.Write(text);
                        hash = Encoding.UTF8.GetString(ms.ToArray());
                    }
            }

            return hash;
        }

        public byte[] Salt(int saltSize)
        {
            var buff = new byte[saltSize];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(buff);

            return buff;
        }

        #region Private Methods

        private string Hash_Algorithm(Func<byte[], byte[]> hashMethod, string text)
        {
            if (text == null || text.Length <= 0)
                throw new ArgumentNullException("text");

            return Convert.ToBase64String(hashMethod(Encoding.UTF8.GetBytes(text)));
        }

        private string Hash_Algorithm(Func<byte[], byte[], byte[]> hashMethod, string text, string key)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException("text");
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");

            return Convert.ToBase64String(hashMethod(Encoding.UTF8.GetBytes(text), Encoding.UTF8.GetBytes(key)));
        }

        private string Salt_Algorithm(Func<byte[], byte[]> hashMethod, int algorithmSize, string text)
        {
            if (text == null || text.Length <= 0)
                throw new ArgumentNullException("text");

            var algorithmByteSize = algorithmSize / 8;
            var salt = Salt(algorithmByteSize);
            var hash = hashMethod(AppendByteArray(salt, Encoding.UTF8.GetBytes(text)));
            return Convert.ToBase64String(AppendByteArray(salt, hash));
        }

        private bool Salt_AlgorithmAreEqual(Func<byte[], byte[]> hashMethod, int algorithmSize, string text, string hash)
        {
            if (text == null || text.Length <= 0)
                throw new ArgumentNullException("text");

            if (hash == null)
                throw new ArgumentNullException("hash");

            var algorithmByteSize = algorithmSize / 8;
            if (hash.Length != 4 * (int)Math.Ceiling(algorithmByteSize * 2 / 3.0))
                return false;

            var hashBytes = Convert.FromBase64String(hash);
            var textBytes = Encoding.UTF8.GetBytes(text);
            var testHash = hashMethod(AppendByteArray(hashBytes, 0, algorithmByteSize, textBytes, 0, textBytes.Length));
            return CompareByteArray(hashBytes, algorithmByteSize, testHash, 0, algorithmByteSize);
        }

        private static byte[] Hash_HMACSHA1(byte[] text, byte[] key)
        {
            using (var hmacsha1 = new HMACSHA1(key))
                return hmacsha1.ComputeHash(text);
        }

        private static byte[] Hash_SHA256(byte[] text)
        {
            using (var sha256 = new SHA256CryptoServiceProvider())
                return sha256.ComputeHash(text);
        }

        private static byte[] Hash_RIPEMD160(byte[] text)
        {
            using (var ripeMd160 = RIPEMD160.Create())
                return ripeMd160.ComputeHash(text);
        }

        private static byte[] Hash_SHA512(byte[] text)
        {
            using (var sha512 = new SHA512CryptoServiceProvider())
                return sha512.ComputeHash(text);
        }

        private static byte[] AppendByteArray(byte[] b1, byte[] b2)
        {
            return AppendByteArray(b1, 0, b1.Length, b2, 0, b2.Length);
        }

        private static byte[] AppendByteArray(byte[] b1, int offset1, int count1, byte[] b2, int offset2, int count2)
        {
            var result = new byte[count1 + count2];

            for (var i = offset1; i < count1; i++)
                result[i] = b1[i];
            for (var i = offset2; i < count2; i++)
                result[count1 + i] = b2[i];

            return result;
        }

        private static bool CompareByteArray(byte[] b1, int offset1, byte[] b2, int offset2, int count)
        {
            for (var i = 0; i < count; i++)
                if (b1[i + offset1] != b2[i + offset2])
                    return false;

            return true;
        }

        #endregion
    }
}
