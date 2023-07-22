using System.Windows.Media;
using DevToys.Api;
using Microsoft.Extensions.Logging;

namespace DevToys.Windows.Core;

[Export(typeof(IFontProvider))]
internal sealed partial class FontProvider : IFontProvider
{
    private readonly Lazy<string[]> _fontFamilies
        = new(() => Fonts.SystemFontFamilies.SelectMany(f => f.FamilyNames.Values).Order().ToArray());

    private readonly ILogger _logger;

    [ImportingConstructor]
    public FontProvider()
    {
        _logger = this.Log();
    }

    public string[] GetFontFamilies()
    {
        try
        {
            return _fontFamilies.Value;
        }
        catch (Exception ex)
        {
            LogGetFontFamiliesFailed(ex);
        }

        return Array.Empty<string>();
    }

    [LoggerMessage(1, LogLevel.Error, "Failed to retrieve the list of fonts installed on the system.")]
    partial void LogGetFontFamiliesFailed(Exception ex);
}
