using DevToys.MonacoEditor.Monaco.Editor;

namespace DevToys.MonacoEditor.Monaco.Languages;

/// <summary>
/// The hover provider interface defines the contract between extensions and
/// the [hover](https://code.visualstudio.com/docs/editor/intellisense)-feature.
/// </summary>
public interface HoverProvider
{
    /// <summary>
    /// Provide a hover for the given position and document. Multiple hovers at the same
    /// position will be merged by the editor. A hover can have a range which defaults
    /// to the word range at the position when omitted.
    /// </summary>
    Task<Hover> ProvideHover(IModel model, Position position);
}
