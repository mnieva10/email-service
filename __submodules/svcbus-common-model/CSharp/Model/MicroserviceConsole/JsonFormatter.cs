using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Sovos.SvcBus.Common.Model.Extensions;

namespace Sovos.SvcBus.Common.Model.MicroserviceConsole
{
    public class JsonFormatter
    {
        public const string Indent = "  ";

        public string Print(object obj)
        {
            return Print(Regex.Unescape(SafeSerialize(obj)));
        }

        public string Print(string json)
        {
            var indentation = 0;
            var quoteCount = 0;

            var result =
                from ch in json
                let quotes = (ch == '"') ? quoteCount++ : quoteCount
                let lineBreak = (ch == ',' && quotes % 2 == 0) ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(Indent, indentation)) : null
                let openChar = (ch == '{' || ch == '[') ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(Indent, ++indentation)) : ch.ToString()
                let closeChar = (ch == '}' || ch == ']') ? Environment.NewLine + string.Concat(Enumerable.Repeat(Indent, --indentation)) + ch : ch.ToString()
                let colon = (ch == ':') ? string.Format(" {0} ", ch) : ch.ToString()
                select lineBreak ?? (colon.Length > 1 ? colon : (openChar.Length > 1 ? openChar : closeChar));

            return string.Concat(result);
        }

        public string SafeSerialize(object objToSerialize)
        {
            if (objToSerialize == null)
                return FormatValue(objToSerialize);

            var returnString = "{";
            var t = objToSerialize.GetType();
            if (t.GetTypeInfo().IsPrimitive || t.Name == "String")
                return new JavaScriptSerializer().Serialize(objToSerialize);

            if (objToSerialize is IEnumerable)
            {
                var objectList = objToSerialize as IEnumerable;
                var loopReturn = string.Empty;

                if (objToSerialize is IDictionary)
                    loopReturn += string.Format("{{{0}}}", FormatValue(objToSerialize));
                else
                    loopReturn = string.Format("[{0}]", 
                        objectList.Cast<object>().Aggregate(loopReturn, (current, obj) => string.Format("{0}{1}{2}", current, current.Length >= 1 ? "," : string.Empty, SafeSerialize(obj))));

                return loopReturn;
            }

            var members = objToSerialize.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
            foreach (var memberInfo in members)
            {
                var fieldInfo = memberInfo as FieldInfo;
                var propertyInfo = memberInfo as PropertyInfo;

                if (fieldInfo == null && propertyInfo == null)
                    continue;
                if (propertyInfo != null)
#if NETCORE
                    if (!CustomAttributeExtensions.IsDefined(propertyInfo, typeof(SvcBusSerializableAttribute)))
#else
                    if (!Attribute.IsDefined(propertyInfo, typeof(SvcBusSerializableAttribute)))
#endif
                        continue;
                var value = fieldInfo != null ? fieldInfo.GetValue(objToSerialize) : propertyInfo.GetValue(objToSerialize, null);

                returnString += string.Format("{0}\"{1}\":{2}", returnString.Length > 2 ? "," : string.Empty, memberInfo.Name, FormatValue(value));
            }
            returnString += "}";

            return returnString;
        }

        private string FormatValue(object o)
        {
            if (o == null)
                return "null";

            if (o is char && (char) o == '\0')
                return string.Empty;

            switch (o.GetType().Name.ToLower())
            {
                case "datetime":
                    return string.Format("{0:M/d/yyyy h:mm:ss tt}", (DateTime) o);
                //dictionary`2 is to catch subOjects serialized as keyValuePairs
                case "string":
                case "dictionary`2":
                    return new JavaScriptSerializer().Serialize(o);
                case "memorystream":
                    var memStream = o as MemoryStream;
                    if (memStream != null)
                    {
                        var memStreamCopy = new MemoryStream();
                        var pos = memStream.Position;
                        memStream.CopyTo(memStreamCopy, 100);
                        memStream.Position = pos;
                        if (memStreamCopy != null)
                        {
                            var len = memStreamCopy.Length;
                            memStreamCopy.Position = pos;

                            return string.Format("\"{0}\"  Stream Length {1}",
                                memStreamCopy.ConvertToString().Substring(0, (int) Math.Min(len, 100)), len);
                        }
                    }
                    return "null";
            }

            if (o is ValueType)
                return new JavaScriptSerializer().Serialize(o);
            return SafeSerialize(o);
        }
    }
}
