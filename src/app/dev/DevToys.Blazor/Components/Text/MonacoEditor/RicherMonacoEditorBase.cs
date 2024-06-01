using System.Text.Json;
using System.Text.Json.Serialization;
using DevToys.Blazor.Components.Monaco;
using DevToys.Blazor.Components.Monaco.Editor;
using DevToys.Core.Settings;

namespace DevToys.Blazor.Components;

public class RicherMonacoEditorBase : MonacoEditorBase
{
    private readonly List<string> _deltaDecorationIds = new();
    private CursorStateComputer? _executeEditsLambda;

    [Import]
    protected ISettingsProvider SettingsProvider = default!;

    public RicherMonacoEditorBase(IJSRuntime? jsRuntime = null, ISettingsProvider? settingsProvider = null)
    {
        if (jsRuntime is not null)
        {
            JSRuntime = jsRuntime;
        }

        if (settingsProvider is not null)
        {
            SettingsProvider = settingsProvider;
        }
    }

    /// <summary>
    /// An event emitted when the content of the current model has changed.
    /// </summary>
    [Parameter]
    public EventCallback<ModelContentChangedEvent> OnDidChangeModelContent { get; set; }

    /// <summary>
    /// An event emitted when the language of the current model has changed.
    /// </summary>
    [Parameter]
    public EventCallback<ModelLanguageChangedEvent> OnDidChangeModelLanguage { get; set; }

    /// <summary>
    /// An event emitted when the language configuration of the current model has changed.
    /// </summary>
    [Parameter]
    public EventCallback<ModelLanguageConfigurationChangedEvent> OnDidChangeModelLanguageConfiguration { get; set; }

    /// <summary>
    /// An event emitted when the options of the current model has changed.
    /// </summary>
    [Parameter]
    public EventCallback<ModelOptionsChangedEvent> OnDidChangeModelOptions { get; set; }

    /// <summary>
    /// An event emitted when the configuration of the editor has changed. (e.g. `editor.updateOptions()`)
    /// </summary>
    [Parameter]
    public EventCallback<ConfigurationChangedEvent> OnDidChangeConfiguration { get; set; }

    /// <summary>
    /// An event emitted when the cursor position has changed.
    /// </summary>
    [Parameter]
    public EventCallback<CursorPositionChangedEvent> OnDidChangeCursorPosition { get; set; }

    /// <summary>
    /// An event emitted when the cursor selection has changed.
    /// </summary>
    [Parameter]
    public EventCallback<CursorSelectionChangedEvent> OnDidChangeCursorSelection { get; set; }

    /// <summary>
    /// An event emitted when the model of this editor has changed (e.g. `editor.setModel()`).
    /// </summary>
    [Parameter]
    public EventCallback<ModelChangedEvent> OnDidChangeModel { get; set; }

    /// <summary>
    /// An event emitted when the decorations of the current model have changed.
    /// </summary>
    [Parameter]
    public EventCallback<ModelDecorationsChangedEvent> OnDidChangeModelDecorations { get; set; }

    /// <summary>
    /// An event emitted when the text inside this editor gained focus (i.e. cursor starts blinking).
    /// </summary>
    [Parameter]
    public EventCallback OnDidFocusEditorText { get; set; }

    /// <summary>
    /// An event emitted when the text inside this editor lost focus (i.e. cursor stops blinking).
    /// </summary>
    [Parameter]
    public EventCallback OnDidBlurEditorText { get; set; }

    /// <summary>
    /// An event emitted when the text inside this editor or an editor widget gained focus.
    /// </summary>
    [Parameter]
    public EventCallback OnDidFocusEditorWidget { get; set; }

    /// <summary>
    /// An event emitted when the text inside this editor or an editor widget lost focus.
    /// </summary>
    [Parameter]
    public EventCallback OnDidBlurEditorWidget { get; set; }

    /// <summary>
    /// An event emitted after composition has started.
    /// </summary>
    [Parameter]
    public EventCallback OnDidCompositionStart { get; set; }

    /// <summary>
    /// An event emitted after composition has ended.
    /// </summary>
    [Parameter]
    public EventCallback OnDidCompositionEnd { get; set; }

    /// <summary>
    /// An event emitted when users paste text in the editor.
    /// </summary>
    [Parameter]
    public EventCallback<PasteEvent> OnDidPaste { get; set; }

    /// <summary>
    /// An event emitted on a "mouseup".
    /// </summary>
    [Parameter]
    public EventCallback<EditorMouseEvent> OnMouseUp { get; set; }

    /// <summary>
    /// An event emitted on a "mousedown".
    /// </summary>
    [Parameter]
    public EventCallback<EditorMouseEvent> OnMouseDown { get; set; }

    /// <summary>
    /// An event emitted on a "contextmenu".
    /// </summary>
    [Parameter]
    public EventCallback<EditorMouseEvent> OnContextMenu { get; set; }

    /// <summary>
    /// An event emitted on a "mousemove".
    /// </summary>
    [Parameter]
    public EventCallback<EditorMouseEvent> OnMouseMove { get; set; }

    /// <summary>
    /// An event emitted on a "mouseleave".
    /// </summary>
    [Parameter]
    public EventCallback<PartialEditorMouseEvent> OnMouseLeave { get; set; }

    /// <summary>
    /// An event emitted on a "keyup".
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEvent> OnKeyUp { get; set; }

    /// <summary>
    /// An event emitted on a "keydown".
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEvent> OnKeyDown { get; set; }

    /// <summary>
    /// An event emitted when the layout of the editor has changed.
    /// </summary>
    [Parameter]
    public EventCallback<EditorLayoutInfo> OnDidLayoutChange { get; set; }

    /// <summary>
    /// An event emitted when the content width or content height in the editor has changed.
    /// </summary>
    [Parameter]
    public EventCallback<ContentSizeChangedEvent> OnDidContentSizeChange { get; set; }

    /// <summary>
    /// An event emitted when the scroll in the editor has changed.
    /// </summary>
    [Parameter]
    public EventCallback<ScrollEvent> OnDidScrollChange { get; set; }

    internal override async Task SetEventListenersAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        if (OnDidCompositionEnd.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidCompositionEnd));
        }

        if (OnDidCompositionStart.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidCompositionStart));
        }

        if (OnContextMenu.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnContextMenu));
        }

        if (OnDidBlurEditorText.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidBlurEditorText));
        }

        if (OnDidBlurEditorWidget.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidBlurEditorWidget));
        }

        if (OnDidChangeConfiguration.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidChangeConfiguration));
        }

        if (OnDidChangeCursorPosition.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidChangeCursorPosition));
        }

        if (OnDidChangeCursorSelection.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidChangeCursorSelection));
        }

        if (OnDidChangeModel.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidChangeModel));
        }

        if (OnDidChangeModelContent.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidChangeModelContent));
        }

        if (OnDidChangeModelDecorations.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidChangeModelDecorations));
        }

        if (OnDidChangeModelLanguage.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidChangeModelLanguage));
        }

        if (OnDidChangeModelLanguageConfiguration.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidChangeModelLanguageConfiguration));
        }

        if (OnDidChangeModelOptions.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidChangeModelOptions));
        }

        if (OnDidContentSizeChange.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidContentSizeChange));
        }

        if (OnDidFocusEditorText.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidFocusEditorText));
        }

        if (OnDidFocusEditorWidget.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidFocusEditorWidget));
        }

        if (OnDidLayoutChange.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidLayoutChange));
        }

        if (OnDidPaste.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidPaste));
        }

        if (OnDidScrollChange.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidScrollChange));
        }

        if (OnKeyDown.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnKeyDown));
        }

        if (OnKeyUp.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnKeyUp));
        }

        if (OnMouseDown.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnMouseDown));
        }

        if (OnMouseLeave.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnMouseLeave));
        }

        if (OnMouseMove.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnMouseMove));
        }

        if (OnMouseUp.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnMouseUp));
        }

        await base.SetEventListenersAsync();
    }

    [JSInvokable]
    public override async Task EventCallbackAsync(string eventName, string? eventJson)
    {
        eventJson ??= string.Empty;
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        if (!_isDisposed)
        {
            switch (eventName)
            {
                case nameof(OnDidCompositionEnd):
                    await OnDidCompositionEnd.InvokeAsync(this);
                    break;

                case nameof(OnDidCompositionStart):
                    await OnDidCompositionStart.InvokeAsync(this);
                    break;

                case nameof(OnContextMenu):
                    if (!string.IsNullOrEmpty(eventJson))
                    {
                        await OnContextMenu.InvokeAsync(JsonSerializer.Deserialize<EditorMouseEvent>(eventJson, jsonOptions));
                    }
                    break;

                case nameof(OnDidBlurEditorText):
                    await OnDidBlurEditorText.InvokeAsync(this);
                    break;

                case nameof(OnDidBlurEditorWidget):
                    await OnDidBlurEditorWidget.InvokeAsync(this);
                    break;

                case nameof(OnDidChangeConfiguration):
                    await OnDidChangeConfiguration.InvokeAsync(new ConfigurationChangedEvent(JsonSerializer.Deserialize<List<bool>?>(eventJson, jsonOptions)));
                    break;

                case nameof(OnDidChangeCursorPosition):
                    await OnDidChangeCursorPosition.InvokeAsync(JsonSerializer.Deserialize<CursorPositionChangedEvent>(eventJson, jsonOptions));
                    break;

                case nameof(OnDidChangeCursorSelection):
                    await OnDidChangeCursorSelection.InvokeAsync(JsonSerializer.Deserialize<CursorSelectionChangedEvent>(eventJson, jsonOptions));
                    break;

                case nameof(OnDidChangeModel):
                    await OnDidChangeModel.InvokeAsync(JsonSerializer.Deserialize<ModelChangedEvent>(eventJson, jsonOptions));
                    break;

                case nameof(OnDidChangeModelContent):
                    await OnDidChangeModelContent.InvokeAsync(JsonSerializer.Deserialize<ModelContentChangedEvent>(eventJson, jsonOptions));
                    break;

                case nameof(OnDidChangeModelDecorations):
                    await OnDidChangeModelDecorations.InvokeAsync(JsonSerializer.Deserialize<ModelDecorationsChangedEvent>(eventJson, jsonOptions));
                    break;

                case nameof(OnDidChangeModelLanguage):
                    await OnDidChangeModelLanguage.InvokeAsync(JsonSerializer.Deserialize<ModelLanguageChangedEvent>(eventJson, jsonOptions));
                    break;

                case nameof(OnDidChangeModelLanguageConfiguration):
                    await OnDidChangeModelLanguageConfiguration.InvokeAsync(JsonSerializer.Deserialize<ModelLanguageConfigurationChangedEvent>(eventJson, jsonOptions));
                    break;

                case nameof(OnDidChangeModelOptions):
                    await OnDidChangeModelOptions.InvokeAsync(JsonSerializer.Deserialize<ModelOptionsChangedEvent>(eventJson, jsonOptions));
                    break;

                case nameof(OnDidContentSizeChange):
                    await OnDidContentSizeChange.InvokeAsync(JsonSerializer.Deserialize<ContentSizeChangedEvent>(eventJson, jsonOptions));
                    break;

                case nameof(OnDidFocusEditorText):
                    await OnDidFocusEditorText.InvokeAsync(this);
                    break;

                case nameof(OnDidFocusEditorWidget):
                    await OnDidFocusEditorWidget.InvokeAsync(this);
                    break;

                case nameof(OnDidLayoutChange):
                    if (!string.IsNullOrEmpty(eventJson))
                    {
                        await OnDidLayoutChange.InvokeAsync(JsonSerializer.Deserialize<EditorLayoutInfo>(eventJson, jsonOptions));
                    }
                    break;

                case nameof(OnDidPaste):
                    if (!string.IsNullOrEmpty(eventJson))
                    {
                        await OnDidPaste.InvokeAsync(JsonSerializer.Deserialize<PasteEvent>(eventJson, jsonOptions));
                    }
                    break;

                case nameof(OnDidScrollChange):
                    if (!string.IsNullOrEmpty(eventJson))
                    {
                        await OnDidScrollChange.InvokeAsync(JsonSerializer.Deserialize<ScrollEvent>(eventJson, jsonOptions));
                    }
                    break;

                case nameof(OnKeyDown):
                    if (!string.IsNullOrEmpty(eventJson))
                    {
                        await OnKeyDown.InvokeAsync(JsonSerializer.Deserialize<KeyboardEvent>(eventJson, jsonOptions));
                    }
                    break;

                case nameof(OnKeyUp):
                    if (!string.IsNullOrEmpty(eventJson))
                    {
                        await OnKeyUp.InvokeAsync(JsonSerializer.Deserialize<KeyboardEvent>(eventJson, jsonOptions));
                    }
                    break;

                case nameof(OnMouseDown):
                    if (!string.IsNullOrEmpty(eventJson))
                    {
                        await OnMouseDown.InvokeAsync(JsonSerializer.Deserialize<EditorMouseEvent>(eventJson, jsonOptions));
                    }
                    break;

                case nameof(OnMouseLeave):
                    if (!string.IsNullOrEmpty(eventJson))
                    {
                        await OnMouseLeave.InvokeAsync(JsonSerializer.Deserialize<PartialEditorMouseEvent>(eventJson, jsonOptions));
                    }
                    break;

                case nameof(OnMouseMove):
                    if (!string.IsNullOrEmpty(eventJson))
                    {
                        await OnMouseMove.InvokeAsync(JsonSerializer.Deserialize<EditorMouseEvent>(eventJson, jsonOptions));
                    }
                    break;

                case nameof(OnMouseUp):
                    if (!string.IsNullOrEmpty(eventJson))
                    {
                        await OnMouseUp.InvokeAsync(JsonSerializer.Deserialize<EditorMouseEvent>(eventJson, jsonOptions));
                    }
                    break;
            }
        }

        await base.EventCallbackAsync(eventName, eventJson);
    }

    /// <summary>
    /// Returns true if the text inside this editor or an editor widget has focus.
    /// </summary>
    internal ValueTask<bool> HasWidgetFocusAsync()
        => JSRuntime.InvokeAsync<bool>("devtoys.MonacoEditor.hasWidgetFocus", Id);

    /// <summary>
    /// Type the getModel() of IEditor.
    /// </summary>
    internal ValueTask<TextModel> GetModelAsync()
        => JSRuntime.InvokeAsync<TextModel>("devtoys.MonacoEditor.getInstanceModel", Id);

    /// <summary>
    /// Sets the current model attached to this editor.
    /// If the previous model was created by the editor via the value key in the options
    /// literal object, it will be destroyed. Otherwise, if the previous model was set
    /// via setModel, or the model key in the options literal object, the previous model
    /// will not be destroyed.
    /// It is safe to call setModel(null) to simply detach the current model from the editor.
    /// </summary>
    internal ValueTask<bool> SetModelAsync(TextModel model)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.setInstanceModel", Id, model.Uri);

    /// <summary>
    /// Gets all the editor computed options.
    /// </summary>
    internal async Task<ComputedEditorOptions> GetOptionsAsync()
    {
        List<string> strList = await JSRuntime.InvokeAsync<List<string>>("devtoys.MonacoEditor.getOptions", Id);

        return new ComputedEditorOptions(strList);
    }
    /// <summary>
    /// Gets a specific editor option.
    /// </summary>
    internal async Task<T?> GetOptionAsync<T>(EditorOption option)
    {
        string strValue = await JSRuntime.InvokeAsync<string>("devtoys.MonacoEditor.getOption", Id, (int)option);

        return JsonSerializer.Deserialize<T>(strValue);
    }
    /// <summary>
    /// Returns the editor's configuration (without any validation or defaults).
    /// </summary>
    internal ValueTask<EditorOptions> GetRawOptionsAsync()
        => JSRuntime.InvokeAsync<EditorOptions>("devtoys.MonacoEditor.getRawOptions", Id);

    /// <summary>
    /// Get value of the current model attached to this editor.
    /// See <see cref="TextModel.GetWordAtPosition(Position)"/>
    /// </summary>
    internal ValueTask<WordAtPosition> GetWordAtPositionAsync(string position)
        => JSRuntime.InvokeAsync<WordAtPosition>("devtoys.MonacoEditor.getWordAtPosition", position);

    /// <summary>
    /// Get value of the current model attached to this editor.
    /// See <see cref="TextModel.GetValue(EndOfLinePreference?, bool?)"/>
    /// </summary>
    internal ValueTask<string> GetValueAsync(bool? preserveBOM = null, string? lineEnding = null)
        => JSRuntime.InvokeAsync<string>("devtoys.MonacoEditor.getValue", Id, preserveBOM, lineEnding);

    /// <summary>
    /// Set the value of the current model attached to this editor.
    /// See <see cref="TextModel.SetValue(string)"/>
    /// </summary>
    internal ValueTask<bool> SetValueAsync(string newValue)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.setValue", Id, newValue);

    /// <summary>
    /// Get the width of the editor's content.
    /// This is information that is "erased" when computing `scrollWidth = Math.max(contentWidth, width)`
    /// </summary>
    internal ValueTask<double> GetContentWidthAsync()
        => JSRuntime.InvokeAsync<double>("devtoys.MonacoEditor.getContentWidth", Id);

    /// <summary>
    /// Get the scrollWidth of the editor's viewport.
    /// </summary>
    internal ValueTask<double> GetScrollWidthAsync()
        => JSRuntime.InvokeAsync<double>("devtoys.MonacoEditor.getScrollWidth", Id);

    /// <summary>
    /// Get the scrollLeft of the editor's viewport.
    /// </summary>
    internal ValueTask<double> GetScrollLeftAsync()
        => JSRuntime.InvokeAsync<double>("devtoys.MonacoEditor.getScrollLeft", Id);

    /// <summary>
    /// Get the height of the editor's content.
    /// This is information that is "erased" when computing `scrollHeight = Math.max(contentHeight, height)`
    /// </summary>
    internal ValueTask<double> GetContentHeightAsync()
        => JSRuntime.InvokeAsync<double>("devtoys.MonacoEditor.getContentHeight", Id);

    /// <summary>
    /// Get the scrollHeight of the editor's viewport.
    /// </summary>
    internal ValueTask<double> GetScrollHeightAsync()
        => JSRuntime.InvokeAsync<double>("devtoys.MonacoEditor.getScrollHeight", Id);

    /// <summary>
    /// Get the scrollTop of the editor's viewport.
    /// </summary>
    internal ValueTask<double> GetScrollTopAsync()
        => JSRuntime.InvokeAsync<double>("devtoys.MonacoEditor.getScrollTop", Id);

    /// <summary>
    /// Change the scrollLeft of the editor's viewport.
    /// </summary>
    internal ValueTask<bool> SetScrollLeftAsync(double newScrollLeft, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.setScrollLeft", Id, newScrollLeft, scrollType);

    /// <summary>
    /// Change the scrollTop of the editor's viewport.
    /// </summary>
    internal ValueTask<bool> SetScrollTopAsync(double newScrollTop, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.setScrollTop", Id, newScrollTop, scrollType);

    /// <summary>
    /// Change the scroll position of the editor's viewport.
    /// </summary>
    internal ValueTask<bool> SetScrollPositionAsync(NewScrollPosition position, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.setScrollPosition", Id, position, scrollType);

    /// <summary>
    /// Create an "undo stop" in the undo-redo stack.
    /// </summary>
    internal ValueTask<bool> PushUndoStopAsync()
        => JSRuntime.InvokeAsync<bool>("devtoys.MonacoEditor.pushUndoStop", Id);

    /// <summary>
    /// Remove the "undo stop" in the undo-redo stack.
    /// </summary>
    internal ValueTask<bool> PopUndoStopAsync()
        => JSRuntime.InvokeAsync<bool>("devtoys.MonacoEditor.popUndoStop", Id);

    /// <summary>
    /// Execute edits on the editor.
    /// The edits will land on the undo-redo stack, but no "undo stop" will be pushed.
    /// </summary>
    /// <param name="source">The source of the call.</param>
    /// <param name="edits">The edits to execute.</param>
    /// <param name="endCursorState">Cursor state after the edits were applied.</param>
    internal ValueTask<bool> ExecuteEditsAsync(string source, List<IdentifiedSingleEditOperation> edits, List<Selection> endCursorState)
        => JSRuntime.InvokeAsync<bool>("devtoys.MonacoEditor.executeEdits", Id, source, edits, endCursorState);

    internal ValueTask<bool> ExecuteEditsAsync(string source, List<IdentifiedSingleEditOperation> edits, CursorStateComputer endCursorState)
    {
        _executeEditsLambda = endCursorState;
        return JSRuntime.InvokeAsync<bool>("devtoys.MonacoEditor.executeEdits", Id, source, edits, "function");
    }

    [JSInvokable]
    public List<Selection>? ExecuteEditsCallback(List<ValidEditOperation> inverseEditOperations)
    {
        Console.WriteLine("ExecuteEditsCallback is called : " + JsonSerializer.Serialize(inverseEditOperations));
        return _executeEditsLambda?.Invoke(inverseEditOperations) ?? null;
    }

    /// <summary>
    /// All decorations added through this call will get the ownerId of this editor.
    /// </summary>
    internal async Task<string[]> DeltaDecorationsAsync(string[] oldDecorationIds, ModelDeltaDecoration[] newDecorations)
    {
        TextModel textModel = await this.GetModelAsync();

        if (textModel is not null)
        {
            string[] newDecorationIds = await textModel.DeltaDecorationsAsync(JSRuntime, oldDecorationIds, newDecorations, null);
            _deltaDecorationIds.RemoveAll(d => oldDecorationIds.Any(o => o == d));
            _deltaDecorationIds.AddRange(newDecorationIds);
            return newDecorationIds;
        }

        return oldDecorationIds ?? Array.Empty<string>();
    }

    /// <summary>
    /// Remove all the decorations.
    /// </summary>
    internal Task ResetDeltaDecorationsAsync()
        => DeltaDecorationsAsync(_deltaDecorationIds.ToArray(), Array.Empty<ModelDeltaDecoration>());

    /// <summary>
    /// Replaces all the decorations.
    /// </summary>
    internal Task ReplaceAllDecorationsByAsync(ModelDeltaDecoration[] newDecorations)
        => DeltaDecorationsAsync(_deltaDecorationIds.ToArray(), newDecorations);

    /// <summary>
    /// Get the layout info for the editor.
    /// </summary>
    internal ValueTask<EditorLayoutInfo> GetLayoutInfoAsync()
        => JSRuntime.InvokeAsync<EditorLayoutInfo>("devtoys.MonacoEditor.getLayoutInfo", Id);

    /// <summary>
    /// Returns the ranges that are currently visible.
    /// Does not account for horizontal scrolling.
    /// </summary>
    internal ValueTask<List<Monaco.Range>> GetVisibleRangesAsync()
        => JSRuntime.InvokeAsync<List<Monaco.Range>>("devtoys.MonacoEditor.getVisibleRanges", Id);

    /// <summary>
    /// Get the vertical position (top offset) for the line's top w.r.t. to the first line.
    /// </summary>
    internal ValueTask<double> GetTopForLineNumberAsync(int lineNumber)
        => JSRuntime.InvokeAsync<double>("devtoys.MonacoEditor.getTopForLineNumber", Id, lineNumber);

    /// <summary>
    /// Get the vertical position (top offset) for the position w.r.t. to the first line.
    /// </summary>
    internal ValueTask<double> GetTopForPositionAsync(int lineNumber, int column)
        => JSRuntime.InvokeAsync<double>("devtoys.MonacoEditor.getTopForPosition", Id, lineNumber, column);

    /// <summary>
    /// Returns the editor's container dom node
    /// </summary>
    internal ValueTask<string> GetContainerDomNodeIdAsync()
        => JSRuntime.InvokeAsync<string>("devtoys.MonacoEditor.getContainerDomNodeId", Id);

    /// <summary>
    /// Returns the editor's dom node
    /// </summary>
    internal ValueTask<string> GetDomNodeIdAsync()
        => JSRuntime.InvokeAsync<string>("devtoys.MonacoEditor.getDomNodeId", Id);

    /// <summary>
    /// Get the horizontal position (left offset) for the column w.r.t to the beginning of the line.
    /// This method works only if the line `lineNumber` is currently rendered (in the editor's viewport).
    /// Use this method with caution.
    /// </summary>
    internal ValueTask<int> GetOffsetForColumnAsync(int lineNumber, int column)
        => JSRuntime.InvokeAsync<int>("devtoys.MonacoEditor.getOffsetForColumn", Id, lineNumber, column);

    /// <summary>
    /// Force an editor render now.
    /// </summary>
    internal ValueTask<bool> RenderAsync(bool? forceRedraw = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.render", Id, forceRedraw);

    /// <summary>
    /// Get the hit test target at coordinates `clientX` and `clientY`.
    /// The coordinates are relative to the top-left of the viewport.
    /// </summary>
    /// <returns>Hit test target or null if the coordinates fall outside the editor or the editor has no model.</returns>
    internal ValueTask<BaseMouseTarget> GetTargetAtClientPointAsync(int clientX, int clientY)
        => JSRuntime.InvokeAsync<BaseMouseTarget>("devtoys.MonacoEditor.getTargetAtClientPoint", Id, clientX, clientY);

    /// <summary>
    /// Get the visible position for `position`.
    /// The result position takes scrolling into account and is relative to the top left corner of the editor.
    /// Explanation 1: the results of this method will change for the same `position` if the user scrolls the editor.
    /// Explanation 2: the results of this method will not change if the container of the editor gets repositioned.
    /// Warning: the results of this method are inaccurate for positions that are outside the current editor viewport.
    /// </summary>
    internal ValueTask<ScrolledVisiblePosition> GetScrolledVisiblePositionAsync(Position position)
        => JSRuntime.InvokeAsync<ScrolledVisiblePosition>("devtoys.MonacoEditor.getScrolledVisiblePosition", Id, position);

    /// <summary>
    /// Change the language for a model.
    /// </summary>
    internal async ValueTask<bool> SetLanguageAsync(string languageId)
        => await MonacoEditorHelper.SetModelLanguageAsync(JSRuntime, await GetModelAsync(), languageId);

    internal async ValueTask<bool> UpdateOptionsAsync(EditorUpdateOptions newOptions)
    {
        if (_isDisposed)
        {
            return false;
        }

        // Convert the options object into a JsonElement to get rid of the properties with null values
        string optionsJson = JsonSerializer.Serialize(newOptions, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        JsonElement optionsDict = JsonSerializer.Deserialize<JsonElement>(optionsJson);
        bool success = await JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.updateOptions", Id, optionsDict);

        if (success)
        {
            TextModel textModel = await this.GetModelAsync();

            if (textModel is not null)
            {
                UITextEndOfLinePreference eolPreference = SettingsProvider.GetSetting(PredefinedSettings.TextEditorEndOfLinePreference);
                if (eolPreference != UITextEndOfLinePreference.TextDefault)
                {
                    EndOfLineSequence eol = EndOfLineSequence.CRLF;
                    if (eolPreference == UITextEndOfLinePreference.LF)
                    {
                        eol = EndOfLineSequence.LF;
                    }

                    success = await textModel.SetEOLAsync(JSRuntime, eol);
                }
            }
        }

        return success;
    }
}
