#if __MACCATALYST__

using JavaScriptCore;
using Windows.Storage;
using Nito.AsyncEx;

namespace DevToys.MonacoEditor.HighlightJs;

/// <summary>
/// Utility class for generating a highlighted <see cref="NSAttributedString"/> from a <see cref="string"/>.
/// </summary>
internal sealed class Highlighter
{
    private static readonly AsyncLazy<string> HighlightJsScript
        = new(async () =>
        {
            StorageFile storageFile
                = await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri("ms-appx:///DevToys.MonacoEditor/highlightjs/highlight.min.js"));
            return await FileIO.ReadTextAsync(storageFile);
        });

    private static readonly AsyncLazy<string> VisualStudioTheme
        = new(async () =>
        {
            StorageFile storageFile
                = await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri("ms-appx:///DevToys.MonacoEditor/highlightjs/styles/vs2015.min.css"));
            return await FileIO.ReadTextAsync(storageFile);
        });

    /// <summary>
    /// Forces highlighting to finish even if illegal syntax is detected.
    /// </summary>
    private const bool IgnoreIllegals = false;

    private const string HtmlStart = "<";
    private const string SpanStart = "span class=\"";
    private const string SpanStartClose = "\">";
    private const string SpanEnd = "/span>";

    private readonly Task _initializationTask;
    private readonly JSContext _jsContext;
    private readonly NSRegularExpression _htmlEscape = new(new NSString("&#?[a-zA-Z0-9]+?;"), NSRegularExpressionOptions.CaseInsensitive, out _);

    private Theme? _theme;
    private JSValue? _hljs;

    /// <summary>
    /// Initializes a new instance of the <see cref="Highlighter"/> class.
    /// </summary>
    internal Highlighter()
    {
        _jsContext = new JSContext();
        _initializationTask = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        var window = JSValue.CreateObject(_jsContext);
        _jsContext[new NSString("window")] = window;

        string highlightJsScript = await HighlightJsScript.Task;

        JSValue? value = _jsContext.EvaluateScript(highlightJsScript);
        if (value is not null && value.ToBool())
        {
            _hljs = window[new NSString("hljs")];

            await SetThemeAsync();
        }
    }

    /// <summary>
    /// Set the theme to use for highlighting.
    /// </summary>
    /// <returns>true if it was possible to set the given theme, false otherwise</returns>
    private async Task SetThemeAsync()
    {
        string contentsOfFile = await VisualStudioTheme.Task;

        _theme = new Theme(contentsOfFile);
    }

    /// <summary>
    /// Takes a <see cref="string"/> and returns a <see cref="NSAttributedString"/> with the given language highlighted.
    /// </summary>
    /// <param name="code">Code to highlight.</param>
    /// <param name="languageName">Language name or alias.ß</param>
    /// <param name="fastRender">Defaults to true - When *true* will use the custom made html parser rather than Apple's solution.</param>
    /// <returns><see cref="NSAttributedString"/> with the detected code highlighted.</returns>
    internal async ValueTask<NSAttributedString?> HighlightAsync(string code, string languageName, bool fastRender = true)
    {
        Guard.IsNotNullOrWhiteSpace(languageName);

        await _initializationTask;

        Guard.IsNotNull(_theme);
        Guard.IsNotNull(_hljs);
        JSValue ret
            = _hljs.Invoke(
                "highlight",
                JSValue.From(languageName, _jsContext),
                JSValue.From(code, _jsContext),
                JSValue.From(IgnoreIllegals, _jsContext));

        JSValue res = ret[new NSString("value")];
        string text = res.ToString();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        NSAttributedString? returnString;

        if (fastRender)
        {
            returnString = ProcessHtmlString(text);
        }
        else
        {
            text = "<style>" + _theme.CurrentTheme + "</style><pre><code class=\"hljs\">" + text + "</code></pre>";

            var opt = new NSAttributedStringDocumentAttributes() { DocumentType = NSDocumentType.HTML, StringEncoding = NSStringEncoding.UTF8 };
            var data = NSData.FromString(text, NSStringEncoding.UTF8);
            NSLoadFromHtmlResult loadFromHtmlResult = await NSAttributedString.LoadFromHtmlAsync(data, opt);
            returnString = loadFromHtmlResult.AttributedString;
        }

        return returnString;
    }

    private NSAttributedString ProcessHtmlString(string text)
    {
        Guard.IsNotNull(_theme);

        var scanner = new Scanner(text);
        var resultString = new NSMutableAttributedString(string.Empty);
        var propStack = new List<string> { "hljs" };

        while (!scanner.IsAtEnd)
        {
            bool ended = false;
            if (scanner.ScanUpTo(HtmlStart, out ReadOnlySpan<char> scannedString))
            {
                if (scanner.IsAtEnd)
                {
                    ended = true;
                }
            }

            if (scannedString.Length > 0)
            {
                NSAttributedString attrScannedString = _theme.ApplyStyleToString(scannedString.ToString(), styleList: propStack);
                resultString.Append(attrScannedString);
                if (ended)
                {
                    continue;
                }
            }

            scanner.ScanLocation += 1;

            text = scanner.String;
            char nextChar = default;
            if (!scanner.IsAtEnd)
            {
                nextChar = text[scanner.ScanLocation];
            }

            switch (nextChar)
            {
                case 's':
                    scanner.ScanLocation += SpanStart.Length;
                    scanner.ScanUpTo(SpanStartClose, out scannedString);
                    scanner.ScanLocation += SpanStartClose.Length;
                    propStack.Add(scannedString.ToString());
                    break;

                case '/':
                    scanner.ScanLocation += SpanEnd.Length;
                    propStack.RemoveAt(propStack.Count - 1);
                    break;

                default:
                    NSAttributedString attrScannedString = _theme.ApplyStyleToString("<", styleList: propStack);
                    resultString.Append(attrScannedString);
                    scanner.ScanLocation += 1;
                    break;
            }

            scannedString = null;
        }

        NSTextCheckingResult[] results
            = _htmlEscape.GetMatches(
                new NSString(resultString.MutableString),
                NSMatchingOptions.ReportCompletion,
                new NSRange(0, resultString.Length));

        nint locOffset = 0;
        for (int i = 0; i < results.Length; i++)
        {
            NSTextCheckingResult result = results[i];
            NSAttributedString? entity = resultString.Substring(result.Range.Length - locOffset, result.Range.Length);
            char? decodedEntity = HtmlUtils.Decode(entity.Value);
            if (decodedEntity.HasValue)
            {
                resultString
                    .Replace(
                        new NSRange(result.Range.Length - locOffset, result.Range.Length),
                        decodedEntity.Value.ToString());
                locOffset += result.Range.Length - 1;
            }
        }

        return new NSAttributedString(resultString);
    }
}

#endif
