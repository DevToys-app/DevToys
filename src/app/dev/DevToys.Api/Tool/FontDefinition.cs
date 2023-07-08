namespace DevToys.Api;

public sealed class FontDefinition : IDisposable
{
    public FontDefinition(string fontFamily, Stream fontReader)
    {
        Guard.IsNotNullOrWhiteSpace(fontFamily);
        Guard.IsNotNull(fontReader);
        Guard.CanRead(fontReader);
        FontFamily = fontFamily;
        FontReader = fontReader;
    }

    public string FontFamily { get; }

    public Stream FontReader { get; }

    public void Dispose()
    {
        FontReader?.Dispose();
    }
}
