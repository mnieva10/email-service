namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class Environment
    {
        [SvcBusSerializable]
        public string Domain { get; set; }
        [SvcBusSerializable]
        public string Schema { get; set; }
        [SvcBusSerializable]
        public string ConnectionString { get; set; }
        [SvcBusSerializable]
        public string EnvironmentName { get; set; }
        [SvcBusSerializable]
        public string ShortName { get; set; }

        public Environment() { }

        public Environment(string domain, string connectionString, string environmentName, string shortName, string schema)
        {
            Domain = domain;
            ConnectionString = connectionString;
            EnvironmentName = environmentName;
            ShortName = shortName;
            Schema = schema;
        }
    }
}
