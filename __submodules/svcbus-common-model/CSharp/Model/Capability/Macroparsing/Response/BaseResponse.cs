using System.Collections.Generic;

namespace Sovos.SvcBus.Common.Model.Capability.Macroparsing.Response
{
    public class BaseResponse<T>
    {
        [SvcBusSerializable]
        public List<T> Responses { get; set; }
    }
}
