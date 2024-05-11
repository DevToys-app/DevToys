using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

internal sealed class VersionString
{
    internal VersionString()
    {
        Major = "0";
        Minor = "0";
        Build = "0";
        Revision = "0";
    }

    public VersionString(string version)
    {
        if (!Parse(version))
        {
            throw new ArgumentException("Invalid version string");
        }
    }

    private bool Parse(string input)
    {
        string pattern = @"^(?<Major>\d+)\.(?<Minor>\d+)\.(?:(?:(?<Build>\d+|\d+\-\w*)\.(?<Revision>\*|\d+|\d+\-\w*))|(?<Build>\*|\d+|\d+\-\w*))$";
        var regex = new Regex(pattern);
        Match match = regex.Match(input);
        if (match.Success)
        {
            Major = match.Groups["Major"].Value;
            Minor = match.Groups["Minor"].Value;
            Build = match.Groups["Build"].Value;
            Revision = match.Groups["Revision"].Value;
        }
        return match.Success;
    }

    public static bool TryParse(string input, [MaybeNullWhen(false)] out VersionString? version)
    {
        var temp = new VersionString();
        version = null;
        if (temp.Parse(input))
        {
            version = temp;
        }
        return version != null;
    }

    public string Major { get; set; } = string.Empty;

    public string Minor { get; set; } = string.Empty;

    public string Build { get; set; } = string.Empty;

    public string Revision { get; set; } = string.Empty;

    public override string ToString()
    {
        return string.Format("{0}.{1}{2}{3}",
            Major, Minor,
            string.IsNullOrEmpty(Build) ? "" : "." + Build,
            string.IsNullOrEmpty(Revision) ? "" : "." + Revision);
    }
}
