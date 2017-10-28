using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Sovos.SvcBus.Common.Model.Services.Interfaces;

namespace Sovos.SvcBus.Common.Model.Services
{
    public class XmlParserService : IXmlParserService
    {
        public T Parse<T>(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException(fileName);

            var serializer = new XmlSerializer(typeof(T));


            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = XmlReader.Create(fs))
                {
                    var obj = default(T);
                    try
                    {
                        obj = (T)serializer.Deserialize(reader);
                        fs.Dispose();
                    }
                    catch { }
                    return obj;
                }
            }
        }
    }
}
