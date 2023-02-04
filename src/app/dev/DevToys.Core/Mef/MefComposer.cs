using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using DevToys.Api;

namespace DevToys.Core.Mef;

/// <summary>
/// Provides a set of methods to initialize and manage MEF.
/// </summary>
public sealed class MefComposer : IDisposable
{
    private readonly Assembly[] _assemblies;
    private readonly object[] _customExports;
    private bool _isExportProviderDisposed = true;

    public IMefProvider Provider { get; }

    public CompositionContainer ExportProvider { get; private set; }

    public MefComposer(Assembly[]? assemblies = null, params object[] customExports)
    {
        if (Provider is not null)
        {
            throw new InvalidOperationException("Mef composer already initialized.");
        }

        _assemblies = assemblies ?? Array.Empty<Assembly>();
        _customExports = customExports ?? Array.Empty<object>();
        ExportProvider = InitializeMef();

        Provider = ExportProvider.GetExport<IMefProvider>()!.Value;
        ((MefProvider)Provider).ExportProvider = ExportProvider;
    }

    public void Dispose()
    {
        ExportProvider?.Dispose();

        _isExportProviderDisposed = true;
    }

    internal void Reset()
    {
        // For unit tests.
        Dispose();
        InitializeMef();
    }

    private CompositionContainer InitializeMef()
    {
        if (!_isExportProviderDisposed)
        {
            return ExportProvider;
        }

        var assemblies
            = new HashSet<Assembly>(_assemblies)
            {
                Assembly.GetExecutingAssembly()
            };

        // Discover MEF extensions coming from known assemblies.
        var catalog = new AggregateCatalog();
        foreach (Assembly assembly in assemblies)
        {
            catalog.Catalogs.Add(new AssemblyCatalog(assembly));
        }

        // Dynamically load assemblies coming from the Plugin repository folder and
        // try to discover MEF extensions in them.
        foreach (string pluginFolder in GetPotentialPluginFolders())
        {
            try
            {
                catalog.Catalogs.Add(new RecursiveDirectoryCatalog(pluginFolder));
            }
            catch (Exception ex)
            {
                // TODO: Log this. We maybe failed to load a plugin.
            }
        }

        // Compose MEF.
        var container = new CompositionContainer(catalog);
        var batch = new CompositionBatch();
        batch.AddPart(this);

        for (int i = 0; i < _customExports.Length; i++)
        {
            batch.AddPart(_customExports[i]);
        }

        container.Compose(batch);

        ExportProvider = container;

        _isExportProviderDisposed = false;

        return ExportProvider;
    }

    private static IEnumerable<string> GetPotentialPluginFolders()
    {
        // TODO: Maybe plugins should be placed in the app's LocalStorage instead?
        string appFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        string pluginFolder = Path.Combine(appFolder, "Plugins");
        return Directory.EnumerateDirectories(pluginFolder, "*", SearchOption.TopDirectoryOnly);
    }
}
