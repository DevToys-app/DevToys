﻿using DevToys.Api;
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
    private static readonly SettingDefinition<JwtMode> toolModeSetting
        = new(
            name: $"{nameof(JwtEncoderDecoderGuiTool)}.{nameof(toolModeSetting)}",
            defaultValue: JwtMode.Decode);

    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly JwtDecoderGuiTool _decoderGuiTool;
    private readonly JwtEncoderGuiTool _encoderGuiTool;

    private readonly IUISwitch _conversionModeSwitch = Switch("jwt-token-conversion-mode-switch");

    private readonly IUIGrid _jwtEncoderDecoderGuiGrid = Grid("jwt-encoder-decoder-grid");

    [ImportingConstructor]
    public JwtEncoderDecoderGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
        _decoderGuiTool = new(_settingsProvider);
        _encoderGuiTool = new();
        LoadChildView();
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
                Cell(
                    JwtGridRows.SubContainer,
                    GridColumns.Stretch,
                    Stack()
                    .Vertical()
                    .WithChildren(
                        _decoderGuiTool.ViewStack(),
                        _encoderGuiTool.ViewStack()
                    )
                )
            //_decoderGuiTool.GridCell,
            //_encoderGuiTool.GridCell
            )
        );

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // Todo need to dispose child view too?
    }

    private void OnConversionModeChanged(bool toolMode)
    {
        _settingsProvider.SetSetting(JwtEncoderDecoderGuiTool.toolModeSetting, toolMode ? JwtMode.Encode : JwtMode.Decode);
        LoadChildView();
    }

    private IUIGridCell EncodeDecodeSettings()
        => Cell(
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
        );

    private void LoadChildView()
    {
        switch (_settingsProvider.GetSetting(JwtEncoderDecoderGuiTool.toolModeSetting))
        {
            case JwtMode.Encode:
                _decoderGuiTool.Hide();
                _encoderGuiTool.Show();
                break;

            case JwtMode.Decode:
                _encoderGuiTool.Hide();
                _decoderGuiTool.Show();
                break;

            default:
                throw new NotSupportedException();
        }
    }
}
