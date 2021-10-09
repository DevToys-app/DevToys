#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{

    /// <summary>
    /// Control the rendering of the minimap slider.
    /// Defaults to 'mouseover'.
    ///
    /// Controls whether the fold actions in the gutter stay always visible or hide unless the
    /// mouse is over the gutter.
    /// Defaults to 'mouseover'.
    /// </summary>
    [JsonConverter(typeof(ShowConverter))]
    public enum Show
    {
        Always,
        Mouseover
    }
}
