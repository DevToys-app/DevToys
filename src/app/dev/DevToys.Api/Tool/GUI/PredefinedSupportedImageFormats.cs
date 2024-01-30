namespace DevToys.Api;

/// <summary>
/// Represents a predefined list of supported image formats.
/// </summary>
public static class PredefinedSupportedImageFormats
{
    /// <summary>
    /// A list of file extensions representing image formats supported by default.
    /// They include BMP, GIF, JPEG, PBM, PNG, TIFF, TGA, WEBP formats.
    /// </summary>
    public static readonly string[] ImageFileExtensions
        = new[] { ".bmp", ".gif", ".jpeg", ".jpg", ".pbm", ".png", ".tiff", ".tga", ".webp" };
}
