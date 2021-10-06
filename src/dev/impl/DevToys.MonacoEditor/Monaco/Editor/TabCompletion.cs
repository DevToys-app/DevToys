#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{

    /// <summary>
    /// Enable tab completion.
    /// </summary>
    [JsonConverter(typeof(TabCompletionConverter))]
    public enum TabCompletion
    {
        Off,
        On,
        OnlySnippets
    }
}
