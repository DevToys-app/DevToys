using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DevToys.Core.Mef;

/// <summary>
/// Provides a set of methods to initialize and manage MEF.
/// </summary>
public sealed partial class MefComposer : IDisposable
{
    private readonly ILogger _logger;
    private readonly Assembly[] _assemblies;
    private readonly string[]? _pluginFolders;
    private readonly object[] _customExports;
    private bool _isExportProviderDisposed = true;

    public IMefProvider Provider { get; }

    public CompositionContainer ExportProvider { get; private set; }

    public MefComposer(Assembly[]? assemblies = null, string[]? pluginFolders = null, params object[] customExports)
    {
        if (Provider is not null)
        {
            throw new InvalidOperationException("Mef composer already initialized.");
        }

        _logger = this.Log();

        _assemblies = assemblies ?? Array.Empty<Assembly>();
        _pluginFolders = pluginFolders;
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

        DateTime startTime = DateTime.Now;
        var assemblies
            = new HashSet<Assembly>(_assemblies)
            {
                Assembly.GetEntryAssembly()!,  // .exe
                typeof(IMefProvider).Assembly, // DevToys.API
                typeof(MefComposer).Assembly,  // DevToys.Core
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
            LogDiscoveringPlugin(pluginFolder);
            try
            {
                catalog.Catalogs.Add(new RecursiveDirectoryCatalog(pluginFolder));
            }
            catch (Exception ex)
            {
                LogDiscoveringPluginFault(ex, pluginFolder);
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
        LogMefComposition((DateTime.Now - startTime).TotalMilliseconds);

        ExportProvider = container;

        _isExportProviderDisposed = false;

        return ExportProvider;
    }

    private IEnumerable<string> GetPotentialPluginFolders()
    {
        // TODO: Maybe plugins should be placed in the app's LocalStorage instead?
        string appFolder = AppContext.BaseDirectory;
        if (!string.IsNullOrEmpty(appFolder))
        {
            string[] pluginFolders;
            if (_pluginFolders is not null)
            {
                pluginFolders = _pluginFolders;
            }
            else
            {
                pluginFolders = new[] { Path.Combine(appFolder!, "Plugins") };
            }

            for (int i = 0; i < pluginFolders.Length; i++)
            {
                string pluginFolder = pluginFolders[i];
                if (Directory.Exists(pluginFolder))
                {
                    foreach (string folder in Directory.EnumerateDirectories(pluginFolder, "*", SearchOption.TopDirectoryOnly))
                    {
                        yield return folder;
                    }
                }
            }
        }
    }

    [LoggerMessage(0, LogLevel.Information, "MEF composed in {duration} ms")]
    partial void LogMefComposition(double duration);

    [LoggerMessage(1, LogLevel.Information, "Discovering plugin in '{pluginFolder}'...")]
    partial void LogDiscoveringPlugin(string pluginFolder);

    [LoggerMessage(2, LogLevel.Error, "Unable to load plugin in '{pluginFolder}'...")]
    partial void LogDiscoveringPluginFault(Exception ex, string pluginFolder);
}
