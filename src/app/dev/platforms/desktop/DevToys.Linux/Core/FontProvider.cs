using DevToys.Api;
using Microsoft.Extensions.Logging;

namespace DevToys.Linux.Core;

[Export(typeof(IFontProvider))]
internal sealed partial class FontProvider : IFontProvider
{
    public string[] GetFontFamilies()
    {
        return Array.Empty<string>(); // TODO: Implement.
    }
}
