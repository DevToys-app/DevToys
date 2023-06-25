///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// A decoration in the model.
/// </summary>
public class ModelDecoration
{
    /// <summary>
    /// Identifier for a decoration.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Identifier for a decoration's owner.
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// Range that this decoration covers.
    /// </summary>
    public Range? Range { get; set; }

    /// <summary>
    /// Options associated with this decoration.
    /// </summary>
    public ModelDecorationOptions? Options { get; set; }
}
