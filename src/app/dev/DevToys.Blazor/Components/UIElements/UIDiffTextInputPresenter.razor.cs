using DevToys.Blazor.Components.Monaco;
using DevToys.Blazor.Components.Monaco.Editor;

namespace DevToys.Blazor.Components.UIElements;

public partial class UIDiffTextInputPresenter : JSStyledComponentBase, IDisposable
{
    private MonacoEditorDiff _monacoEditor = default!;
    private UITextInputWrapper _textInputWrapper = default!;

    [Parameter]
    public IUIDiffTextInput UIDiffTextInput { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UIDiffTextInput.TextChanged += UIMultiLineTextInput_TextChanged;
        UIDiffTextInput.ModifiedTextChanged += UIDiffTextInput_RightTextChanged;
        UIDiffTextInput.IsReadOnlyChanged += UIMultiLineTextInput_IsReadOnlyChanged;
        UIDiffTextInput.InlineModeChanged += UIDiffTextInput_InlineModeChanged;
    }

    public void Dispose()
    {
        UIDiffTextInput.TextChanged -= UIMultiLineTextInput_TextChanged;
        UIDiffTextInput.ModifiedTextChanged -= UIDiffTextInput_RightTextChanged;
        UIDiffTextInput.IsReadOnlyChanged -= UIMultiLineTextInput_IsReadOnlyChanged;
        UIDiffTextInput.InlineModeChanged -= UIDiffTextInput_InlineModeChanged;
        GC.SuppressFinalize(this);
    }

    private async Task OnMonacoEditorInitializedAsync()
    {
        // Get or create the original model
        TextModel original_model = await MonacoEditorHelper.GetModelAsync(JSRuntime, "sample-diff-editor-originalModel");
        if (original_model == null)
        {
            original_model = await MonacoEditorHelper.CreateModelAsync(JSRuntime, UIDiffTextInput.Text, "javascript", "sample-diff-editor-originalModel");
        }

        // Get or create the modified model
        TextModel modified_model = await MonacoEditorHelper.GetModelAsync(JSRuntime, "sample-diff-editor-modifiedModel");
        if (modified_model == null)
        {
            modified_model = await MonacoEditorHelper.CreateModelAsync(JSRuntime, UIDiffTextInput.ModifiedText, "javascript", "sample-diff-editor-modifiedModel");
        }

        // Set the editor model
        await _monacoEditor.SetModelAsync(new DiffEditorModel
        {
            Original = original_model,
            Modified = modified_model
        });
    }

    private async void UIMultiLineTextInput_TextChanged(object? sender, EventArgs e)
    {
        Guard.IsNotNull(_monacoEditor.OriginalEditor);
        await _monacoEditor.OriginalEditor.SetValueAsync(UIDiffTextInput.Text);
    }

    private async void UIDiffTextInput_RightTextChanged(object? sender, EventArgs e)
    {
        Guard.IsNotNull(_monacoEditor.ModifiedEditor);
        await _monacoEditor.ModifiedEditor.SetValueAsync(UIDiffTextInput.ModifiedText);
    }

    private async void UIMultiLineTextInput_IsReadOnlyChanged(object? sender, EventArgs e)
    {
        await _monacoEditor.UpdateOptionsAsync(new DiffEditorOptions
        {
            ReadOnly = UIDiffTextInput.IsReadOnly
        });
    }

    private async void UIDiffTextInput_InlineModeChanged(object? sender, EventArgs e)
    {
        await _monacoEditor.UpdateOptionsAsync(new DiffEditorOptions
        {
            EnableSplitViewResizing = !UIDiffTextInput.InlineMode
        });
    }

    private StandaloneDiffEditorConstructionOptions OnMonacoConstructionOptions(MonacoEditorDiff monacoEditor)
    {
        return new StandaloneDiffEditorConstructionOptions
        {
            ReadOnly = UIDiffTextInput.IsReadOnly,
            EnableSplitViewResizing = !UIDiffTextInput.InlineMode
        };
    }
}
