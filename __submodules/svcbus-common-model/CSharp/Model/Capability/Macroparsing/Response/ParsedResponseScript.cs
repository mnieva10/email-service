namespace Sovos.SvcBus.Common.Model.Capability.Macroparsing.Response
{
    [SvcBusBuildable]
    public class ParsedResponseScript : BaseResponseScript
    {
        [SvcBusSerializable]
        public string Text { get; set; }
    }
}
