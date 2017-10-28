using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sovos.SvcBus.Common.Model.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceByTagMap(this string instance, IDictionary<string, string> tagMap, string startTag = "{{", string endTag = "}}")
        {
            if (string.IsNullOrEmpty(startTag) || string.IsNullOrEmpty(endTag))
                return instance;

            return tagMap.Aggregate(instance, (current, tag) => current.Replace(string.Format("{0}{1}{2}", startTag, tag.Key, endTag), tag.Value));
        }

        public static string TrimNullable(this string instance)
        {
            return string.IsNullOrEmpty(instance) ? instance : instance.Trim();
        }

        public static string ToLowerNullable(this string instance)
        {
            return string.IsNullOrEmpty(instance) ? instance : instance.ToLower();
        }

        public static string ToUpperNullable(this string instance)
        {
            return string.IsNullOrEmpty(instance) ? instance : instance.ToUpper();
        }

        public static IEnumerable<string> Split(this string str, Func<char, bool> controller)
        {
            var nextPiece = 0;

            for (var c = 0; c < str.Length; c++)
            {
                if (!controller(str[c])) continue;
                yield return str.Substring(nextPiece, c - nextPiece);
                nextPiece = c + 1;
            }

            yield return str.Substring(nextPiece);
        }

        public static string TrimMatchingQuotes(this string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }

        public static IEnumerable<string> SplitCommandLine(this string commandLine)
        {
            var inQuotes = false;
            var bracketCount = 0;

            return commandLine.Split(c =>
            {
                if (c == '\"')
                    inQuotes = !inQuotes;
                if (c == '{')
                    bracketCount++;
                if (c == '}')
                    bracketCount--;
                return !inQuotes && bracketCount == 0 && c == ' ';
            })
                    .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
                    .Where(arg => !string.IsNullOrEmpty(arg));
        }

        public static MemoryStream ToMemoryStream(this string str)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(str));
        }
    }
}
