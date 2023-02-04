#if __MACCATALYST__

using System.Text;

namespace DevToys.MonacoEditor.HighlightJs;

/// <summary>
/// Theme parser, can be used to configure the theme parameters. 
/// </summary>
internal sealed class Theme
{
    private readonly Dictionary<string, Dictionary<string, string?>> _strippedTheme;
    private Dictionary<string, UIStringAttributes> _themeDict;

    public Theme(string currentThemeString)
    {
        CurrentTheme = currentThemeString;
        _strippedTheme = StripTheme(currentThemeString);
        SetCodeFont(UIFont.FromName("Courier New", size: 16));
        LightTheme = StrippedThemeToString(_strippedTheme);
        _themeDict = StrippedThemeToTheme(_strippedTheme);

        string? bkgColorHex = string.Empty;
        if (_strippedTheme.TryGetValue(".hljs", out Dictionary<string, string?>? st))
        {
            if (!st.TryGetValue("background", out bkgColorHex))
            {
                st.TryGetValue("background-color", out bkgColorHex);
            }
        }

        if (!string.IsNullOrEmpty(bkgColorHex))
        {
            if (string.Equals(bkgColorHex, "white", StringComparison.Ordinal))
            {
                ThemeBackgroundColor = new UIColor(white: 1, alpha: 1);
            }
            else if (string.Equals(bkgColorHex, "black", StringComparison.Ordinal))
            {
                ThemeBackgroundColor = new UIColor(white: 0, alpha: 1);
            }
            else
            {
                ThemeBackgroundColor = ColorWithHexString(bkgColorHex);
            }
        }
        else
        {
            ThemeBackgroundColor = UIColor.White;
        }
    }

    internal string CurrentTheme { get; }

    internal string LightTheme { get; }

    /// <summary>
    /// Regular font to be used by this theme
    /// </summary>
    internal UIFont CodeFont { get; set; } = null!;

    /// <summary>
    /// Bold font to be used by this theme
    /// </summary>
    internal UIFont BoldCodeFont { get; set; } = null!;

    /// <summary>
    /// Italic font to be used by this theme
    /// </summary>
    internal UIFont ItalicCodeFont { get; set; } = null!;

    /// <summary>
    /// Default background color for the current theme.
    /// </summary>
    internal UIColor ThemeBackgroundColor { get; }

    internal NSAttributedString ApplyStyleToString(string text, IReadOnlyList<string> styleList)
    {
        var attrs = new UIStringAttributes { Font = CodeFont };

        if (styleList.Count > 0)
        {
            for (int i = 0; i < styleList.Count; i++)
            {
                string style = styleList[i];

                if (styleList.Contains("hljs-title")
                    && styleList.Contains("hljs-function")
                    && _themeDict.TryGetValue("hljs-function-hljs-title", out UIStringAttributes? _))
                {
                    style = "hljs-function-hljs-title";
                }

                if (styleList.Contains("hljs-title")
                    && styleList.Contains("hljs-class")
                    && _themeDict.TryGetValue("hljs-class-hljs-title", out UIStringAttributes? _))
                {
                    style = "hljs-class-hljs-title";
                }
                
                if (_themeDict.TryGetValue(style, out UIStringAttributes? themeStyle))
                {
                    foreach (KeyValuePair<NSObject, NSObject> keyValuePair in themeStyle.Dictionary)
                    {
                        NSObject attrName = keyValuePair.Key;
                        NSObject attrValue = keyValuePair.Value;
                        attrs.Dictionary[attrName] = attrValue;
                    }
                }
            }
        }
        else
        {
            attrs.Dictionary[UIStringAttributeKey.Font] = CodeFont;
        }

        var returnString = new NSAttributedString(text, attrs);

        return returnString;
    }

    private static Dictionary<string, Dictionary<string, string?>> StripTheme(string themeString)
    {
        var objcString = new NSString(themeString);
        var cssRegex
            = new NSRegularExpression(
                new NSString("(?:(\\.[a-zA-Z0-9\\-_]*(?:[, ]\\.[a-zA-Z0-9\\-_]*)*)\\{([^\\}]*?)\\})"),
                NSRegularExpressionOptions.CaseInsensitive,
                out _);

        NSTextCheckingResult[] results
            = cssRegex.GetMatches(
                objcString,
                NSMatchingOptions.ReportCompletion,
                new NSRange(0, themeString.Length));

        var resultDict = new Dictionary<string, Dictionary<string, string?>>();

        for (int i = 0; i < results.Length; i++)
        {
            NSTextCheckingResult result = results[i];
            if (result.NumberOfRanges == 3)
            {
                var attributes = new Dictionary<string, string?>();
                NSRange secondRange = result.RangeAtIndex(2);
                string[] cssPairs
                    = themeString.Substring(
                            (int)secondRange.Location,
                            (int)secondRange.Length)
                        .Split(';');

                for (int j = 0; j < cssPairs.Length; j++)
                {
                    string pair = cssPairs[j];
                    string[] cssPropComp = pair.Split(':');
                    if (cssPropComp.Length == 2)
                    {
                        attributes[cssPropComp[0]] = cssPropComp[1];
                    }
                }

                if (attributes.Count > 0)
                {
                    NSRange firstRange = result.RangeAtIndex(1);
                    resultDict[themeString.Substring((int)firstRange.Location, (int)firstRange.Length)] = attributes;
                }
            }
        }

        var returnDict = new Dictionary<string, Dictionary<string, string?>>();

        foreach ((string? keys, Dictionary<string, string?>? result) in resultDict)
        {
            string[] keyArray = keys.Replace(" ", ",").Split(",");
            for (int i = 0; i < keyArray.Length; i++)
            {
                string key = keyArray[i];
                if (keyArray.Contains(".hljs-title") && keyArray.Contains(".hljs-function"))
                {
                    key = "hljs-function-hljs-title";
                }

                if (keyArray.Contains(".hljs-title") && keyArray.Contains(".hljs-class"))
                {
                    key = "hljs-class-hljs-title";
                }
                
                if (!returnDict.TryGetValue(key, out Dictionary<string, string?>? props))
                {
                    props = new();
                }

                foreach ((string pName, string? pValue) in result)
                {
                    props[pName] = pValue;
                }

                returnDict[key] = props;
            }
        }

        return returnDict;
    }

    /// <summary>
    /// Changes the theme font. This will try to automatically populate the codeFont, boldCodeFont and italicCodeFont properties based on the provided font.
    /// </summary>
    /// <param name="font"></param>
    private void SetCodeFont(UIFont font)
    {
        CodeFont = font;

        var boldDescriptor
            = UIFontDescriptor.FromAttributes(
                new UIFontAttributes { Family = font.FamilyName, Face = "Bold" });

        var italicDescriptor
            = UIFontDescriptor.FromAttributes(
                new UIFontAttributes { Family = font.FamilyName, Face = "Italic" });

        var obliqueDescriptor
            = UIFontDescriptor.FromAttributes(
                new UIFontAttributes { Family = font.FamilyName, Face = "Oblique" });

        BoldCodeFont = UIFont.FromDescriptor(boldDescriptor, font.PointSize);
        ItalicCodeFont = UIFont.FromDescriptor(italicDescriptor, font.PointSize);

        if (ItalicCodeFont == null || ItalicCodeFont.FamilyName != font.FamilyName)
        {
            ItalicCodeFont = UIFont.FromDescriptor(obliqueDescriptor, font.PointSize);
        }

        if (ItalicCodeFont == null)
        {
            ItalicCodeFont = font;
        }

        if (BoldCodeFont == null)
        {
            BoldCodeFont = font;
        }

        _themeDict = StrippedThemeToTheme(_strippedTheme);
    }

    private static string StrippedThemeToString(Dictionary<string, Dictionary<string, string?>> theme)
    {
        var resultString = new StringBuilder();

        foreach ((string? key, Dictionary<string, string?>? props) in theme)
        {
            resultString.Append(key).Append('{');

            foreach ((string cssProp, string? val) in props)
            {
                if (!string.Equals(key, ".hljs", StringComparison.OrdinalIgnoreCase)
                    || (!string.Equals(cssProp, "background-color", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(cssProp, "background", StringComparison.OrdinalIgnoreCase)))
                {
                    resultString
                        .Append(cssProp)
                        .Append(':')
                        .Append(val);
                }
            }

            resultString.Append('{');
        }

        return resultString.ToString();
    }

    private Dictionary<string, UIStringAttributes> StrippedThemeToTheme(Dictionary<string, Dictionary<string, string?>> theme)
    {
        var returnTheme = new Dictionary<string, UIStringAttributes>();
        foreach ((string? className, Dictionary<string, string?>? props) in theme)
        {
            var keyProps = new UIStringAttributes();
            foreach ((string key, string? prop) in props)
            {
                switch (key)
                {
                    case "color":
                        keyProps.Dictionary[AttributeForCssKey(key)] = ColorWithHexString(prop);
                        break;
                    case "font-style":
                        keyProps.Dictionary[AttributeForCssKey(key)] = FontForCssStyle(prop);
                        break;
                    case "font-weight":
                        keyProps.Dictionary[AttributeForCssKey(key)] = FontForCssStyle(prop);
                        break;
                    case "background-color":
                        keyProps.Dictionary[AttributeForCssKey(key)] = ColorWithHexString(prop);
                        break;
                }

                if (keyProps.Dictionary.Count > 0)
                {
                    returnTheme[className.Replace(".", string.Empty)] = keyProps;
                }
            }
        }

        return returnTheme;
    }

    private static NSString AttributeForCssKey(string key)
    {
        return key switch
        {
            "color" => UIStringAttributeKey.ForegroundColor,
            "font-weight" => UIStringAttributeKey.Font,
            "font-style" => UIStringAttributeKey.Font,
            "background-color" => UIStringAttributeKey.BackgroundColor,
            _ => UIStringAttributeKey.Font
        };
    }

    private static UIColor ColorWithHexString(string? hex)
    {
        string? cString = hex?.Trim(' ', '\t', '\n', '\r');

        if (!string.IsNullOrEmpty(cString) && cString.StartsWith("#"))
        {
            cString = cString.Substring(1);
        }
        else
        {
            return cString switch
            {
                "white" => new UIColor(white: 1, alpha: 1),
                "black" => new UIColor(white: 0, alpha: 1),
                "red" => new UIColor(red: 1, green: 0, blue: 0, alpha: 1),
                "green" => new UIColor(red: 0, green: 1, blue: 0, alpha: 1),
                "blue" => new UIColor(red: 0, green: 0, blue: 1, alpha: 1),
                _ => UIColor.Gray
            };
        }

        if (cString.Length != 6 && cString.Length != 3)
        {
            return UIColor.Gray;
        }

        long r;
        long g;
        long b;
        double divisor;

        if (cString.Length == 6)
        {
            string rString = cString.Substring(0, 2);
            string gString = cString.Substring(2, 2);
            string bString = cString.Substring(4, 2);
            r = Convert.ToInt64(rString, 16);
            g = Convert.ToInt64(gString, 16);
            b = Convert.ToInt64(bString, 16);
            divisor = 255.0;
        }
        else
        {
            string rString = cString.Substring(0, 1);
            string gString = cString.Substring(1, 1);
            string bString = cString.Substring(2, 1);
            r = Convert.ToInt64(rString, 16);
            g = Convert.ToInt64(gString, 16);
            b = Convert.ToInt64(bString, 16);
            divisor = 15.0;
        }

        return new UIColor(red: (nfloat)(r / divisor), green: (nfloat)(g / divisor), blue: (nfloat)(b / divisor), alpha: 1);
    }

    private UIFont FontForCssStyle(string? fontStyle)
    {
        switch (fontStyle)
        {
            case "bold":
            case "bolder":
            case "600":
            case "700":
            case "800":
            case "900":
                return BoldCodeFont;
            case "italic":
            case "oblique":
                return ItalicCodeFont;
            default:
                return CodeFont;
        }
    }
}

#endif
