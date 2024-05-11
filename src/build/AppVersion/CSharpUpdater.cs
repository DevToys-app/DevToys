using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System;

internal sealed class CSharpUpdater
{
    private readonly List<CSharpVersionUpdateRule> _updateRules;

    internal CSharpUpdater(string appVersion, string sdkVersion)
    {
        _updateRules = new List<CSharpVersionUpdateRule>();
        if (!string.IsNullOrEmpty(appVersion))
        {
            _updateRules.Add(new CSharpVersionUpdateRule("AssemblyVersion", appVersion));
            _updateRules.Add(new CSharpVersionUpdateRule("AssemblyFileVersion", appVersion));
            _updateRules.Add(new CSharpVersionUpdateRule("AssemblyInformationalVersion", sdkVersion));
        }
    }

    public void UpdateFile(string fileName)
    {
        string[] lines = File.ReadAllLines(fileName);

        var outlines = new List<string>();
        foreach (string line in lines)
        {
            outlines.Add(UpdateLine(line));
        }

        File.WriteAllLines(fileName, outlines.ToArray());
    }

    private string UpdateLine(string line)
    {
        foreach (CSharpVersionUpdateRule rule in _updateRules)
        {
            if (UpdateLineWithRule(ref line, rule))
            {
                break;
            }
        }
        return line;
    }

    private static bool UpdateLineWithRule(ref string line, CSharpVersionUpdateRule rule)
    {
        bool updated = false;
        Group? g = GetVersionString(line, rule.AttributeName);
        if (g != null)
        {
            if (VersionString.TryParse(g.Value, out VersionString? v) && v is not null)
            {
                string newVersion = rule.Update(v);
                line = string.Concat(line.AsSpan(0, g.Index), newVersion, line.AsSpan(g.Index + g.Length));
                updated = true;
            }
        }

        return updated;
    }

    private static Group? GetVersionString(string input, string attributeName)
    {
        int commentIndex = input.IndexOf("//");
        if (commentIndex != -1)
        {
            input = input.Substring(0, commentIndex);
        }
        string attributeMatch = string.Format("(?:(?:{0})|(?:{0}Attribute))", attributeName);

        string pattern = @"^\s*\[assembly: " + attributeMatch + @"\(""(?<Version>[0-9(\-\w)?\.\*]+)""\)\]";
        var regex = new Regex(pattern);
        Match m = regex.Match(input);
        if (m.Success)
        {
            return m.Groups["Version"];
        }
        return null;
    }
}
