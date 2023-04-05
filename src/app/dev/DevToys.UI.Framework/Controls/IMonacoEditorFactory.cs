namespace DevToys.UI.Framework.Controls;

/// <summary>
/// Represents a factory to create an instance of the Monaco Editor.
/// </summary>
public interface IMonacoEditorFactory
{
    IMonacoEditor CreateMonacoEditorInstance();
}
