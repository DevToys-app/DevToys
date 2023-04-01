using DevToys.Api.Core;
using Microsoft.Extensions.Logging;
using Uno.Extensions;

namespace DevToys.MauiBlazor.Core.FontProvider;

[Export(typeof(IFontProvider))]
internal sealed partial class FontProvider : IFontProvider
{
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
#if __MACCATALYST__
            return UIKit.UIFont.FamilyNames;
#else
            return Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
#endif
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
