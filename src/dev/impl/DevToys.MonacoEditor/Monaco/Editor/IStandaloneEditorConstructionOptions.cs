#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    public interface IStandaloneEditorConstructionOptions : IEditorConstructionOptions, IGlobalEditorOptions
    {
        /// <summary>
        /// The initial model associated with this code editor.
        /// </summary>
        [JsonProperty("model")]
        IModel? Model { get; set; }

        /// <summary>
        /// The initial value of the auto created model in the editor.
        /// To not create automatically a model, use `model: null`.
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        string? Value { get; set; }

        /// <summary>
        /// The initial language of the auto created model in the editor.
        /// To not create automatically a model, use `model: null`.
        /// </summary>
        [JsonProperty("language", NullValueHandling = NullValueHandling.Ignore)]
        string? Language { get; set; }

        /// <summary>
        /// Initial theme to be used for rendering.
        /// The current out-of-the-box available themes are: 'vs' (default), 'vs-dark', 'hc-black'.
        /// You can create custom themes via `monaco.editor.defineTheme`.
        /// To switch a theme, use `monaco.editor.setTheme`
        /// </summary>
        [JsonProperty("theme", NullValueHandling = NullValueHandling.Ignore)]
        string? Theme { get; set; }

        /// <summary>
        /// An URL to open when Ctrl+H (Windows and Linux) or Cmd+H (OSX) is pressed in
        /// the accessibility help dialog in the editor.
        ///
        /// Defaults to "https://go.microsoft.com/fwlink/?linkid=852450"
        /// </summary>
        [JsonProperty("accessibilityHelpUrl", NullValueHandling = NullValueHandling.Ignore)]
        string? AccessibilityHelpUrl { get; set; }
    }
}
