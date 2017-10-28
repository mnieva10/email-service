using System;
using System.Text;

namespace ModelUT
{
    public static class TestHelper
    {
        private const string DefaultAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string GetRandomString(int size)
        {
            var random = new Random(Guid.NewGuid().GetHashCode());

            if (size == 0)
                throw new ArgumentException("size == 0");

            var sb = new StringBuilder();

            var alphabet = DefaultAlphabet.ToCharArray();

            for (var i = 0; i < size; ++i)
            {
                var next = random.Next(0, alphabet.Length);
                sb.Append(alphabet[next]);
            }

            return sb.ToString();
        }
    }
}
