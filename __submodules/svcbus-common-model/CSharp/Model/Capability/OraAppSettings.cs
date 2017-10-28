namespace Sovos.SvcBus.Common.Model.Capability
{
    public class OraAppSettings : AppSettings
    {
        public string IBatisConfig { get; set; }
        public string IBatisCS { get; set; }

        public OraAppSettings()
        {
            IBatisConfig = @"config\sqlMap.config";
        }
    }
}