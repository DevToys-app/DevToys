using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.EncodersDecoders.Jwt;

[Export(typeof(IGuiTool))]
[Name("JwtEncoderDecoder")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\ue78d',
    GroupName = PredefinedCommonToolGroupNames.EncodersDecoders,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.Jwt.JwtEncoderDecoder",
    ShortDisplayTitleResourceName = nameof(JwtEncoderDecoder.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(JwtEncoderDecoder.LongDisplayTitle),
    DescriptionResourceName = nameof(JwtEncoderDecoder.Description),
    AccessibleNameResourceName = nameof(JwtEncoderDecoder.AccessibleName))]
internal sealed partial class JwtEncoderDecoderGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Whether the tool should encode or decode JWT Token.
    /// </summary>
    private static readonly SettingDefinition<JwtMode> toolMode
        = new(
            name: $"{nameof(JwtEncoderDecoderGuiTool)}.{nameof(toolMode)}",
            defaultValue: JwtMode.Decode);

    internal enum JwtGridRows
    {
        Settings,
        SubContainer
    }

    private enum GridColumns
    {
        Stretch
    }

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;

    private readonly IUISwitch _conversionModeSwitch = Switch("jwt-token-conversion-mode-switch");

    private CancellationTokenSource? _cancellationTokenSource;

    private readonly JwtDecoderGuiTool _decoderGuiTool = new();

    private readonly JwtEncoderGuiTool _encoderGuiTool = new();

    private readonly IUIGrid _jwtEncoderDecoderGuiGrid = Grid("jwt-encoder-decoder-grid");

    private IUIGridCell _subToolView;

    [ImportingConstructor]
    public JwtEncoderDecoderGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        switch (_settingsProvider.GetSetting(JwtEncoderDecoderGuiTool.toolMode))
        {
            case JwtMode.Encode:
                _subToolView = _encoderGuiTool.GridCell;
                break;

            case JwtMode.Decode:
                _subToolView = _decoderGuiTool.GridCell;
                break;

            default:
                throw new NotSupportedException();
        }
    }

    // For unit tests.
    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: true,
            _jwtEncoderDecoderGuiGrid
                .RowLargeSpacing()
                .Rows(
                    (JwtGridRows.Settings, Auto),
                    (JwtGridRows.SubContainer, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Columns(
                    (GridColumns.Stretch, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Cells(
                    Cell(
                        JwtGridRows.Settings,
                        GridColumns.Stretch,
                        Stack()
                            .Vertical()
                            .SmallSpacing()
                            .WithChildren(
                                Label()
                                    .Text(JwtEncoderDecoder.ConfigurationTitle),
                                Setting("jwt-token-conversion-mode-setting")
                                    .Icon("FluentSystemIcons", '\uF18D')
                                    .Title(JwtEncoderDecoder.ToolModeTitle)
                                    .Description(JwtEncoderDecoder.ToolModeDescription)
                                    .InteractiveElement(
                                        _conversionModeSwitch
                                        .OnText(JwtEncoderDecoder.EncodeMode)
                                        .OffText(JwtEncoderDecoder.DecodeMode)
                                        .OnToggle(OnConversionModeChanged)
                                    )
                            )
                    ),
                    _subToolView
                )
        );

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnConversionModeChanged(bool toolMode)
    {
        _settingsProvider.SetSetting(JwtEncoderDecoderGuiTool.toolMode, toolMode ? JwtMode.Encode : JwtMode.Decode);
        switch (_settingsProvider.GetSetting(JwtEncoderDecoderGuiTool.toolMode))
        {
            case JwtMode.Encode:
                _subToolView = _encoderGuiTool.GridCell;
                break;

            case JwtMode.Decode:
                _subToolView = _decoderGuiTool.GridCell;
                break;

            default:
                throw new NotSupportedException();
        }
    }

}
