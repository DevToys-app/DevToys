#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// The options to create a diff editor.
    /// </summary>
    public interface IDiffEditorConstructionOptions : IDiffEditorOptions
    {
        /// <summary>
        /// Initial theme to be used for rendering.
        /// The current out-of-the-box available themes are: 'vs' (default), 'vs-dark', 'hc-black'.
        /// You can create custom themes via `monaco.editor.defineTheme`.
        /// To switch a theme, use `monaco.editor.setTheme`
        /// </summary>
        [JsonProperty("theme", NullValueHandling = NullValueHandling.Ignore)]
        string Theme { get; set; }
    }
}
