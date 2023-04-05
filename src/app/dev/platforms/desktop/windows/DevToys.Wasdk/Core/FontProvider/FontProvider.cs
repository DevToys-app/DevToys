using DevToys.Api.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas.Text;
using Uno.Extensions;

namespace DevToys.Wasdk.Core.FontProvider;

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
            return CanvasTextFormat.GetSystemFontFamilies();
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
