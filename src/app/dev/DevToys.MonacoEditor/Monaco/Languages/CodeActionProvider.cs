using DevToys.MonacoEditor.Monaco.Editor;

namespace DevToys.MonacoEditor.Monaco.Languages;

/// <summary>
/// The code action interface defines the contract between extensions and
/// the [light bulb](https://code.visualstudio.com/docs/editor/editingevolved#_code-action) feature.
/// </summary>
public interface CodeActionProvider
{
    /// <summary>
    /// Provide commands for the given document and range.
    /// </summary>
    Task<CodeActionList> ProvideCodeActionsAsync(IModel model, Range range, CodeActionContext context);
}

