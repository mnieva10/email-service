using System.ComponentModel;

namespace ModelUT.Extensions.Stubs
{
    public enum EnumStub
    {
        [DescriptionAttribute("Success")]
        Success = 0,

        [Description("Failure: Invalid state")]
        InvalidState = 1,
    }
}
