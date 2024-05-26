using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DevToys.Core.Mef;

/// <summary>
/// Provides a set of methods to initialize and manage MEF.
/// </summary>
public sealed partial class MefComposer : IDisposable
{
    public const string ExtraPluginEnvironmentVariableName = "EXTRAPLUGIN";

    private readonly ILogger _logger;
    private readonly Dictionary<string, Assembly> _loadedAssemblies = new();
    private readonly Assembly[] _assemblies;
    private readonly string[]? _pluginFolders;
    private readonly string? _extraPlugin;
    private readonly object[] _customExports;
    private bool _isExportProviderDisposed = true;

    public IMefProvider Provider { get; }

    public CompositionContainer ExportProvider { get; private set; }

    public MefComposer(Assembly[]? assemblies = null, string[]? pluginFolders = null, params object[] customExports)
    {
        _logger = this.Log();

        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentAppDomain_AssemblyResolve;
        AppDomain.CurrentDomain.AssemblyResolve += CurrentAppDomain_AssemblyResolve;

        _extraPlugin = Environment.GetEnvironmentVariable(ExtraPluginEnvironmentVariableName);

        _assemblies = assemblies ?? Array.Empty<Assembly>();
        _pluginFolders = pluginFolders;
        _customExports = customExports ?? Array.Empty<object>();
        ExportProvider = InitializeMef();

        Provider = ExportProvider.GetExport<IMefProvider>()!.Value;
        ((MefProvider)Provider).ExportProvider = ExportProvider;
    }

    private Assembly? CurrentAppDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        // Try to resolve the assembly from the list loaded assemblies.
        // We need to do this because when we load an assembly from MEF, the assembly may reference other assemblies
        // that isn't part of the folder where the assembly is, such as "DevToys.API" (extensions generally don't have it in their folder).

        string searchedAssemblyName = new AssemblyName(args.Name).Name!;

        if (_loadedAssemblies.TryGetValue(searchedAssemblyName, out Assembly? assembly))
        {
            return assembly;
        }

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        for (int i = 0; i < assemblies.Length; i++)
        {
            assembly = assemblies[i];
            string? assemblyName = assembly.GetName().Name;

            if (!string.IsNullOrEmpty(assemblyName))
            {
                _loadedAssemblies[assemblyName] = assembly;
                if (string.Equals(assemblyName, searchedAssemblyName, StringComparison.CurrentCulture))
                {
                    return assembly;
                }
            }
        }

        return null;
    }

    public void Dispose()
    {
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= CurrentAppDomain_AssemblyResolve;
        AppDomain.CurrentDomain.AssemblyResolve -= CurrentAppDomain_AssemblyResolve;
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

        int assemblyCount = assemblies.Count;

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
                var directoryCatalog = new RecursiveDirectoryCatalog(pluginFolder);
                assemblyCount += directoryCatalog.AssemblyCount;
                catalog.Catalogs.Add(directoryCatalog);
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
        Guard.IsNotNull(container.Catalog);
        LogMefComposition(container.Catalog.Count(), assemblyCount, (DateTime.Now - startTime).TotalMilliseconds);

        ExportProvider = container;

        _isExportProviderDisposed = false;

        return ExportProvider;
    }

    private IEnumerable<string> GetPotentialPluginFolders()
    {
        string[] pluginFolders;
        if (_pluginFolders is not null)
        {
            pluginFolders = _pluginFolders;
        }
        else
        {
            string appFolder = AppContext.BaseDirectory;
            pluginFolders = new[] { Path.Combine(appFolder!, "Plugins") };
        }

        if (!string.IsNullOrWhiteSpace(_extraPlugin) && Directory.Exists(_extraPlugin))
        {
            yield return _extraPlugin;
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

    [LoggerMessage(0, LogLevel.Information, "MEF composed {parts} parts from {assemblies} assemblies in {duration}ms")]
    partial void LogMefComposition(int parts, int assemblies, double duration);

    [LoggerMessage(1, LogLevel.Information, "Discovering plugin in '{pluginFolder}'...")]
    partial void LogDiscoveringPlugin(string pluginFolder);

    [LoggerMessage(2, LogLevel.Error, "Unable to load plugin in '{pluginFolder}'...")]
    partial void LogDiscoveringPluginFault(Exception ex, string pluginFolder);
}
