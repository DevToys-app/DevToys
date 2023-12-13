// This attribute will make the .NET runtime invoke the ClearCache and UpdateApplication methods when a hot reload
// is requested from Visual Studio / VS Code or Rider.
// More info: https://learn.microsoft.com/en-us/visualstudio/debugger/hot-reload-metadataupdatehandler
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(DevToys.Core.Debugger.HotReloadService))]

namespace DevToys.Core.Debugger;

[Export(typeof(HotReloadService))]
public sealed class HotReloadService
{
    [ImportingConstructor]
    public HotReloadService()
    {
        internalHotReloadRequestClearCache += (sender, args) => HotReloadRequestClearCache?.Invoke(sender, args);
        internalHotReloadRequestUpdateApplication += (sender, args) => HotReloadRequestUpdateApplication?.Invoke(sender, args);
    }

    public EventHandler<HotReloadEventArgs>? HotReloadRequestClearCache;

    public EventHandler<HotReloadEventArgs>? HotReloadRequestUpdateApplication;

    private static EventHandler<HotReloadEventArgs>? internalHotReloadRequestClearCache;

    private static EventHandler<HotReloadEventArgs>? internalHotReloadRequestUpdateApplication;

    /// <summary>
    /// Hot reload handler invoked by thanks to the MetadataUpdateHandler attribute.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Invoked by .NET runtime on Hot Reload.")]
    private static void ClearCache(Type[]? types)
    {
        if (System.Diagnostics.Debugger.IsAttached)
        {
            internalHotReloadRequestClearCache?.Invoke(null, new HotReloadEventArgs(types));
        }
    }

    /// <summary>
    /// Hot reload handler invoked by thanks to the MetadataUpdateHandler attribute.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Invoked by .NET runtime on Hot Reload.")]
    private static void UpdateApplication(Type[]? types)
    {
        if (System.Diagnostics.Debugger.IsAttached)
        {
            internalHotReloadRequestUpdateApplication?.Invoke(null, new HotReloadEventArgs(types));
        }
    }
}
