#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{

    /// <summary>
    /// Options for auto closing quotes.
    /// Defaults to language defined behavior.
    /// </summary>
    [JsonConverter(typeof(AutoClosingQuotesConverter))]
    public enum AutoClosingQuotes
    {
        Always,
        BeforeWhitespace,
        LanguageDefined,
        Never
    }
}
