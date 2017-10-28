using System;
using System.Collections.Generic;
using System.Linq;
using Sovos.SvcBus.Common.Model.Capability;
using Sovos.SvcBus.Common.Model.Services.Interfaces;

namespace Sovos.SvcBus.Common.Model.Services
{
    public class MacroParTextParser : IMacroParTextParser
    {
        public Dictionary<string, string> ParseMacroParametersToMap(string text)
        {
            var map = new Dictionary<string, string>();
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (
                var tokens in
                    lines.Select(line => line.Split('='))
                         .Where(tokens => tokens.Length == 2)
                         .Where(tokens => !map.ContainsKey(tokens[0])))
                map.Add(tokens[0].Trim(), tokens[1].Trim());

            return map;
        }

        public MacroParameters ParseMacroParameters(string text, string resourceName)
        {
            var map = new MacroParameters();
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var tokens in lines.Select(line => line.Split('=')).Where(tokens => tokens.Length == 2))
            {
                SetPriority(tokens[0], tokens[1], map);
                
                if (resourceName.EndsWith(Constants.VarsSuffix))
                    if (!map.Vars.ContainsKey(tokens[0]))
                        map.Vars.Add(tokens[0].Trim(), tokens[1].Trim());
                if (resourceName.EndsWith(Constants.DefsSuffix))
                    if (!map.Defs.ContainsKey(tokens[0]))
                        map.Defs.Add(tokens[0].Trim(), tokens[1].Trim());
            }

            return map;
        }
        
        public MacroParameters ParseDefaultMacroParameters(string text)
        {
            var map = new MacroParameters();
            var pairs = text.Split(new[] { "," }, StringSplitOptions.None);

            foreach (var tokens in pairs.Select(pair => pair.Trim('\"').Split('=')).Where(tokens => tokens.Length == 2))
            {
                SetPriority(tokens[0], tokens[1], map);

                if (tokens[0].StartsWith(Constants.VarPrefix))
                {
                    var varKey = tokens[0].Replace(Constants.VarPrefix, "");
                    if (!map.Vars.ContainsKey(varKey))
                        map.Vars.Add(varKey, tokens[1]);
                }
                if (tokens[0].StartsWith(Constants.DefPrefix))
                {
                    var defKey = tokens[0].Replace(Constants.DefPrefix, "");
                    if (!map.Defs.ContainsKey(defKey))
                        map.Defs.Add(defKey, tokens[1]);
                }
            }

            return map;
        }

        private void SetPriority(string key, string value, MacroParameters map)
        {
            if (!key.ToLower().Contains("priority")) return;
            
            int number;
            var result = Int32.TryParse(value, out number);
            if (result)
                map.Priority = number;
        }
    }
}
