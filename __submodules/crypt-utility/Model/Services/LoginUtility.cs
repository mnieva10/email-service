using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Sovos.Crypt.Model.Services
{
    public static class LoginUtility
    {
        private const string DllVersion = "1-0-2"; /* PLEASE!!! maintain this constant in sync with the dll version this code operates with */

        #region DllNameBindingConstants
        #if DEBUG
            private const string ConfigType = "d";
        #else
            private const string ConfigType = "r";
        #endif
        #if WIN64
            private const string CpuType = "64";
        #else
            private const string CpuType = "32";
        #endif
        #endregion

        public const string LoginFuncsDll = "LoginFuncs_" + ConfigType + CpuType + "_v" + DllVersion + ".dll";
        
        [DllImport(LoginFuncsDll, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr getRMD160(StringBuilder text);

        [DllImport(LoginFuncsDll, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr getRMD160Legacy(StringBuilder text);

        [DllImport(LoginFuncsDll, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr strEncrypt(StringBuilder text);

        [DllImport(LoginFuncsDll, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr strEncryptRnd(StringBuilder text);

        [DllImport(LoginFuncsDll, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr strDecrypt(StringBuilder text);

        [DllImport(LoginFuncsDll, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void memFree(IntPtr dest);

        public static string Rmd160(string text)
        {
            var encryptionStr = new StringBuilder(text);
            var ptr = new IntPtr();
            string result;

            try
            {
                ptr = getRMD160(encryptionStr);
                result = Marshal.PtrToStringAnsi(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    memFree(ptr);
            }
            return result;
        }

        public static string Rmd160Legacy(string text)
        {
            var encryptionStr = new StringBuilder(text);
            var ptr = new IntPtr();
            string result;

            try
            {
                ptr = getRMD160Legacy(encryptionStr);
                result = Marshal.PtrToStringAnsi(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    memFree(ptr);
            }
            return result;
        }

        public static string Encrypt(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            var encryptionStr = new StringBuilder(text);
            var ptr = new IntPtr();
            string result;

            try
            {
                ptr = strEncrypt(encryptionStr);
                result = Marshal.PtrToStringAnsi(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    memFree(ptr);
            }
            return result;
        }

        public static string EncryptRnd(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            var encryptionStr = new StringBuilder(text);
            var ptr = new IntPtr();
            string result;

            try
            {
                ptr = strEncryptRnd(encryptionStr);
                result = Marshal.PtrToStringAnsi(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    memFree(ptr);
            }
            return result;
        }

        public static string Decrypt(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            var decryptionStr = new StringBuilder(text);
            var ptr = new IntPtr();
            string result;

            try
            {
                ptr = strDecrypt(decryptionStr);
                result = Marshal.PtrToStringAnsi(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    memFree(ptr);
            }
            return result;
        }

        public static string Hash(string text)
        {
            return EncryptRnd(Rmd160(text));
        }

        public static string HashSha256Hex(string toHash)
        {
            var hash = new SHA256Managed();
            var utf8 = UTF8Encoding.UTF8.GetBytes(toHash);
            return BytesToHex(hash.ComputeHash(utf8));
        }

        private static string BytesToHex(byte[] toConvert)
        {
            var s = new StringBuilder(toConvert.Length * 2);
            foreach (byte b in toConvert)
                s.Append(b.ToString("x2"));

            return s.ToString();
        }
    }
}
