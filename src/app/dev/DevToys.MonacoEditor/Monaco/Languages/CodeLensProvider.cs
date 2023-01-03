using DevToys.MonacoEditor.Monaco.Editor;

namespace DevToys.MonacoEditor.Monaco.Languages;

public interface CodeLensProvider
{
    Task<CodeLensList> ProvideCodeLensesAsync(IModel model);

    Task<CodeLens> ResolveCodeLensAsync(IModel model, CodeLens codeLens);
}

