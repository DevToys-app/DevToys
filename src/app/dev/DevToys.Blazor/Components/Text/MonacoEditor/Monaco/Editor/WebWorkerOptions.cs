///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class WebWorkerOptions
{
    /// <summary>
    /// The AMD moduleId to load.
    /// It should export a function `create` that should return the exported proxy.
    /// </summary>
    public string? ModuleId { get; set; }

    /// <summary>
    /// The data to send over when calling create on the module.
    /// </summary>
    public object? CreateData { get; set; }

    /// <summary>
    /// A label to be used to identify the web worker for debugging purposes.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// An object that can be used by the web worker to make calls back to the main thread.
    /// </summary>
    public object? Host { get; set; }

    /// <summary>
    /// Keep idle models.
    /// Defaults to false, which means that idle models will stop syncing after a while.
    /// </summary>
    public bool? KeepIdleModels { get; set; }
}
