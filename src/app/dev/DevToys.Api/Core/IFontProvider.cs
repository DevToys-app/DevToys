namespace DevToys.Api;

/// <summary>
/// Provides a platform agnostic way to retrieve information about fonts installed on the operating system.
/// </summary>
public interface IFontProvider
{
    /// <summary>
    /// Retrieves the list of font families available on the operating system.
    /// </summary>
    string[] GetFontFamilies();
}
