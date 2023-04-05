using DevToys.Api.Core;

namespace DevToys.Wasm.Core.FontProvider;

[Export(typeof(IFontProvider))]
internal sealed class FontProvider : IFontProvider
{
    public string[] GetFontFamilies()
    {
        // TODO
        throw new NotImplementedException();
    }
}
