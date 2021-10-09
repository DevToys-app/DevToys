#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{

    /// <summary>
    /// Enable snippet suggestions. Default to 'true'.
    /// </summary>
    [JsonConverter(typeof(SnippetSuggestionsConverter))]
    public enum SnippetSuggestions
    {
        Bottom,
        Inline,
        None,
        Top
    }
}
