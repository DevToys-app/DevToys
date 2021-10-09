#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Configure the editor's accessibility support.
    /// Defaults to 'auto'. It is best to leave this to 'auto'.
    /// </summary>
    [JsonConverter(typeof(AccessibilitySupportConverter))]
    public enum AccessibilitySupport
    {
        Auto,
        Off,
        On
    }
}
