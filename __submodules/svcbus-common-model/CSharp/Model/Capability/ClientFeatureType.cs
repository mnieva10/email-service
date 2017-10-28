using System.ComponentModel;

namespace Sovos.SvcBus.Common.Model.Capability
{
    public enum ClientFeatureType
    {
        [Description("On Demand Determination")]
        On_Demand_Determination = 0,

        [Description("Filing")]
        Filing = 1
    }
}
