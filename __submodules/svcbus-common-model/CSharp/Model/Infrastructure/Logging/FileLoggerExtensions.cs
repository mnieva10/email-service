using System;
using System.Collections.Specialized;
using System.Text;

namespace Sovos.SvcBus.Common.Model.Infrastructure.Logging
{
    public static class FileLoggerExtensions
    {
        public static void LogSettings(this FileLogger logger, NameValueCollection appSettings)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Settings:");

            foreach (string curSettingName in appSettings)
                sb.AppendLine(string.Format("\t{0}: {1}", curSettingName, appSettings[curSettingName]));

            logger.WriteLogEntry(LogLevel.Always, sb.ToString());
        }

        public static void LogException(this FileLogger logger, Exception ex, string intro)
        {
            logger.WriteLogEntry(LogLevel.Always, string.Format("{3}:{1}   {0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace, intro));
        }
    }
}
