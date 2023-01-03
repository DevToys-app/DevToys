using DevToys.MonacoEditor.Monaco.Editor;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Languages;

/// <summary>
/// Contains additional diagnostic information about the context in which
/// a [code action](#CodeActionProvider.provideCodeActions) is run.
/// </summary>
public sealed class CodeActionContext
{
    /// <summary>
    /// An array of diagnostics.
    /// </summary>
    [JsonProperty("markers", NullValueHandling = NullValueHandling.Ignore)]
    public MarkerData[]? Markers { get; set; } // TODO: Should setup the serialization mappings between interfaces to leave interfaces here...

    /// <summary>
    /// Requested kind of actions to return.
    /// </summary>
    [JsonProperty("only", NullValueHandling = NullValueHandling.Ignore)]
    public string? Only { get; set; }
}

