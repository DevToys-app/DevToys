using System;
using System.Collections.Generic;

internal sealed class VersionUpdateRule
{
    private readonly string[] _partRules;
    public VersionUpdateRule(string rule)
    {
        _partRules = rule.Split('.');
        if (_partRules.Length < 2 || _partRules.Length > 4)
        {
            throw new ArgumentException("Expecting 2-4 version parts");
        }
        foreach (string partRule in _partRules)
        {
            if (partRule == "+"
                || partRule == "*"
                || partRule == "="
                || partRule.Contains("-pre"))
            {
                // OK, valid rule
            }
            else
            {
                // will throw an exception if not an int
                int.Parse(partRule);
            }
        }
    }

    public string Update(string version)
    {
        return Update(new VersionString(version));
    }

    public string Update(VersionString version)
    {
        var inParts = new List<string>() { version.Major, version.Minor, version.Build, version.Revision };
        var outParts = new List<string>();
        for (int index = 0; index < _partRules.Length; index++)
        {
            string rule = _partRules[index];
            string inPart = inParts[index];
            if (rule == "=")
            {
                if (inPart.Length > 0)
                {
                    outParts.Add(inParts[index]);
                }
            }
            else if (rule == "+" || rule == "*")
            {
                if (inPart.Length == 0)
                {
                    throw new ArgumentException("Can't increment missing value");
                }
                _ = int.TryParse(inPart, out int inNumber); // * gets turned into a zero
                inNumber++;
                outParts.Add(inNumber.ToString());
            }
            else
            {
                // must be a numeric literal
                outParts.Add(_partRules[index]);
            }
        }
        return string.Join(".", outParts.ToArray());
    }
}
