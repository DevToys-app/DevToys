#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Enable auto indentation adjustment.
    /// Defaults to false.
    /// </summary>
    [JsonConverter(typeof(AutoIndentConverter))]
    public enum AutoIndent
    {
        Advanced,
        Brackets,
        Full,
        Keep,
        None
    }
}
