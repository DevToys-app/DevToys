#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Options for auto surrounding.
    /// Defaults to always allowing auto surrounding.
    /// </summary>
    [JsonConverter(typeof(AutoSurroundConverter))]
    public enum AutoSurround
    {
        Brackets,
        LanguageDefined,
        Never,
        Quotes
    }
}
