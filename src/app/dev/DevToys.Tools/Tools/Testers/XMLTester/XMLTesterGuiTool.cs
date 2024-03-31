using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Testers.XMLTester;

[Export(typeof(IGuiTool))]
[Name("XMLTester")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0116',
    GroupName = PredefinedCommonToolGroupNames.Testers,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Testers.XMLTester.XMLTester",
    ShortDisplayTitleResourceName = nameof(XMLTester.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(XMLTester.LongDisplayTitle),
    DescriptionResourceName = nameof(XMLTester.Description),
    AccessibleNameResourceName = nameof(XMLTester.AccessibleName),
    SearchKeywordsResourceName = nameof(XMLTester.SearchKeywords))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Xml)]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Xsd)]
internal sealed class XMLTesterGuiTool : IGuiTool, IDisposable
{
    private enum GridRows
    {
        TopStretch,
        BottomAuto,
    }

    private enum GridColumns
    {
        Stretch
    }

    private readonly ILogger _logger;
    private readonly DisposableSemaphore _semaphore = new();
    private readonly IUIMultiLineTextInput _xsdInputText = MultilineTextInput();
    private readonly IUIMultiLineTextInput _xmlInputText = MultilineTextInput();
    private readonly IUIInfoBar _infoBar = InfoBar();

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    internal XMLTesterGuiTool()
    {
        _logger = this.Log();
    }

    // For unit tests.
    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: false,
            Grid()
                .ColumnMediumSpacing()

                .Rows(
                    (GridRows.TopStretch, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (GridRows.BottomAuto, Auto))

                .Columns(
                    (GridColumns.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))

                .Cells(
                    Cell(
                        GridRows.TopStretch,
                        GridColumns.Stretch,

                        SplitGrid()
                            .Vertical()
                            .WithLeftPaneChild(
                                _xsdInputText
                                    .Title(XMLTester.XsdScheme)
                                    .Language("xml")
                                    .CanCopyWhenEditable()
                                    .Extendable()
                                    .OnTextChanged(OnInputChanged))

                            .WithRightPaneChild(
                                _xmlInputText
                                    .Title(XMLTester.XmlData)
                                    .Language("xml")
                                    .CanCopyWhenEditable()
                                    .Extendable()
                                    .OnTextChanged(OnInputChanged))),

                    Cell(
                        GridRows.BottomAuto,
                        GridColumns.Stretch,

                        _infoBar
                            .NonClosable()
                            .Open())));

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Xml && parsedData is string xml)
        {
            _xmlInputText.Text(xml);
        }
        else if (dataTypeName == PredefinedCommonDataTypeNames.Xsd && parsedData is string xsd)
        {
            _xsdInputText.Text(xsd);
        }
    }

    private void OnInputChanged(string _)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = ValidateAsync(_xsdInputText.Text, _xmlInputText.Text, _cancellationTokenSource.Token);
    }

    private async Task ValidateAsync(string xsd, string xml, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(xsd) || string.IsNullOrWhiteSpace(xml))
        {
            _infoBar
                .Description(XMLTester.ValidationImpossibleMsg)
                .Informational();
            return;
        }

        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        ResultInfo<string> result = XsdHelper.ValidateXmlAgainstXsd(xsd, xml, _logger, cancellationToken);

        switch (result.Severity)
        {
            case ResultInfoSeverity.Success:
                _infoBar
                    .Description(XMLTester.XmlValidMessage)
                    .Success();
                break;

            case ResultInfoSeverity.Warning:
                _infoBar
                    .Description(result.Data)
                    .Warning();
                break;

            case ResultInfoSeverity.Error:
                string errorDescription;
                if (string.IsNullOrWhiteSpace(result.Data))
                {
                    errorDescription = XMLTester.XmlInvalidMessage;
                }
                else
                {
                    errorDescription = result.Data;
                }

                _infoBar
                    .Description(errorDescription)
                    .Error();
                break;

            default:
                throw new NotImplementedException();
        }
    }
}
