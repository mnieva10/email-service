namespace Sovos.SvcBus.Common.Model.MicroserviceConsole.Capability
{
    public class BsonResponse
    {
        [SvcBusSerializable]
        public object _id { get; set; }
        [SvcBusSerializable]
        public object msgid { get; set; }
        [SvcBusSerializable]
        public bool brdcst { get; set; }
        [SvcBusSerializable]
        public object response { get; set; }
        [SvcBusSerializable]
        public object exception { get; set; }
        [SvcBusSerializable]
        public object message { get; set; }
    }
}
