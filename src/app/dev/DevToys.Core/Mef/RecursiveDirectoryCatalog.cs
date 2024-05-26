using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DevToys.Core.Mef;

/// <summary>
/// Extends <see cref="DirectoryCatalog"/> to support discovery of parts in sub-directories.
/// </summary>
internal sealed partial class RecursiveDirectoryCatalog : ComposablePartCatalog, INotifyComposablePartCatalogChanged,
    ICompositionElement
{
    private readonly ILogger _logger;
    private readonly string _path;
    private AggregateCatalog? _aggregateCatalog;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecursiveDirectoryCatalog"/> class with <see cref="ComposablePartDefinition"/> objects based on all the DLL files in the specified directory path and its sub-directories.
    /// </summary>
    /// <param name="path">Path to the directory to scan for assemblies to add to the catalog.</param>
    public RecursiveDirectoryCatalog(string path)
        : this(
              path,
              "*.dll",
              // Perf: Ignore resource assembly as they only don't contain MEF parts.
              (file) => !file.EndsWith(".resources.dll"))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecursiveDirectoryCatalog"/> class with <see cref="ComposablePartDefinition"/> objects based on the specified search pattern in the specified directory path path and its sub-directories.
    /// </summary>
    /// <param name="path">Path to the directory to scan for assemblies to add to the catalog.</param>
    /// <param name="searchPattern">The pattern to search with. The format of the pattern should be the same as specified for GetFiles.</param>
    /// <param name="pathFilter">A filter to apply to files.</param>
    /// <exception cref="ArgumentNullException">The value of the <paramref name="path"/> parameter was <see langword="null"/>.</exception>
    public RecursiveDirectoryCatalog(string path, string searchPattern, Predicate<string>? pathFilter)
    {
        Guard.IsNotNull(path);

        _path = path;
        _logger = this.Log();

        Initialize(path, searchPattern, pathFilter);
    }

    /// <summary>
    /// The number of assemblies read from the directory catalog.
    /// </summary>
    internal int AssemblyCount { get; private set; }

    /// <summary>
    /// Gets the part definitions that are contained in the recursive directory catalog. (Overrides ComposablePartCatalog.Parts.)
    /// </summary>
    public override IQueryable<ComposablePartDefinition> Parts
    {
        get
        {
            Guard.IsNotNull(_aggregateCatalog);
            return _aggregateCatalog.Parts;
        }
    }

    /// <summary>
    /// Occurs when the contents of the catalog has changed.
    /// </summary>
    public event EventHandler<ComposablePartCatalogChangeEventArgs>? Changed;

    /// <summary>
    /// Occurs when the catalog is changing.
    /// </summary>
    public event EventHandler<ComposablePartCatalogChangeEventArgs>? Changing;

    private void Initialize(string path, string searchPattern, Predicate<string>? pathFilter)
    {
        IEnumerable<string> files
            = Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories);

        if (pathFilter is not null)
        {
            files = files.Where(f => pathFilter(f));
        }

        _aggregateCatalog = new AggregateCatalog();

        _aggregateCatalog.Changed += (o, e) =>
        {
            Changed?.Invoke(o, e);
        };

        _aggregateCatalog.Changing += (o, e) =>
        {
            Changing?.Invoke(o, e);
        };

        string pluginFolderName = Path.GetFileName(path);
        string hypotheticalPluginMainDllFile = $"{pluginFolderName}.dll";
        bool foundMefPartInMainDllFile = false;
        string? pluginMainDllFile = files.FirstOrDefault(f => f.EndsWith(hypotheticalPluginMainDllFile, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(pluginMainDllFile) && File.Exists(pluginMainDllFile))
        {
            // Perf: If the main plugin DLL is found, try to load it first.
            // This is to avoid loading all the other DLLs in the directory, which slows down the app's startup.
            foundMefPartInMainDllFile = AddLibraryToMefCatalog(pluginMainDllFile);
        }

        if (!foundMefPartInMainDllFile)
        {
            foreach (string file in files)
            {
                AddLibraryToMefCatalog(file);
            }
        }
    }

    private bool AddLibraryToMefCatalog(string filePath)
    {
        Guard.IsNotNull(_aggregateCatalog);

        try
        {
            AssemblyCount++;
            var asmCat = new AssemblyCatalog(filePath);

            // Force MEF to load the plugin and figure out if there are any exports
            // good assemblies will not throw the RTLE exception and can be added to the catalog
            if (asmCat.Parts.Any())
            {
                _aggregateCatalog.Catalogs.Add(asmCat);
                return true;
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            LogLoadLibraryFault(ex, filePath);
        }
        catch (BadImageFormatException)
        {
        }

        return false;
    }

    private string GetDisplayName()
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            "{0} (RecusrivePath=\"{1}\")", new[] { GetType().Name, _path });
    }

    public override string ToString()
    {
        return GetDisplayName();
    }

    /// <summary>
    /// Gets the display name of the directory catalog.
    /// </summary>
    string ICompositionElement.DisplayName => GetDisplayName();

    /// <summary>
    /// Gets the composition element from which the directory catalog originated.
    /// </summary>
    ICompositionElement? ICompositionElement.Origin => null;

    [LoggerMessage(0, LogLevel.Warning, "Unable to load library '{filePath}'...")]
    partial void LogLoadLibraryFault(Exception ex, string filePath);
}
