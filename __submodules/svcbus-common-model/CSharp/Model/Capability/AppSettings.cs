namespace Sovos.SvcBus.Common.Model.Capability
{
    public class AppSettings
    {
        public AppSettings ()
        {
            QueueMode = "Volatile";
            DispatchMode = "Single";
            MinThreads = 2;
            MaxThreads = 2;
            ThreadCount = 5;
        }

        public string MongoCS { get; set; }
        public string ServiceName { get; set; }
        public string QueueMode { get; set; }
        public string DispatchMode { get; set; }
        public int MinThreads { get; set; }
        public int MaxThreads { get; set; }
        public int ThreadCount { get; set; }
    }
}