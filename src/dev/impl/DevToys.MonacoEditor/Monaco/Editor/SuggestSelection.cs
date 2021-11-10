#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// The history mode for suggestions.
    /// </summary>
    [JsonConverter(typeof(SuggestSelectionConverter))]
    public enum SuggestSelection
    {
        First,
        RecentlyUsed,
        RecentlyUsedByPrefix
    }
}
