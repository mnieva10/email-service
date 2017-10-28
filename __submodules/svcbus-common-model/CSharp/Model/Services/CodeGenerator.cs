using System;
using System.Text;
using Sovos.SvcBus.Common.Model.Services.Interfaces;

namespace Sovos.SvcBus.Common.Model.Services
{
    public class CodeGenerator : ICodeGenerator
    {
        protected readonly string Prefix;
        protected readonly int FirstPartLen;
        protected readonly int SecondPartLen;

        public CodeGenerator(string prefix, int firstPartLen, int secondPartLen)
        {
            Prefix = prefix;
            FirstPartLen = firstPartLen;
            SecondPartLen = secondPartLen;
        }

        public string GenerateCode()
        {
            var sb = new StringBuilder(Prefix);
            const string alphaNum = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            for (var i = 0; i < FirstPartLen + SecondPartLen; i++)
                sb.Append(alphaNum.Substring(random.Next(alphaNum.Length - 1), 1));
            sb.Insert(FirstPartLen + Prefix.Length, '-');

            return sb.ToString();
        }
    }
}
