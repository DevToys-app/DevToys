#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{

    /// <summary>
    /// Enable highlighting of matching brackets.
    /// Defaults to 'always'.
    /// </summary>
    [JsonConverter(typeof(MatchBracketsConverter))]
    public enum MatchBrackets { Always, Near, Never };
}
