using DevToys.Blazor.Components.Monaco;
using DevToys.Blazor.Components.Monaco.Editor;

namespace DevToys.Blazor.Components.UIElements;

public partial class UIMultiLineTextInputPresenter : JSStyledComponentBase
{
    private readonly TaskCompletionSource _monacoInitializationAwaiter = new();

    private MonacoEditor _monacoEditor = default!;
    private UITextInputWrapper _textInputWrapper = default!;
    private bool _ignoreChangeComingFromUIMultiLineTextInput;
    private string _modelName = string.Empty;

    [Parameter]
    public IUIMultiLineTextInput UIMultiLineTextInput { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _modelName = Id + "-text-model";
        UIMultiLineTextInput.SyntaxColorizationLanguageNameChanged += UIMultiLineTextInput_SyntaxColorizationLanguageNameChanged;
        UIMultiLineTextInput.TextChanged += UIMultiLineTextInput_TextChanged;
        UIMultiLineTextInput.IsReadOnlyChanged += UIMultiLineTextInput_IsReadOnlyChanged;
    }

    public override ValueTask DisposeAsync()
    {
        UIMultiLineTextInput.SyntaxColorizationLanguageNameChanged -= UIMultiLineTextInput_SyntaxColorizationLanguageNameChanged;
        UIMultiLineTextInput.TextChanged -= UIMultiLineTextInput_TextChanged;
        UIMultiLineTextInput.IsReadOnlyChanged -= UIMultiLineTextInput_IsReadOnlyChanged;

        return base.DisposeAsync();
    }

    private async void UIMultiLineTextInput_TextChanged(object? sender, EventArgs e)
    {
        if (!_ignoreChangeComingFromUIMultiLineTextInput)
        {
            await _monacoInitializationAwaiter.Task;
            await _monacoEditor.SetValueAsync(UIMultiLineTextInput.Text);
        }
    }

    private async void UIMultiLineTextInput_SyntaxColorizationLanguageNameChanged(object? sender, EventArgs e)
    {
        await _monacoInitializationAwaiter.Task;
        await _monacoEditor.SetLanguageAsync(UIMultiLineTextInput.SyntaxColorizationLanguageName);
    }

    private async void UIMultiLineTextInput_IsReadOnlyChanged(object? sender, EventArgs e)
    {
        await _monacoInitializationAwaiter.Task;
        await _monacoEditor.UpdateOptionsAsync(new EditorUpdateOptions
        {
            ReadOnly = UIMultiLineTextInput.IsReadOnly
        });
    }

    private async Task OnMonacoEditorTextChangedAsync(ModelContentChangedEvent ev)
    {
        if (ev.Changes is not null)
        {
            _ignoreChangeComingFromUIMultiLineTextInput = true;

            string documentText = await _monacoEditor.GetValueAsync(preserveBOM: null, lineEnding: null);

            UIMultiLineTextInput.Text(documentText);

            _ignoreChangeComingFromUIMultiLineTextInput = false;
        }
    }

    private async Task OnMonacoEditorInitializedAsync()
    {
        // Get or create the text model
        TextModel textModel = await MonacoEditorHelper.GetModelAsync(JSRuntime, uri: _modelName);
        if (textModel == null)
        {
            textModel
                = await MonacoEditorHelper.CreateModelAsync(
                    JSRuntime,
                    UIMultiLineTextInput.Text,
                    language: UIMultiLineTextInput.SyntaxColorizationLanguageName,
                    uri: _modelName);

            // Set the editor model
            await _monacoEditor.SetModelAsync(textModel);
        }

        // Set the text of model
        await textModel.SetValueAsync(JSRuntime, UIMultiLineTextInput.Text);

        _monacoInitializationAwaiter.TrySetResult();
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
