using DevToys.MonacoEditor.Monaco.Editor;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Languages;

/// <summary>
/// String representations for a color.
/// <seealso href="https://microsoft.github.io/monaco-editor/api/interfaces/monaco.languages.icolorpresentation.html">monaco.languages.IColorPresentation</seealso>
/// </summary>
public sealed class ColorPresentation
{
    /// <summary>
    /// An optional array of additional text edits that are applied when
    /// selecting this completion. Edits must not overlap with the main edit
    /// nor with themselves.
    /// </summary>
    [JsonProperty("additionalTextEdits", NullValueHandling = NullValueHandling.Ignore)]
    public ISingleEditOperation[]? AdditionalTextEdits { get; set; }

    /// <summary>
    /// The label of this color presentation. It will be shown on the color picker header. 
    /// By default this is also the text that is inserted when selecting this color presentation.
    /// </summary>
    [JsonProperty("label")]
    public string? Label { get; set; }

    [JsonProperty("textEdit", NullValueHandling = NullValueHandling.Ignore)]
    public ISingleEditOperation? TextEdit { get; set; }

    public ColorPresentation(string label)
    {
        Label = label;
    }
}
