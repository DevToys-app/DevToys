using System.Reflection;
using ColorCode.Styling;
using DevToys.Tools.Models;
using Markdig;
using Markdown.ColorCode;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Text.MarkdownPreview;

[Export(typeof(IGuiTool))]
[Name("MarkdownPreview")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0112',
    GroupName = PredefinedCommonToolGroupNames.Text,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Text.MarkdownPreview.MarkdownPreview",
    ShortDisplayTitleResourceName = nameof(MarkdownPreview.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(MarkdownPreview.LongDisplayTitle),
    DescriptionResourceName = nameof(MarkdownPreview.Description),
    AccessibleNameResourceName = nameof(MarkdownPreview.AccessibleName),
    SearchKeywordsResourceName = nameof(MarkdownPreview.SearchKeywords))]
[AcceptedDataTypeName("Markdown")]
internal sealed class MarkdownPreviewGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Whether the markdown should be previewed in dark or light theme.
    /// </summary>
    private static readonly SettingDefinition<MarkdownPreviewTheme> theme
        = new(
            name: $"{nameof(MarkdownPreviewGuiTool)}.{nameof(theme)}",
            defaultValue: MarkdownPreviewTheme.Light);

    private enum GridRows
    {
        TopAuto,
        Stretch,
    }

    private enum GridColumns
    {
        Stretch
    }

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIWebView _webView = WebView();
    private readonly IUIMultiLineTextInput _inputText = MultilineTextInput();
    private readonly Lazy<string> _githubMarkdownIndexPage = new(() => ReadEmbeddedResource("index.html"));

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public MarkdownPreviewGuiTool(ISettingsProvider settingsProvider, IThemeListener themeListener)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        // Override the theme with the one defined in the app settings.
        _settingsProvider.SetSetting(
            theme,
            themeListener.ActualAppTheme == ApplicationTheme.Dark
                ? MarkdownPreviewTheme.Dark
                : MarkdownPreviewTheme.Light);
    }

    // For unit tests.
    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: false,
            Grid()
                .ColumnMediumSpacing()

                .Rows(
                    (GridRows.TopAuto, Auto),
                    (GridRows.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))

                .Columns(
                    (GridColumns.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))

                .Cells(
                    Cell(
                        GridRows.TopAuto,
                        GridColumns.Stretch,

                        Stack()
                            .Vertical()

                            .WithChildren(
                                Label().Text(MarkdownPreview.Configuration),

                                Setting()
                                    .Icon("FluentSystemIcons", '\uF591')
                                    .Title(MarkdownPreview.ThemeTitle)
                                    .Description(MarkdownPreview.ThemeDescription)
                                    .Handle(
                                        _settingsProvider,
                                        theme,
                                        OnThemeChanged,
                                        Item(MarkdownPreview.DarkTheme, MarkdownPreviewTheme.Dark),
                                        Item(MarkdownPreview.LightTheme, MarkdownPreviewTheme.Light)))),

                    Cell(
                        GridRows.Stretch,
                        GridColumns.Stretch,

                        SplitGrid()
                            .Vertical()
                            .LeftPaneLength(new UIGridLength(1, UIGridUnitType.Fraction))
                            .RightPaneLength(new UIGridLength(1, UIGridUnitType.Fraction))

                            .WithLeftPaneChild(
                                _inputText
                                    .Title(MarkdownPreview.Input)
                                    .Language("markdown")
                                    .CanCopyWhenEditable()
                                    .OnTextChanged(OnInputTextChanged))

                            .WithRightPaneChild(
                                _webView
                                    .Title(MarkdownPreview.Output)))));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == "Markdown" && parsedData is string markdown)
        {
            _inputText.Text(markdown); // This will trigger an update of the preview.
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnThemeChanged(MarkdownPreviewTheme _)
    {
        StartPreview();
    }

    private void OnInputTextChanged(string _)
    {
        StartPreview();
    }

    private void StartPreview()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = PreviewAsync(_inputText.Text, _settingsProvider.GetSetting(theme), _cancellationTokenSource.Token);
    }

    private async Task PreviewAsync(string markdown, MarkdownPreviewTheme theme, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            StyleDictionary codeStyleDictionary = theme == MarkdownPreviewTheme.Dark
                ? StyleDictionary.DefaultDark
                : StyleDictionary.DefaultLight;

            MarkdownPipeline pipeline
                = new MarkdownPipelineBuilder()
                    .UseEmojiAndSmiley()
                    .UseSmartyPants()
                    .UseAdvancedExtensions()
                    .UseColorCode(styleDictionary: codeStyleDictionary)
                    .EnableTrackTrivia()
                    .UseYamlFrontMatter()
                    .Build();

            string? htmlBody = Markdig.Markdown.ToHtml(markdown, pipeline);

            string htmlDocument
                = _githubMarkdownIndexPage.Value
                .Replace("{{renderTheme}}", theme.ToString().ToLower())
                .Replace("{{htmlBody}}", htmlBody);

            _webView.RenderHTML(htmlDocument);
        }
    }

    private static string ReadEmbeddedResource(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = $"DevToys.Tools.Assets.GitHubMarkdown.{name}";

        using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
