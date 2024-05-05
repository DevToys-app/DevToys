namespace DevToys.Core.Version;

/// <summary>
/// Represents a service that provides information about the version of the app.
/// </summary>
public interface IVersionService
{
    /// <summary>
    /// Indicates whether the current instance of the app is a preview/beta version.
    /// </summary>
    bool IsPreviewVersion();
}
