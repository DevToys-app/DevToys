using Newtonsoft.Json;
using Windows.Foundation.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;

namespace DevToys.MonacoEditor.Monaco;

public sealed class IMarkdownString
{
    [JsonProperty("isTrusted")]
    public bool IsTrusted { get; set; }
    [JsonProperty("supportThemeIcons", NullValueHandling = NullValueHandling.Ignore)]
    public bool? SupportThemeIcons { get; set; }

    [JsonProperty("uris", NullValueHandling = NullValueHandling.Ignore)]
    public IDictionary<string, Uri>? Uris { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }

    public IMarkdownString(string svalue) : this(svalue, false) { }

    public IMarkdownString(string svalue, bool isTrusted)
    {
        Value = svalue;
        IsTrusted = isTrusted;
    }
}

public static class MarkdownStringExtensions
{
    [DefaultOverload]
    public static IMarkdownString ToMarkdownString(this string svalue)
    {
        return ToMarkdownString(svalue, false);
    }

    [DefaultOverload]
    public static IMarkdownString ToMarkdownString(this string svalue, bool isTrusted)
    {
        return new IMarkdownString(svalue, isTrusted);
    }

    public static IMarkdownString[] ToMarkdownString(this string[] values)
    {
        return ToMarkdownString(values, false);
    }

    public static IMarkdownString[] ToMarkdownString(this string[] values, bool isTrusted)
    {
        return values.Select(value => new IMarkdownString(value, isTrusted)).ToArray();
    }
}
