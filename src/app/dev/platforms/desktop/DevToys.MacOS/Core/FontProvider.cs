﻿using DevToys.Api;
using Microsoft.Extensions.Logging;
using Uno.Extensions;

namespace DevToys.MacOS.Core;

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
            return UIKit.UIFont.FamilyNames;
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
