using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System;

internal sealed class ProjectUpdater
{
    private readonly List<ProjectVersionUpdateRule> _updateRules;

    internal ProjectUpdater(string sdkVersion)
    {
        _updateRules = new List<ProjectVersionUpdateRule>();
        if (!string.IsNullOrEmpty(sdkVersion))
        {
            _updateRules.Add(new ProjectVersionUpdateRule("Version", sdkVersion));
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
        foreach (ProjectVersionUpdateRule rule in _updateRules)
        {
            if (UpdateLineWithRule(ref line, rule))
            {
                break;
            }
        }
        return line;
    }

    private static bool UpdateLineWithRule(ref string line, ProjectVersionUpdateRule rule)
    {
        bool updated = false;
        Group? g = GetVersionString(line, rule.ParameterName);
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

    private static Group? GetVersionString(string input, string parameterName)
    {
        string parameterMatch = string.Format("(?:(?:{0})|(?:{0}Attribute))", parameterName);

        string pattern = @"^\s*\<" + parameterMatch + @">(?<Version>[0-9(\-\w)?\.\*]+)</" + parameterMatch + ">";
        var regex = new Regex(pattern);
        Match m = regex.Match(input);
        if (m.Success)
        {
            return m.Groups["Version"];
        }
        return null;
    }
}
