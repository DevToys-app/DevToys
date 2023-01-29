namespace DevToys.Api;

/// <summary>
/// Provides a way to import MEF components on the fly.
/// </summary>
public interface IMefProvider
{
    /// <summary>
    /// Imports the given type.
    /// </summary>
    TExport Import<TExport>();

    /// <summary>
    /// Imports the given type.
    /// </summary>
    IEnumerable<Lazy<TExport, TMetadataView>> ImportMany<TExport, TMetadataView>();

    /// <summary>
    /// Satisfies the imports of the specified <paramref name="object"/> exactly once and they will not ever be recomposed.
    /// </summary>
    /// <param name="object">The object containing MEF components to import.</param>
    void SatisfyImports(object @object);
}
