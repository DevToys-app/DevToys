using Newtonsoft.Json;

#if !NETSTANDARD2_0
using System.Runtime.InteropServices.WindowsRuntime;
#else
using ReadOnlyArrayAttribute = Monaco.Helpers.Stubs.ReadOnlyArrayAttribute;
#endif

namespace DevToys.MonacoEditor.Monaco.Languages;

/// <summary>
/// A hover represents additional information for a symbol or word. Hovers are
/// rendered in a tooltip-like widget.
/// </summary>
public sealed class Hover
{
    /// <summary>
    /// The contents of this hover.
    /// </summary>
    [JsonProperty("contents")]
    public IMarkdownString[] Contents { get; set; }

    /// <summary>
    /// The range to which this hover applies. When missing, the
    /// editor will use the range at the current position or the
    /// current position itself.
    /// </summary>
    [JsonProperty("range", NullValueHandling = NullValueHandling.Ignore)]
    public IRange Range { get; set; }

    public Hover([ReadOnlyArray] string[] contents, IRange range) : this(contents, range, false) { }

    public Hover([ReadOnlyArray] string[] contents, IRange range, bool isTrusted)
    {
        Contents = contents.ToMarkdownString(isTrusted);
        Range = range;
    }
}
