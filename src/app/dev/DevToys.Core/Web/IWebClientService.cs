namespace DevToys.Core.Web;

/// <summary>
/// Represents a service that provides methods to interact with web resources.
/// </summary>
public interface IWebClientService
{
    /// <summary>
    /// Asynchronously downloads the requested resource as a string without crashing.
    /// If an error occurs, the method will return <see langword="null"/>.
    /// </summary>
    Task<string?> SafeGetStringAsync(Uri uri, CancellationToken cancellationToken);
}
