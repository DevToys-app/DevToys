using System.Text.Json;
using System.Text.Json.Serialization;
using DevToys.Blazor.Components.Monaco;
using DevToys.Blazor.Components.Monaco.Editor;

///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components;

internal static class MonacoEditorHelper
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Create a new editor under `domElement`.
    /// `domElement` should be empty (not contain other dom nodes).
    /// The editor will read the size of `domElement`.
    /// </summary>
    internal static async ValueTask<MonacoEditor?> CreateMonacoEditorInstanceAsync(
        IJSRuntime runtime,
        string domElementId,
        StandaloneEditorConstructionOptions options,
        EditorOverrideServices? overrideServices,
        DotNetObjectReference<JSStyledComponentBase> dotnetObjectRef)
    {
        Guard.IsNotNull(runtime);
        Guard.IsNotNullOrWhiteSpace(domElementId);
        Guard.IsNotNull(options);
        Guard.IsNotNull(dotnetObjectRef);

        // Convert the options object into a JsonElement to get rid of the properties with null values
        string optionsJson = JsonSerializer.Serialize(options, jsonSerializerOptions);
        JsonElement optionsDict = JsonSerializer.Deserialize<JsonElement>(optionsJson);

        // Create the editor
        await runtime.InvokeVoidAsync(
            "devtoys.MonacoEditor.create",
            domElementId,
            optionsDict,
            overrideServices,
            dotnetObjectRef);

        return dotnetObjectRef.Value as MonacoEditor;
    }

    /// <summary>
    /// Create a new diff editor under `domElement`.
    /// `domElement` should be empty (not contain other dom nodes).
    /// The editor will read the size of `domElement`.
    /// </summary>
    internal static async ValueTask<MonacoEditorDiff?> CreateMonacoEditorDiffInstanceAsync(
        IJSRuntime runtime,
        string domElementId,
        StandaloneDiffEditorConstructionOptions options,
        EditorOverrideServices? overrideServices,
        DotNetObjectReference<JSStyledComponentBase> dotnetObjectRef,
        DotNetObjectReference<JSStyledComponentBase> dotnetObjectRefOriginal,
        DotNetObjectReference<JSStyledComponentBase> dotnetObjectRefModified)
    {
        options ??= new StandaloneDiffEditorConstructionOptions();

        // Convert the options object into a JsonElement to get rid of the properties with null values
        string optionsJson = JsonSerializer.Serialize(options, jsonSerializerOptions);
        JsonElement optionsDict = JsonSerializer.Deserialize<JsonElement>(optionsJson);

        // Create the editor
        await runtime.InvokeVoidAsync(
            "devtoys.MonacoEditor.createDiffEditor",
            domElementId,
            optionsDict,
            overrideServices,
            dotnetObjectRef,
            dotnetObjectRefOriginal,
            dotnetObjectRefModified);

        return dotnetObjectRef.Value as MonacoEditorDiff;
    }

    /// <summary>
    /// Create a new editor model.
    /// You can specify the language that should be set for this model or let the language be inferred from the `uri`.
    /// </summary>
    internal static ValueTask<TextModel> CreateModelAsync(IJSRuntime runtime, string value, string? language = null, string? uri = null)
        => runtime.InvokeAsync<TextModel>("devtoys.MonacoEditor.createModel", value, language, uri);

    /// <summary>
    /// Change the language for a model.
    /// </summary>
    internal static ValueTask<bool> SetModelLanguageAsync(IJSRuntime runtime, TextModel model, string languageId)
        => runtime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.setModelLanguage", model.Uri, languageId);

    /// <summary>
    /// Get the model that has `uri` if it exists.
    /// </summary>
    internal static ValueTask<TextModel> GetModelAsync(IJSRuntime runtime, string uri)
        => runtime.InvokeAsync<TextModel>("devtoys.MonacoEditor.getModel", uri);

    /// <summary>
    /// Get all the created models.
    /// </summary>
    internal static ValueTask<List<TextModel>> GetModelsAsync(IJSRuntime runtime)
        => runtime.InvokeAsync<List<TextModel>>("devtoys.MonacoEditor.getModels");

    /// <summary>
    /// Colorize the contents of `domNode` using attribute `data-lang`.
    /// </summary>
    internal static ValueTask<bool> ColorizeElementAsync(IJSRuntime runtime, string domNodeId, ColorizerElementOptions options)
        => runtime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.colorizeElement", domNodeId, options);

    /// <summary>
    /// Colorize `text` using language `languageId`.
    /// </summary>
    internal static ValueTask<string> ColorizeAsync(IJSRuntime runtime, string text, string languageId, ColorizerOptions options)
        => runtime.InvokeAsync<string>("devtoys.MonacoEditor.colorize", text, languageId, options);

    /// <summary>
    /// Colorize a line in a model.
    /// </summary>
    internal static ValueTask<string> ColorizeModelLineAsync(IJSRuntime runtime, TextModel model, int lineNumber, int? tabSize = null)
        => runtime.InvokeAsync<string>("devtoys.MonacoEditor.colorizeModelLine", model.Uri, lineNumber, tabSize);

    /// <summary>
    /// Define a new theme or update an existing theme.
    /// </summary>
    internal static ValueTask<bool> DefineThemeAsync(IJSRuntime runtime, string themeName, StandaloneThemeData themeData)
        => runtime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.defineTheme", themeName, themeData);

    /// <summary>
    /// Switches to a theme.
    /// </summary>
    internal static ValueTask<bool> SetThemeAsync(IJSRuntime runtime, string themeName)
        => runtime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.setTheme", themeName);

    /// <summary>
    /// Clears all cached font measurements and triggers re-measurement.
    /// </summary>
    internal static ValueTask<bool> RemeasureFontsAsync(IJSRuntime runtime)
        => runtime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.remeasureFonts");

    internal static MonacoEditor CreateVirtualEditor(IJSRuntime jsRuntime, string id, string? cssClass = null, ISettingsProvider? settingsProvider = null)
    {
        return new MonacoEditor(jsRuntime, id, cssClass, settingsProvider);
    }
}
