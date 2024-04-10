using System.Text;
using DevToys.Core.Tools;
using Microsoft.Extensions.Logging;

namespace DevToys.Blazor.Core.Services;

public sealed partial class FontService
{
    private readonly ILogger _logger;
    private readonly IJSRuntime _jsRuntime;

#pragma warning disable IDE0044 // Add readonly modifier
    [ImportMany]
    private IEnumerable<Lazy<IResourceAssemblyIdentifier, ResourceAssemblyIdentifierMetadata>> _resourceAssemblyIdentifiers = default!;
#pragma warning restore IDE0044 // Add readonly modifier

    public FontService(IMefProvider mefProvider, IJSRuntime jsRuntime)
    {
        _logger = this.Log();
        _jsRuntime = jsRuntime;
        mefProvider.SatisfyImports(this);
    }

    internal async Task ImportThirdPartyFontsAsync()
    {
        DateTime startTime = DateTime.Now;
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);

        var tasks = new List<Task<string>>();
        var fontNames = new HashSet<string>();

        await foreach (FontDefinition fontDefinition in GetFontDefinitionsAsync())
        {
            if (fontNames.Add(fontDefinition.FontFamily))
            {
                tasks.Add(GenerateCssForFontAsync(fontDefinition));
            }
            else
            {
                LogFontAlreadyRegistered(fontDefinition.FontFamily);
            }
        }

        var allFontCss = new StringBuilder();

        string[] fonts = await Task.WhenAll(tasks);
        for (int i = 0; i < fonts.Length; i++)
        {
            allFontCss.AppendLine(fonts[i]);
        }

        double loadDuration = (DateTime.Now - startTime).TotalMilliseconds;
        startTime = DateTime.Now;

        await _jsRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.addFontToDocument", allFontCss.ToString());

        LogImportThirdPartyFontsAsync(fonts.Length, loadDuration, (DateTime.Now - startTime).TotalMilliseconds);
    }

    private async IAsyncEnumerable<FontDefinition> GetFontDefinitionsAsync()
    {
        var tasks = new List<Task<FontDefinition[]>>();

        foreach (Lazy<IResourceAssemblyIdentifier, ResourceAssemblyIdentifierMetadata> resourceAssemblyIdentifier in _resourceAssemblyIdentifiers)
        {
            tasks.Add(GetFontDefinitionsAsync(resourceAssemblyIdentifier));
        }

        while (tasks.Count > 0)
        {
            var task = await Task.WhenAny(tasks);
            tasks.Remove(task);
            FontDefinition[] fontDefinitions = await task.ConfigureAwait(false);
            for (int i = 0; i < fontDefinitions.Length; i++)
            {
                yield return fontDefinitions[i];
            }
        }
    }

    private async Task<FontDefinition[]> GetFontDefinitionsAsync(Lazy<IResourceAssemblyIdentifier, ResourceAssemblyIdentifierMetadata> resourceAssemblyIdentifier)
    {
        try
        {
            return await resourceAssemblyIdentifier.Value.GetFontDefinitionsAsync();
        }
        catch (NotImplementedException) { }
        catch (Exception ex)
        {
            LogGetFontDefinitionsAsyncFailed(ex, resourceAssemblyIdentifier.Metadata.InternalComponentName);
        }

        return Array.Empty<FontDefinition>();
    }

    private async Task<string> GenerateCssForFontAsync(FontDefinition fontDefinition)
    {
        try
        {
            string base64FontString = await ConvertFontToBase64Async(fontDefinition);

            var cssStringBuilder = new StringBuilder();

            cssStringBuilder.AppendLine("@font-face {");
            cssStringBuilder.AppendLine($"    font-family: \"{fontDefinition.FontFamily}\";");
            cssStringBuilder.AppendLine($"    src: url(data:application/font-ttf;charset=utf-8;base64,{base64FontString}) format('truetype');");
            cssStringBuilder.AppendLine("}");
            cssStringBuilder.AppendLine("");
            cssStringBuilder.AppendLine($"i[class^=\"{fontDefinition.FontFamily}\"]:before, i[class*=\" {fontDefinition.FontFamily}\"]:before {{");
            cssStringBuilder.AppendLine($"    font-family: {fontDefinition.FontFamily} !important;");
            cssStringBuilder.AppendLine("    font-style: normal;");
            cssStringBuilder.AppendLine("    font-weight: normal !important;");
            cssStringBuilder.AppendLine("    font-variant: normal;");
            cssStringBuilder.AppendLine("    text-transform: none;");
            cssStringBuilder.AppendLine("    line-height: 1;");
            cssStringBuilder.AppendLine("    -webkit-font-smoothing: antialiased;");
            cssStringBuilder.AppendLine("    -moz-osx-font-smoothing: grayscale;");
            cssStringBuilder.AppendLine("}");

            return cssStringBuilder.ToString();
        }
        catch (Exception ex)
        {
            LogGenerateCssForFontAsyncFailed(ex, fontDefinition.FontFamily);
        }

        return string.Empty;
    }

    private static async Task<string> ConvertFontToBase64Async(FontDefinition fontDefinition)
    {
        byte[] buffer = new byte[fontDefinition.FontReader.Length];

        await fontDefinition.FontReader.ReadAsync(buffer);

        string base64String = Convert.ToBase64String(buffer);

        fontDefinition.Dispose();
        return base64String;
    }

    [LoggerMessage(0, LogLevel.Error, "Unexpectedly failed to get font definitions from '{resourceAssemblyIdentifierName}'.")]
    partial void LogGetFontDefinitionsAsyncFailed(Exception ex, string resourceAssemblyIdentifierName);

    [LoggerMessage(1, LogLevel.Error, "Unexpectedly failed to load the font '{fontName}'.")]
    partial void LogGenerateCssForFontAsyncFailed(Exception ex, string fontName);

    [LoggerMessage(2, LogLevel.Information, "Loaded {fontCount} font(s) in {loadingDuration} ms and injected them in {injectionDuration} ms")]
    partial void LogImportThirdPartyFontsAsync(int fontCount, double loadingDuration, double injectionDuration);

    [LoggerMessage(3, LogLevel.Warning, "The font {fontName} has already been registered, maybe by another extension.")]
    partial void LogFontAlreadyRegistered(string fontName);
}
