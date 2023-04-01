using DevToys.UI.Framework.Controls;

namespace DevToys.MonacoEditor;

[Export(typeof(IMonacoEditorFactory))]
internal sealed class MonacoEditorFactory : IMonacoEditorFactory
{
    public IMonacoEditor CreateMonacoEditorInstance()
    {
        return new CodeEditor();
    }
}
