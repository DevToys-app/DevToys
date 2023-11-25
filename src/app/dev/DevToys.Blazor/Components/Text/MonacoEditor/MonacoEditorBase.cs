using System.Text.Json;
using DevToys.Blazor.Components.Monaco;
using DevToys.Blazor.Components.Monaco.Editor;

namespace DevToys.Blazor.Components;

public abstract class MonacoEditorBase : MefComponentBase, IFocusable
{
    private static readonly object themeDefinedLock = new();
    private static bool isThemeDefined;

    protected bool _isDisposed;

    [Parameter]
    public EventCallback OnDidDispose { get; set; }

    [Parameter]
    public EventCallback OnTextModelInitializationRequested { get; set; }

    [Parameter]
    public EventCallback OnDidInit { get; set; }

    protected override Task OnInitializedAsync()
    {
        lock (themeDefinedLock)
        {
            if (!isThemeDefined)
            {
                isThemeDefined = true;
                MonacoEditorHelper.DefineThemeAsync(
                    JSRuntime,
                    "vs-dark",
                    new StandaloneThemeData
                    {
                        Base = "vs-dark",
                        Inherit = true,
                        Rules = new(),
                        Colors = new Dictionary<string, string>
                        {
                           { "foreground", "#FFFFFF" },
                           { "editor.foreground", "#FFFFFF" },
                           { "editor.background", "#00000000" },
                           { "editor.lineHighlightBackground", "#FFFFFF19" },
                           //{ "editorLineNumber.foreground", "#EEEEEE99" },
                           //{ "editorLineNumber.activeForeground", "#EEEEEE99" },
                           { "editor.inactiveSelectionBackground", "#00000000" },
                           { "editor.selectionForeground", "#FFFFFF" },
                           { "editor.selectionBackground", "#0079D6" },
                           { "editorWidget.background", "#252526" }
                        }
                    });
                MonacoEditorHelper.DefineThemeAsync(
                    JSRuntime,
                    "vs",
                    new StandaloneThemeData
                    {
                        Base = "vs",
                        Inherit = true,
                        Rules = new(),
                        Colors = new Dictionary<string, string>
                        {
                           { "foreground", "#000000" },
                           { "editor.foreground", "#000000" },
                           { "editor.background", "#FFFFFF00" },
                           { "editor.lineHighlightBackground", "#00000019" },
                           //{ "editorLineNumber.foreground", "#00000099" },
                           //{ "editorLineNumber.activeForeground", "#00000099" },
                           { "editor.inactiveSelectionBackground", "#00000000" },
                           { "editor.selectionForeground", "#000000" },
                           { "editor.selectionBackground", "#0079D6" }, // may need to change?
                           { "editorWidget.background", "#F3F3F3" }
                        }
                    });
            }
        }

        return base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await SetEventListenersAsync();
            await OnTextModelInitializationRequested.InvokeAsync(this);
            OnEditorLoaded();
            await OnDidInit.InvokeAsync(this);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    internal virtual async Task SetEventListenersAsync()
    {
        if (OnDidDispose.HasDelegate)
        {
            await SetEventListenerAsync(nameof(OnDidDispose));
        }
    }

    protected virtual void OnEditorLoaded()
    {
    }

    public virtual async Task EventCallbackAsync(string eventName, string eventJson)
    {
        switch (eventName)
        {
            case nameof(OnDidDispose):
                await OnDidDispose.InvokeAsync(this);
                break;
        }
    }

    internal ValueTask<bool> SetEventListenerAsync(string eventName)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.setEventListener", Id, eventName);

    public override async ValueTask DisposeAsync()
    {
        _isDisposed = true;

        try
        {
            await JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.dispose", Id);
        }
        catch
        {
            // ignore.
        }
        await base.DisposeAsync();
    }

    /// <summary>
    /// Get the editor type. Please see `EditorType`.
    /// This is to avoid an instanceof check
    /// </summary>
    internal ValueTask<string> GetEditorTypeAsync()
        => JSRuntime.InvokeAsync<string>("devtoys.MonacoEditor.getEditorType", Id);

    /// <summary>
    /// Instructs the editor to remeasure its container. This method should
    /// be called when the container of the editor gets resized.
    ///
    /// If a dimension is passed in, the passed in value will be used.
    /// </summary>
    internal ValueTask<string> LayoutAsync(Dimension? dimension = null)
        => JSRuntime.InvokeAsync<string>("devtoys.MonacoEditor.layout", Id, dimension);

    /// <summary>
    /// Brings browser focus to the editor text
    /// </summary>
    public ValueTask<bool> FocusAsync()
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.focus", Id);

    /// <summary>
    /// Returns true if the text inside this editor is focused (i.e. cursor is blinking).
    /// </summary>
    internal ValueTask<bool> HasTextFocusAsync()
        => JSRuntime.InvokeAsync<bool>("devtoys.MonacoEditor.hasTextFocus", Id);

    /// <summary>
    /// Given a position, returns a column number that takes tab-widths into account.
    /// </summary>
    internal ValueTask<int> GetVisibleColumnFromPositionAsync(Position position)
        => JSRuntime.InvokeAsync<int>("devtoys.MonacoEditor.getVisibleColumnFromPosition", Id, position);

    /// <summary>
    /// Returns the primary position of the cursor.
    /// </summary>
    internal ValueTask<Position> GetPositionAsync()
        => JSRuntime.InvokeAsync<Position>("devtoys.MonacoEditor.getPosition", Id);

    /// <summary>
    /// Set the primary position of the cursor. This will remove any secondary cursors.
    /// @param position New primary cursor's position
    /// @param source Source of the call that caused the position
    /// </summary>
    internal ValueTask<bool> SetPositionAsync(Position position, string source)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.setPosition", Id, position, source);

    /// <summary>
    /// Scroll vertically as necessary and reveal a line.
    /// </summary>
    internal ValueTask<bool> RevealLineAsync(int lineNumber, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.revealLine", Id, lineNumber, scrollType);

    /// <summary>
    /// Scroll vertically as necessary and reveal a line centered vertically.
    /// </summary>
    internal ValueTask<bool> RevealLineInCenterAsync(int lineNumber, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.revealLineInCenter", Id, lineNumber, scrollType);

    /// <summary>
    /// Scroll vertically as necessary and reveal a line centered vertically only if it lies outside the viewport.
    /// </summary>
    internal ValueTask<bool> RevealLineInCenterIfOutsideViewportAsync(int lineNumber, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.revealLineInCenterIfOutsideViewport", Id, lineNumber, scrollType);

    /// <summary>
    /// Scroll vertically or horizontally as necessary and reveal a position.
    /// </summary>
    internal ValueTask<bool> RevealPositionAsync(Position position, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.revealPosition", Id, position, scrollType);

    /// <summary>
    /// Scroll vertically or horizontally as necessary and reveal a position centered vertically.
    /// </summary>
    internal ValueTask<bool> RevealPositionInCenterAsync(Position position, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.revealPositionInCenter", Id, position, scrollType);

    /// <summary>
    /// Scroll vertically or horizontally as necessary and reveal a position centered vertically only if it lies outside the viewport.
    /// </summary>
    internal ValueTask<bool> RevealPositionInCenterIfOutsideViewportAsync(Position position, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.revealPositionInCenterIfOutsideViewport", Id, position, scrollType);

    /// <summary>
    /// Returns the primary selection of the editor.
    /// </summary>
    internal ValueTask<Selection> GetSelectionAsync()
        => JSRuntime.InvokeAsync<Selection>("devtoys.MonacoEditor.getSelection", Id);

    /// <summary>
    /// Returns all the selections of the editor.
    /// </summary>
    internal ValueTask<List<Selection>> GetSelectionsAsync()
        => JSRuntime.InvokeAsync<List<Selection>>("devtoys.MonacoEditor.getSelections", Id);

    /// <summary>
    /// Set the primary selection of the editor. This will remove any secondary cursors.
    /// @param selection The new selection
    /// </summary>
    internal ValueTask<bool> SetSelectionAsync(Monaco.Range selection)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.setSelection", Id, selection);

    /// <summary>
    /// Set the primary selection of the editor. This will remove any secondary cursors.
    /// @param selection The new selection
    /// </summary>
    internal ValueTask<bool> SetSelectionAsync(Selection selection)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.setSelection", Id, selection);

    /// <summary>
    /// Set the selections for all the cursors of the editor.
    /// Cursors will be removed or added, as necessary.
    /// @param selections The new selection
    /// </summary>
    internal ValueTask<bool> SetSelectionsAsync(List<Selection> selections)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.setSelections", Id, selections);

    /// <summary>
    /// Scroll vertically as necessary and reveal lines.
    /// </summary>
    internal ValueTask<bool> RevealLinesAsync(int startLineNumber, int endLineNumber, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.revealLines", Id, startLineNumber, endLineNumber, scrollType);

    /// <summary>
    /// Scroll vertically as necessary and reveal lines centered vertically.
    /// </summary>
    internal ValueTask<bool> RevealLinesInCenterAsync(int lineNumber, int endLineNumber, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.revealLinesInCenter", Id, lineNumber, endLineNumber, scrollType);

    /// <summary>
    /// Scroll vertically as necessary and reveal lines centered vertically only if it lies outside the viewport.
    /// </summary>
    internal ValueTask<bool> RevealLinesInCenterIfOutsideViewportAsync(int lineNumber, int endLineNumber, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.revealLinesInCenterIfOutsideViewport", Id, lineNumber, endLineNumber, scrollType);

    /// <summary>
    /// Scroll vertically or horizontally as necessary and reveal a range.
    /// </summary>
    internal ValueTask<bool> RevealRangeAsync(Monaco.Range range, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.revealRange", Id, range, scrollType);

    /// <summary>
    /// Scroll vertically or horizontally as necessary and reveal a range centered vertically.
    /// </summary>
    internal ValueTask<bool> RevealRangeInCenterAsync(Monaco.Range range, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.revealRangeInCenter", Id, range, scrollType);

    /// <summary>
    /// Scroll vertically or horizontally as necessary and reveal a range at the top of the viewport.
    /// </summary>
    internal ValueTask<bool> RevealRangeAtTopAsync(Monaco.Range range, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.revealRangeAtTop", Id, range, scrollType);

    /// <summary>
    /// Scroll vertically or horizontally as necessary and reveal a range centered vertically only if it lies outside the viewport.
    /// </summary>
    internal ValueTask<bool> RevealRangeInCenterIfOutsideViewportAsync(Monaco.Range range, ScrollType? scrollType = null)
        => JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.revealRangeInCenterIfOutsideViewport", Id, range, scrollType);

    /// <summary>
    /// Directly trigger a handler or an editor action.
    /// @param source The source of the call.
    /// @param handlerId The id of the handler or the id of a contribution.
    /// @param payload Extra data to be sent to the handler.
    /// </summary>
    internal ValueTask<bool> TriggerAsync(string source, string handlerId, object? payload = null)
    {
        JsonElement payloadJsonElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(payload));
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.trigger", Id, source, handlerId, payloadJsonElement);
    }
}
