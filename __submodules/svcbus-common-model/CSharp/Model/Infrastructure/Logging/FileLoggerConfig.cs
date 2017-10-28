using System.Configuration;

namespace Sovos.SvcBus.Common.Model.Infrastructure.Logging
{
    public class FileLoggerConfig : ConfigurationSection
    {
        [ConfigurationProperty("targetFile", IsRequired = true)]
        public string TargetFile
        {
            get { return (string)base["targetFile"]; }
            set { base["targetFile"] = value; }
        }

        [ConfigurationProperty("logLevel", DefaultValue = LogLevel.Fatal, IsRequired = true)]
        public LogLevel LogLevel
        {
            get { return (LogLevel)base["logLevel"]; }
            set { base["logLevel"] = value; }
        }

        [ConfigurationProperty("maxLogFiles", DefaultValue = (uint)100, IsRequired = false)]
        public uint MaxLogFiles
        {
            get { return (uint)base["maxLogFiles"]; }
            set { base["maxLogFiles"] = value; }
        }
    }    
}
