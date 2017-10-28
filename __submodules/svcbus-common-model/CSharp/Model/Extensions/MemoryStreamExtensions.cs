using System.IO;
using System.Text;

namespace Sovos.SvcBus.Common.Model.Extensions
{
    public static class MemoryStreamExtensions
    {
        public static string ConvertToString(this MemoryStream memStream)
        {
            string retVal;
            using (var sr = new StreamReader(memStream, Encoding.UTF8))
                retVal = sr.ReadToEnd();

            return retVal;
        }
    }
}