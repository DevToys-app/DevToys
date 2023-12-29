using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Formatters.Xml;

[Export(typeof(IGuiTool))]
[Name("XmlFormatter")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0122',
    GroupName = PredefinedCommonToolGroupNames.Formatters,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Formatters.Xml.XmlFormatter",
    ShortDisplayTitleResourceName = nameof(XmlFormatter.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(XmlFormatter.LongDisplayTitle),
    DescriptionResourceName = nameof(XmlFormatter.Description),
    AccessibleNameResourceName = nameof(XmlFormatter.AccessibleName))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Xml)]
internal sealed partial class XmlFormatterGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Which indentation the tool need to use.
    /// </summary>
    private static readonly SettingDefinition<Indentation> indentationMode
        = new(name: $"{nameof(XmlFormatterGuiTool)}.{nameof(indentationMode)}", defaultValue: Indentation.TwoSpaces);

    /// <summary>
    /// Whether XML attributes are put on a new line
    /// </summary>
    private static readonly SettingDefinition<bool> newLineOnAttributes
        = new(name: $"{nameof(XmlFormatterGuiTool)}.{nameof(newLineOnAttributes)}", defaultValue: false);

    private enum GridColumn
    {
        Content
    }

    private enum GridRow
    {
        Header,
        Content,
        Footer
    }

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIMultiLineTextInput _inputTextArea = MultilineTextInput("xml-input-text-area");
    private readonly IUIMultiLineTextInput _outputTextArea = MultilineTextInput("xml-output-text-area");

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public XmlFormatterGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
    }

    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: true,
            Grid()
                .ColumnLargeSpacing()
                .RowLargeSpacing()
                .Rows(
                    (GridRow.Header, Auto),
                    (GridRow.Content, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Columns(
                    (GridColumn.Content, new UIGridLength(1, UIGridUnitType.Fraction))
                )
            .Cells(
                Cell(
                    GridRow.Header,
                    GridColumn.Content,
                    Stack().Vertical().WithChildren(
                        Label().Text(XmlFormatter.Configuration),
                        Setting("xml-text-indentation-setting")
                        .Icon("FluentSystemIcons", '\uF6F8')
                        .Title(XmlFormatter.Indentation)
                        .Handle(
                            _settingsProvider,
                            indentationMode,
                            OnIndentationModelChanged,
                            Item(XmlFormatter.TwoSpaces, Indentation.TwoSpaces),
                            Item(XmlFormatter.FourSpaces, Indentation.FourSpaces),
                            Item(XmlFormatter.OneTab, Indentation.OneTab),
                            Item(XmlFormatter.Minified, Indentation.Minified)
                        ),
                        Setting("xml-text-newLineOnAttributes-setting")
                        .Icon("FluentSystemIcons", '\uf7ed')
                        .Title(XmlFormatter.NewLineOnAttributes)
                        .Description(XmlFormatter.NewLineOnAttributesDescription)
                        .Handle(
                            _settingsProvider,
                            newLineOnAttributes,
                            OnSettingChanged
                        )
                    )
                ),
                Cell(
                    GridRow.Content,
                    GridColumn.Content,
                    SplitGrid()
                        .Vertical()
                        .WithLeftPaneChild(
                            _inputTextArea
                                .Language("xml")
                                .Title(XmlFormatter.Input)
                                .OnTextChanged(OnInputTextChanged))
                        .WithRightPaneChild(
                            _outputTextArea
                                .Language("xml")
                                .Title(XmlFormatter.Output)
                                .ReadOnly()
                                .Extendable())
                )
            )
        );

    // Smart detection handler.
    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Xml &&
            parsedData is string xmlStrongTypedParsedData)
        {
            _inputTextArea.Text(xmlStrongTypedParsedData);
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnSettingChanged(bool value)
    {
        StartFormat(_inputTextArea.Text);
    }

    private void OnIndentationModelChanged(Indentation indentationMode)
    {
        StartFormat(_inputTextArea.Text);
    }

    private void OnInputTextChanged(string text)
    {
        StartFormat(text);
    }

    private void StartFormat(string text)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = FormatAsync(text, _settingsProvider.GetSetting(indentationMode), _settingsProvider.GetSetting(newLineOnAttributes), _cancellationTokenSource.Token);
    }

    private async Task FormatAsync(string input, Indentation indentationSetting, bool newLineOnAttributeSetting, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            ResultInfo<string> formatResult = XmlHelper.Format(
                input,
                indentationSetting,
                newLineOnAttributeSetting,
                _logger);

            _outputTextArea.Text(formatResult.Data);
        }
    }
}
