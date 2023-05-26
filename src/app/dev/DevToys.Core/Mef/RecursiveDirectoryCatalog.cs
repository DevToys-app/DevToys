using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace DevToys.Core.Mef;

/// <summary>
/// Extends <see cref="DirectoryCatalog"/> to support discovery of parts in sub-directories.
/// </summary>
internal sealed class RecursiveDirectoryCatalog : ComposablePartCatalog, INotifyComposablePartCatalogChanged, ICompositionElement
{
    private readonly string _path;
    private AggregateCatalog? _aggregateCatalog;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecursiveDirectoryCatalog"/> class with <see cref="ComposablePartDefinition"/> objects based on all the DLL files in the specified directory path and its sub-directories.
    /// </summary>
    /// <param name="path">Path to the directory to scan for assemblies to add to the catalog.</param>
    public RecursiveDirectoryCatalog(string path)
        : this(path, "*.dll")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecursiveDirectoryCatalog"/> class with <see cref="ComposablePartDefinition"/> objects based on the specified search pattern in the specified directory path path and its sub-directories.
    /// </summary>
    /// <param name="path">Path to the directory to scan for assemblies to add to the catalog.</param>
    /// <param name="searchPattern">The pattern to search with. The format of the pattern should be the same as specified for GetFiles.</param>
    /// <exception cref="ArgumentNullException">The value of the <paramref name="path"/> parameter was <see langword="null"/>.</exception>
    public RecursiveDirectoryCatalog(string path, string searchPattern)
    {
        Guard.IsNotNull(path);

        _path = path;

        Initialize(path, searchPattern);
    }

    private static IEnumerable<string> GetFoldersRecursive(string path)
    {
        var result = new List<string> { path };
        foreach (string child in Directory.GetDirectories(path))
        {
            result.AddRange(GetFoldersRecursive(child));
        }
        return result;
    }

    private void Initialize(string path, string searchPattern)
    {
        IEnumerable<string> files
            = Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories);

        _aggregateCatalog = new AggregateCatalog();

        _aggregateCatalog.Changed += (o, e) =>
        {
            Changed?.Invoke(o, e);
        };

        _aggregateCatalog.Changing += (o, e) =>
        {
            Changing?.Invoke(o, e);
        };

        foreach (string file in files)
        {
            try
            {
                var asmCat = new AssemblyCatalog(file);

                // Force MEF to load the plugin and figure out if there are any exports
                // good assemblies will not throw the RTLE exception and can be added to the catalog
                if (asmCat.Parts.ToList().Count > 0)
                {
                    _aggregateCatalog.Catalogs.Add(asmCat);
                }
            }
            catch (ReflectionTypeLoadException)
            {
            }
            catch (BadImageFormatException)
            {
            }
        }
    }

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
}