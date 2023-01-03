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
}
