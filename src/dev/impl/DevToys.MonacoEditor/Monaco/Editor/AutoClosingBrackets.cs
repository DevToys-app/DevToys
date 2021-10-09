#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Options for auto closing brackets.
    /// Defaults to language defined behavior.
    /// </summary>
    [JsonConverter(typeof(AutoClosingBracketsConverter))]
    public enum AutoClosingBrackets
    {
        Always,
        BeforeWhitespace,
        LanguageDefined,
        Never
    }
}
