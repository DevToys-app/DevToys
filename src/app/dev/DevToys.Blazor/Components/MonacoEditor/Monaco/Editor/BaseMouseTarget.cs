using System.Text.Json;

///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class BaseMouseTarget
{
    /// <summary>
    /// The target element
    /// </summary>
    public JsonElement? Element { get; set; }

    /// <summary>
    /// The 'approximate' editor position
    /// </summary>
    public Position? Position { get; set; }

    /// <summary>
    /// Desired mouse column (e.g. when position.column gets clamped to text length -- clicking after text on a line).
    /// </summary>
    public int MouseColumn { get; set; }

    /// <summary>
    /// The 'approximate' editor range
    /// </summary>
    public Range? Range { get; set; }

    public MouseTargetType Type { get; set; }

    /// <summary>
    /// The 'approximate' editor range
    /// </summary>
    public object? Detail { get; set; }
}
