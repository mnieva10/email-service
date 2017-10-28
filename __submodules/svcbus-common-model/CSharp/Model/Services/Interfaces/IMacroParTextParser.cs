using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.Services.Interfaces
{
    public interface IMacroParTextParser
    {
        MacroParameters ParseDefaultMacroParameters(string text);
        MacroParameters ParseMacroParameters(string text, string resourceName);
        Dictionary<string, string> ParseMacroParametersToMap(string text);
    }
}
