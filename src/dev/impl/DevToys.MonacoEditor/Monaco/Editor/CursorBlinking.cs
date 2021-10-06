#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Control the cursor animation style, possible values are 'blink', 'smooth', 'phase',
    /// 'expand' and 'solid'.
    /// Defaults to 'blink'.
    /// </summary>
    [JsonConverter(typeof(CursorBlinkingConverter))]
    public enum CursorBlinking
    {
        Blink,
        Expand,
        Phase,
        Smooth,
        Solid
    }
}
