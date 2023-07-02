using DevToys.Blazor.Components.Monaco.Editor;

namespace DevToys.Blazor.Components.UIElements;

public partial class UIMultiLineTextInputPresenter : StyledComponentBase, IDisposable
{
    private MonacoEditor _monacoEditor = default!;
    private UITextInputWrapper _textInputWrapper = default!;

    [Parameter]
    public IUIMultiLineTextInput UIMultiLineTextInput { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UIMultiLineTextInput.SyntaxColorizationLanguageNameChanged += UIMultiLineTextInput_SyntaxColorizationLanguageNameChanged;
        UIMultiLineTextInput.TextChanged += UIMultiLineTextInput_TextChanged;
        UIMultiLineTextInput.IsReadOnlyChanged += UIMultiLineTextInput_IsReadOnlyChanged;
    }

    public void Dispose()
    {
        UIMultiLineTextInput.SyntaxColorizationLanguageNameChanged -= UIMultiLineTextInput_SyntaxColorizationLanguageNameChanged;
        UIMultiLineTextInput.TextChanged -= UIMultiLineTextInput_TextChanged;
        UIMultiLineTextInput.IsReadOnlyChanged -= UIMultiLineTextInput_IsReadOnlyChanged;
        GC.SuppressFinalize(this);
    }

    private async void UIMultiLineTextInput_TextChanged(object? sender, EventArgs e)
    {
        await _monacoEditor.SetValueAsync(UIMultiLineTextInput.Text);
    }

    private async void UIMultiLineTextInput_SyntaxColorizationLanguageNameChanged(object? sender, EventArgs e)
    {
        await _monacoEditor.SetLanguageAsync(UIMultiLineTextInput.SyntaxColorizationLanguageName);
    }

    private async void UIMultiLineTextInput_IsReadOnlyChanged(object? sender, EventArgs e)
    {
        await _monacoEditor.UpdateOptionsAsync(new EditorUpdateOptions
        {
            ReadOnly = UIMultiLineTextInput.IsReadOnly
        });
    }

    private async Task OnTextChangedAsync(ModelContentChangedEvent ev)
    {
        UIMultiLineTextInput.Text(await _monacoEditor.GetValueAsync(preserveBOM: null, lineEnding: null));
    }

    private StandaloneEditorConstructionOptions OnMonacoConstructionOptions(MonacoEditor monacoEditor)
    {
        // TODO: if language is plain text, then should we hide line numbers and few other things?
        return new StandaloneEditorConstructionOptions
        {
            ReadOnly = UIMultiLineTextInput.IsReadOnly,
            Language = UIMultiLineTextInput.SyntaxColorizationLanguageName,
            Value = UIMultiLineTextInput.Text
        };
    }
}
