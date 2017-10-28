namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class ProcessResponse
    {
        [SvcBusSerializable]
        public string Error { get; private set; }
        [SvcBusSerializable]
        public string Output { get; private set; }
        [SvcBusSerializable]
        public bool Status { get; private set; }

        public ProcessResponse(string error, string output)
        {
            Error = error;
            Output = output;

            if (string.IsNullOrEmpty(Error))
                Status = true;
        }
    }
}
