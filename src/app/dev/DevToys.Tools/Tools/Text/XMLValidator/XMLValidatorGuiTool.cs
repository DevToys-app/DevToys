using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Text.XMLValidator;

[Export(typeof(IGuiTool))]
[Name("XMLValidator")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0116',
    GroupName = PredefinedCommonToolGroupNames.Text,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Text.XMLValidator.XMLValidator",
    ShortDisplayTitleResourceName = nameof(XMLValidator.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(XMLValidator.LongDisplayTitle),
    DescriptionResourceName = nameof(XMLValidator.Description),
    AccessibleNameResourceName = nameof(XMLValidator.AccessibleName),
    SearchKeywordsResourceName = nameof(XMLValidator.SearchKeywords))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Xml)]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Xsd)]
internal sealed class XMLValidatorGuiTool : IGuiTool, IDisposable
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
    internal XMLValidatorGuiTool()
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
                                    .Title(XMLValidator.XsdScheme)
                                    .Language("xml")
                                    .CanCopyWhenEditable()
                                    .Extendable()
                                    .OnTextChanged(OnInputChanged))

                            .WithRightPaneChild(
                                _xmlInputText
                                    .Title(XMLValidator.XmlData)
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
                .Description(XMLValidator.ValidationImpossibleMsg)
                .Informational();
            return;
        }

        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        ResultInfo<string, XmlValidatorResultSeverity> result = XsdHelper.ValidateXmlAgainstXsd(xsd, xml, _logger, cancellationToken);

        switch (result.Severity)
        {
            case XmlValidatorResultSeverity.Success:
                _infoBar
                    .Description(XMLValidator.XmlValidMessage)
                    .Success();
                break;

            case XmlValidatorResultSeverity.Warning:
                _infoBar
                    .Description(result.Data)
                    .Warning();
                break;

            case XmlValidatorResultSeverity.Error:
                string errorDescription;
                if (string.IsNullOrWhiteSpace(result.Data))
                {
                    errorDescription = XMLValidator.XmlInvalidMessage;
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
