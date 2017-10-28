namespace Sovos.SvcBus.Common.Model.Capability
{
    [SvcBusBuildable]
    public class NameValuePair
    {
        [SvcBusSerializable]
        public string Name { get; set; }
        [SvcBusSerializable]
        public string Value { get; set; }

        public NameValuePair() { }

        public NameValuePair(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}