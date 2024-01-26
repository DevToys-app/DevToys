using System.Text.Json;
using System.Text.Json.Serialization;
using DevToys.Blazor.Components.Monaco;
using DevToys.Blazor.Components.Monaco.Editor;
using DevToys.Core.Settings;

namespace DevToys.Blazor.Components;

public abstract class RicherMonacoEditorDiffBase : MonacoEditorBase
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected RicherMonacoEditorBase? _originalEditor;
    protected RicherMonacoEditorBase? _modifiedEditor;

    [Import]
    protected ISettingsProvider SettingsProvider = default!;

    /// <summary>
    /// Get the `original` editor.
    /// </summary>
    public RicherMonacoEditorBase? OriginalEditor => _originalEditor;

    /// <summary>
    /// Get the `modified` editor.
    /// </summary>
    public RicherMonacoEditorBase? ModifiedEditor => _modifiedEditor;

    /// <summary>
    /// An event emitted when the diff information computed by this diff editor has been updated.
    /// @event
    /// </summary>
    [Parameter]
    public EventCallback<RicherMonacoEditorDiffBase> OnDidUpdateDiff { get; set; }

    [Parameter]
    public EventCallback OnDidDisposeOriginal { get; set; }

    [Parameter]
    public EventCallback OnDidInitOriginal { get; set; }

    [Parameter]
    public EventCallback<ModelContentChangedEvent> OnDidChangeModelContentOriginal { get; set; }

    [Parameter]
    public EventCallback<ModelLanguageChangedEvent> OnDidChangeModelLanguageOriginal { get; set; }

    [Parameter]
    public EventCallback<ModelLanguageConfigurationChangedEvent> OnDidChangeModelLanguageConfigurationOriginal { get; set; }

    [Parameter]
    public EventCallback<ModelOptionsChangedEvent> OnDidChangeModelOptionsOriginal { get; set; }

    [Parameter]
    public EventCallback<ConfigurationChangedEvent> OnDidChangeConfigurationOriginal { get; set; }

    [Parameter]
    public EventCallback<CursorPositionChangedEvent> OnDidChangeCursorPositionOriginal { get; set; }

    [Parameter]
    public EventCallback<CursorSelectionChangedEvent> OnDidChangeCursorSelectionOriginal { get; set; }

    [Parameter]
    public EventCallback<ModelChangedEvent> OnDidChangeModelOriginal { get; set; }

    [Parameter]
    public EventCallback<ModelDecorationsChangedEvent> OnDidChangeModelDecorationsOriginal { get; set; }

    [Parameter]
    public EventCallback OnDidFocusEditorTextOriginal { get; set; }

    [Parameter]
    public EventCallback OnDidBlurEditorTextOriginal { get; set; }

    [Parameter]
    public EventCallback OnDidFocusEditorWidgetOriginal { get; set; }

    [Parameter]
    public EventCallback OnDidBlurEditorWidgetOriginal { get; set; }

    [Parameter]
    public EventCallback OnDidCompositionStartOriginal { get; set; }

    [Parameter]
    public EventCallback OnDidCompositionEndOriginal { get; set; }

    [Parameter]
    public EventCallback<PasteEvent> OnDidPasteOriginal { get; set; }

    [Parameter]
    public EventCallback<EditorMouseEvent> OnMouseUpOriginal { get; set; }

    [Parameter]
    public EventCallback<EditorMouseEvent> OnMouseDownOriginal { get; set; }

    [Parameter]
    public EventCallback<EditorMouseEvent> OnContextMenuOriginal { get; set; }

    [Parameter]
    public EventCallback<EditorMouseEvent> OnMouseMoveOriginal { get; set; }

    [Parameter]
    public EventCallback<PartialEditorMouseEvent> OnMouseLeaveOriginal { get; set; }

    [Parameter]
    public EventCallback<KeyboardEvent> OnKeyUpOriginal { get; set; }

    [Parameter]
    public EventCallback<KeyboardEvent> OnKeyDownOriginal { get; set; }

    [Parameter]
    public EventCallback<EditorLayoutInfo> OnDidLayoutChangeOriginal { get; set; }

    [Parameter]
    public EventCallback<ContentSizeChangedEvent> OnDidContentSizeChangeOriginal { get; set; }

    [Parameter]
    public EventCallback<ScrollEvent> OnDidScrollChangeOriginal { get; set; }

    [Parameter]
    public EventCallback OnDidDisposeModified { get; set; }

    [Parameter]
    public EventCallback OnDidInitModified { get; set; }

    [Parameter]
    public EventCallback<ModelContentChangedEvent> OnDidChangeModelContentModified { get; set; }

    [Parameter]
    public EventCallback<ModelLanguageChangedEvent> OnDidChangeModelLanguageModified { get; set; }

    [Parameter]
    public EventCallback<ModelLanguageConfigurationChangedEvent> OnDidChangeModelLanguageConfigurationModified { get; set; }

    [Parameter]
    public EventCallback<ModelOptionsChangedEvent> OnDidChangeModelOptionsModified { get; set; }

    [Parameter]
    public EventCallback<ConfigurationChangedEvent> OnDidChangeConfigurationModified { get; set; }

    [Parameter]
    public EventCallback<CursorPositionChangedEvent> OnDidChangeCursorPositionModified { get; set; }

    [Parameter]
    public EventCallback<CursorSelectionChangedEvent> OnDidChangeCursorSelectionModified { get; set; }

    [Parameter]
    public EventCallback<ModelChangedEvent> OnDidChangeModelModified { get; set; }

    [Parameter]
    public EventCallback<ModelDecorationsChangedEvent> OnDidChangeModelDecorationsModified { get; set; }

    [Parameter]
    public EventCallback OnDidFocusEditorTextModified { get; set; }

    [Parameter]
    public EventCallback OnDidBlurEditorTextModified { get; set; }

    [Parameter]
    public EventCallback OnDidFocusEditorWidgetModified { get; set; }

    [Parameter]
    public EventCallback OnDidBlurEditorWidgetModified { get; set; }

    [Parameter]
    public EventCallback OnDidCompositionStartModified { get; set; }

    [Parameter]
    public EventCallback OnDidCompositionEndModified { get; set; }

    [Parameter]
    public EventCallback<PasteEvent> OnDidPasteModified { get; set; }

    [Parameter]
    public EventCallback<EditorMouseEvent> OnMouseUpModified { get; set; }

    [Parameter]
    public EventCallback<EditorMouseEvent> OnMouseDownModified { get; set; }

    [Parameter]
    public EventCallback<EditorMouseEvent> OnContextMenuModified { get; set; }

    [Parameter]
    public EventCallback<EditorMouseEvent> OnMouseMoveModified { get; set; }

    [Parameter]
    public EventCallback<PartialEditorMouseEvent> OnMouseLeaveModified { get; set; }

    [Parameter]
    public EventCallback<KeyboardEvent> OnKeyUpModified { get; set; }

    [Parameter]
    public EventCallback<KeyboardEvent> OnKeyDownModified { get; set; }

    [Parameter]
    public EventCallback<EditorLayoutInfo> OnDidLayoutChangeModified { get; set; }

    [Parameter]
    public EventCallback<ContentSizeChangedEvent> OnDidContentSizeChangeModified { get; set; }

    [Parameter]
    public EventCallback<ScrollEvent> OnDidScrollChangeModified { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
#pragma warning disable BL0005

            Guard.IsNotNull(OriginalEditor);
            OriginalEditor.OnDidDispose = OnDidDisposeOriginal;
            OriginalEditor.OnDidInit = OnDidInitOriginal;
            OriginalEditor.OnDidChangeModelContent = OnDidChangeModelContentOriginal;
            OriginalEditor.OnDidChangeModelLanguage = OnDidChangeModelLanguageOriginal;
            OriginalEditor.OnDidChangeModelLanguageConfiguration = OnDidChangeModelLanguageConfigurationOriginal;
            OriginalEditor.OnDidChangeModelOptions = OnDidChangeModelOptionsOriginal;
            OriginalEditor.OnDidChangeConfiguration = OnDidChangeConfigurationOriginal;
            OriginalEditor.OnDidChangeCursorPosition = OnDidChangeCursorPositionOriginal;
            OriginalEditor.OnDidChangeCursorSelection = OnDidChangeCursorSelectionOriginal;
            OriginalEditor.OnDidChangeModel = OnDidChangeModelOriginal;
            OriginalEditor.OnDidChangeModelDecorations = OnDidChangeModelDecorationsOriginal;
            OriginalEditor.OnDidFocusEditorText = OnDidFocusEditorTextOriginal;
            OriginalEditor.OnDidBlurEditorText = OnDidBlurEditorTextOriginal;
            OriginalEditor.OnDidFocusEditorWidget = OnDidFocusEditorWidgetOriginal;
            OriginalEditor.OnDidBlurEditorWidget = OnDidBlurEditorWidgetOriginal;
            OriginalEditor.OnDidCompositionStart = OnDidCompositionStartOriginal;
            OriginalEditor.OnDidCompositionEnd = OnDidCompositionEndOriginal;
            //readonly onDidAttemptReadOnlyEdit: IEvent<void>;
            OriginalEditor.OnDidPaste = OnDidPasteOriginal;
            OriginalEditor.OnMouseUp = OnMouseUpOriginal;
            OriginalEditor.OnMouseDown = OnMouseDownOriginal;
            OriginalEditor.OnContextMenu = OnContextMenuOriginal;
            OriginalEditor.OnMouseMove = OnMouseMoveOriginal;
            OriginalEditor.OnMouseLeave = OnMouseLeaveOriginal;
            OriginalEditor.OnKeyUp = OnKeyUpOriginal;
            OriginalEditor.OnKeyDown = OnKeyDownOriginal;
            OriginalEditor.OnDidLayoutChange = OnDidLayoutChangeOriginal;
            OriginalEditor.OnDidContentSizeChange = OnDidContentSizeChangeOriginal;
            OriginalEditor.OnDidScrollChange = OnDidScrollChangeOriginal;
            //readonly onDidChangeHiddenAreas: IEvent<void>;
            await OriginalEditor.SetEventListenersAsync();
            await OriginalEditor.OnDidInit.InvokeAsync(OriginalEditor);

            Guard.IsNotNull(ModifiedEditor);
            ModifiedEditor.OnDidCompositionEnd = OnDidCompositionEndModified;
            ModifiedEditor.OnDidDispose = OnDidDisposeModified;
            ModifiedEditor.OnDidInit = OnDidInitModified;
            ModifiedEditor.OnDidChangeModelContent = OnDidChangeModelContentModified;
            ModifiedEditor.OnDidChangeModelLanguage = OnDidChangeModelLanguageModified;
            ModifiedEditor.OnDidChangeModelLanguageConfiguration = OnDidChangeModelLanguageConfigurationModified;
            ModifiedEditor.OnDidChangeModelOptions = OnDidChangeModelOptionsModified;
            ModifiedEditor.OnDidChangeConfiguration = OnDidChangeConfigurationModified;
            ModifiedEditor.OnDidChangeCursorPosition = OnDidChangeCursorPositionModified;
            ModifiedEditor.OnDidChangeCursorSelection = OnDidChangeCursorSelectionModified;
            ModifiedEditor.OnDidChangeModel = OnDidChangeModelModified;
            ModifiedEditor.OnDidChangeModelDecorations = OnDidChangeModelDecorationsModified;
            ModifiedEditor.OnDidFocusEditorText = OnDidFocusEditorTextModified;
            ModifiedEditor.OnDidBlurEditorText = OnDidBlurEditorTextModified;
            ModifiedEditor.OnDidFocusEditorWidget = OnDidFocusEditorWidgetModified;
            ModifiedEditor.OnDidBlurEditorWidget = OnDidBlurEditorWidgetModified;
            ModifiedEditor.OnDidCompositionStart = OnDidCompositionStartModified;
            ModifiedEditor.OnDidCompositionEnd = OnDidCompositionEndModified;
            //readonly onDidAttemptReadOnlyEdit: IEvent<void>;
            ModifiedEditor.OnDidPaste = OnDidPasteModified;
            ModifiedEditor.OnMouseUp = OnMouseUpModified;
            ModifiedEditor.OnMouseDown = OnMouseDownModified;
            ModifiedEditor.OnContextMenu = OnContextMenuModified;
            ModifiedEditor.OnMouseMove = OnMouseMoveModified;
            ModifiedEditor.OnMouseLeave = OnMouseLeaveModified;
            ModifiedEditor.OnKeyUp = OnKeyUpModified;
            ModifiedEditor.OnKeyDown = OnKeyDownModified;
            ModifiedEditor.OnDidLayoutChange = OnDidLayoutChangeModified;
            ModifiedEditor.OnDidContentSizeChange = OnDidContentSizeChangeModified;
            ModifiedEditor.OnDidScrollChange = OnDidScrollChangeModified;
            //readonly onDidChangeHiddenAreas: IEvent<void>;
            await ModifiedEditor.SetEventListenersAsync();
            await ModifiedEditor.OnDidInit.InvokeAsync(ModifiedEditor);
#pragma warning restore BL0005
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public override async ValueTask DisposeAsync()
    {
        if (OriginalEditor is not null)
        {
            await OriginalEditor.DisposeAsync();
        }

        if (ModifiedEditor is not null)
        {
            await ModifiedEditor.DisposeAsync();
        }

        await base.DisposeAsync();
    }

    internal override async Task SetEventListenersAsync()
    {
        if (OnDidUpdateDiff.HasDelegate)
        {
            await SetEventListenerAsync("OnDidUpdateDiff");
        }

        await base.SetEventListenersAsync();
    }

    [JSInvokable]
    public override async Task EventCallbackAsync(string eventName, string eventJson)
    {
        if (!_isDisposed)
        {
            switch (eventName)
            {
                case "OnDidUpdateDiff": await OnDidUpdateDiff.InvokeAsync(this); break;
            }
        }

        await base.EventCallbackAsync(eventName, eventJson);
    }

    /// <summary>
    /// @see {@link ICodeEditor.getContainerDomNode}
    /// </summary>
    public ValueTask<string> GetContainerDomNodeIdAsync()
        => JSRuntime.InvokeAsync<string>("devtoys.MonacoEditor.getContainerDomNodeId", Id);

    /// <summary>
    /// Type the getModel() of IEditor.
    /// </summary>
    public ValueTask<DiffEditorModel> GetModelAsync()
        => JSRuntime.InvokeAsync<DiffEditorModel>("devtoys.MonacoEditor.getInstanceDiffModel", Id);

    /// <summary>
    /// Sets the current model attached to this editor.
    /// If the previous model was created by the editor via the value key in the options
    /// literal object, it will be destroyed. Otherwise, if the previous model was set
    /// via setModel, or the model key in the options literal object, the previous model
    /// will not be destroyed.
    /// It is safe to call setModel(null) to simply detach the current model from the editor.
    /// </summary>
    public ValueTask<bool> SetModelAsync(DiffEditorModel model)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.setInstanceDiffModel", Id, model);

    /// <summary>
    /// Update the editor's options after the editor has been created.
    /// </summary>
    public async ValueTask<bool> UpdateOptionsAsync(DiffEditorOptions newOptions)
    {
        // Convert the options object into a JsonElement to get rid of the properties with null values
        string optionsJson = JsonSerializer.Serialize(newOptions, jsonSerializerOptions);
        JsonElement optionsDict = JsonSerializer.Deserialize<JsonElement>(optionsJson);

        bool success = await JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.updateOptions", Id, optionsDict);

        if (success)
        {
            DiffEditorModel textModel = await this.GetModelAsync();

            if (textModel is not null && textModel.Original is not null && textModel.Modified is not null)
            {
                UITextEndOfLinePreference eolPreference = SettingsProvider.GetSetting(PredefinedSettings.TextEditorEndOfLinePreference);
                if (eolPreference != UITextEndOfLinePreference.TextDefault)
                {
                    EndOfLineSequence eol = EndOfLineSequence.CRLF;
                    if (eolPreference == UITextEndOfLinePreference.LF)
                    {
                        eol = EndOfLineSequence.LF;
                    }

                    success = await textModel.Original.SetEOLAsync(JSRuntime, eol);
                    if (success)
                    {
                        success = await textModel.Modified.SetEOLAsync(JSRuntime, eol);
                    }
                }
            }
        }

        return success;
    }
}
