using DevToys.Tools.Models.NumberBase;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Converters.NumberBase;

[Export(typeof(IGuiTool))]
[Name("NumberBaseConverter")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0118',
    GroupName = PredefinedCommonToolGroupNames.Converters,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Converters.NumberBase.NumberBaseConverter",
    ShortDisplayTitleResourceName = nameof(NumberBaseConverter.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(NumberBaseConverter.LongDisplayTitle),
    DescriptionResourceName = nameof(NumberBaseConverter.Description),
    AccessibleNameResourceName = nameof(NumberBaseConverter.AccessibleName))]
[AcceptedDataTypeName("NumberBase")]
internal sealed class NumberBaseConverterGuiTool : IGuiTool
{
    /// <summary>
    /// Whether the value should be formatted or not.
    /// </summary>
    internal static readonly SettingDefinition<bool> formatted
        = new(
            name: $"{nameof(NumberBaseConverterGuiTool)}.{nameof(formatted)}",
            defaultValue: true);

    /// <summary>
    /// Whether the advanced mode should be used or not.
    /// </summary>
    private static readonly SettingDefinition<bool> advancedMode
        = new(
            name: $"{nameof(NumberBaseConverterGuiTool)}.{nameof(advancedMode)}",
            defaultValue: false);

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIStack _modeContainer = Stack();
    private readonly IUIInfoBar _infoBar = InfoBar();
    private readonly Lazy<INumberBaseConverterGuiToolMode> _basicMode;
    private readonly Lazy<INumberBaseConverterGuiToolMode> _advancedMode;

    private INumberBaseConverterGuiToolMode _activeMode = null!;

    [ImportingConstructor]
    public NumberBaseConverterGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        _basicMode = new(() => new NumberBaseConverterGuiToolBasicMode(_settingsProvider, OnError));
        _advancedMode = new(() => new NumberBaseConverterGuiToolAdvancedMode(_settingsProvider, OnError));

        ApplyMode();
    }

    public UIToolView View
        => new(
            isScrollable: true,
            Stack()
                .Vertical()
                .LargeSpacing()
                .WithChildren(

                    // Configuration
                    Stack()
                        .Vertical()
                        .WithChildren(

                            Label().Text(NumberBaseConverter.Configuration),
                            Setting()
                                .Icon("FluentSystemIcons", '\uF7B2')
                                .Title(NumberBaseConverter.Format)
                                .Handle(
                                    _settingsProvider,
                                    formatted,
                                    OnFormatOptionChanged),
                            Setting()
                                .Icon("FluentSystemIcons", '\uF1EE')
                                .Title(NumberBaseConverter.AdvancedMode)
                                .Handle(
                                    _settingsProvider,
                                    advancedMode,
                                    (_) => ApplyMode())),

                    _infoBar
                        .Close()
                        .NonClosable()
                        .Warning(),

                    // Content
                    _modeContainer
                        .Vertical()));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == "NumberBase")
        {
            _settingsProvider.SetSetting(advancedMode, false);
            ApplyMode();

            _activeMode.OnDataReceived(parsedData);
        }
    }

    private void ApplyMode()
    {
        if (_settingsProvider.GetSetting(advancedMode))
        {
            _activeMode = _advancedMode.Value;
        }
        else
        {
            _activeMode = _basicMode.Value;
        }

        _modeContainer.WithChildren(_activeMode.View);
    }

    private void OnFormatOptionChanged(bool _)
    {
        Guard.IsNotNull(_activeMode);
        _activeMode.OnInputChanged();
    }

    private void OnError(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            _infoBar.Close();
        }
        else
        {
            _infoBar.Title(message);
            _infoBar.Open();
        }
    }
}
