namespace DevToys.Api;

/// <summary>
/// Represents a font definition.
/// </summary>
public sealed class FontDefinition : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FontDefinition"/> class.
    /// </summary>
    /// <param name="fontFamily">The font family.</param>
    /// <param name="fontReader">The stream to read the font file.</param>
    public FontDefinition(string fontFamily, Stream fontReader)
    {
        Guard.IsNotNullOrWhiteSpace(fontFamily);
        Guard.IsNotNull(fontReader);
        Guard.CanRead(fontReader);
        FontFamily = fontFamily;
        FontReader = fontReader;
    }

    /// <summary>
    /// Gets the font family.
    /// </summary>
    public string FontFamily { get; }

    /// <summary>
    /// Gets the stream to read the font file.
    /// </summary>
    public Stream FontReader { get; }

    /// <summary>
    /// Disposes the font reader.
    /// </summary>
    public void Dispose()
    {
        FontReader?.Dispose();
    }
}
