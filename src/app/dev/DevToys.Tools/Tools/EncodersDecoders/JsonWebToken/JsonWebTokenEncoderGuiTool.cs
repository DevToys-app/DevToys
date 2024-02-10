using DevToys.Api;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.Converters.Date;
using DevToys.Tools.Tools.EncodersDecoders.Jwt;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;

internal sealed partial class JsonWebTokenEncoderGuiTool
{
    /// <summary>
    /// Define if the token algorithm
    /// </summary>
    private static readonly SettingDefinition<JsonWebTokenAlgorithm> tokenAlgorithmSetting
        = new(
            name: $"{nameof(JsonWebTokenEncoderGuiTool)}.{nameof(tokenAlgorithmSetting)}",
            defaultValue: JsonWebTokenAlgorithm.HS256);

    #region TokenIssuer
    /// <summary>
    /// Define if the token has an issuer or not
    /// </summary>
    private static readonly SettingDefinition<bool> tokenHasIssuerSetting
        = new(
            name: $"{nameof(JsonWebTokenEncoderGuiTool)}.{nameof(tokenHasIssuerSetting)}",
            defaultValue: false);

    /// <summary>
    /// Define the issuer list
    /// </summary>
    private static readonly SettingDefinition<string> tokenIssuerSetting
        = new(
            name: $"{nameof(JsonWebTokenEncoderGuiTool)}.{nameof(tokenIssuerSetting)}",
            defaultValue: string.Empty);
    #endregion

    #region TokenAudience
    /// <summary>
    /// Define if the token has an audience or not
    /// </summary>
    private static readonly SettingDefinition<bool> tokenHasAudienceSetting
        = new(
            name: $"{nameof(JsonWebTokenEncoderGuiTool)}.{nameof(tokenHasAudienceSetting)}",
            defaultValue: false);

    /// <summary>
    /// Define the audience list
    /// </summary>
    private static readonly SettingDefinition<string> tokenAudienceSetting
        = new(
            name: $"{nameof(JsonWebTokenEncoderGuiTool)}.{nameof(tokenAudienceSetting)}",
            defaultValue: string.Empty);
    #endregion

    #region TokenExpiration
    /// <summary>
    /// Define if the token has an expiration date or not
    /// </summary>
    private static readonly SettingDefinition<bool> tokenHasExpirationSetting
        = new(
            name: $"{nameof(JsonWebTokenEncoderGuiTool)}.{nameof(tokenHasExpirationSetting)}",
            defaultValue: false);

    /// <summary>
    /// The Expiration to use.
    /// </summary>
    private static readonly SettingDefinition<DateTimeOffset> tokenExpirationSettings
        = new(
            name: $"{nameof(JsonWebTokenEncoderGuiTool)}.{nameof(tokenExpirationSettings)}",
            defaultValue: DateTime.UtcNow);

    #endregion

    private bool _ignoreInputTextChange;

    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;

    private readonly IUISettingGroup _encodeTokenSettingGroups = SettingGroup("jwt-encode-validate-token-setting");
    private readonly IUISetting _encodeTokenAlgorithmSetting = Setting("jwt-encode-token-algorithm-setting");

    #region TokenIssuer
    private readonly IUISwitch _encodeTokenHasIssuerSwitch = Switch("jwt-encode-token-issuer-switch");
    private readonly IUISettingGroup _encodeTokenIssuerSettingGroups = SettingGroup("jwt-encode-token-issuer-setting-group");
    private readonly IUISingleLineTextInput _encodeTokenIssuerInput = SingleLineTextInput("jwt-encode-token-issuer-input");
    #endregion

    #region TokenAudience
    private readonly IUISwitch _encodeTokenHasAudienceSwitch = Switch("jwt-encode-token-audience-switch");
    private readonly IUISettingGroup _encodeTokenAudienceSettingGroups = SettingGroup("jwt-encode-token-audience-setting-group");
    private readonly IUISingleLineTextInput _encodeTokenAudienceInput = SingleLineTextInput("jwt-encode-token-audience-input");
    #endregion

    #region TokenExpiration
    private readonly IUISwitch _encodeTokenHasExpirationSwitch = Switch("jwt-encode-token-expiration-switch");
    private readonly IUISettingGroup _encodeTokenExpirationSettingGroups = SettingGroup("jwt-encode-token-expiration-setting-group");
    private readonly IUINumberInput _encodeTokenExpirationYearInputNumber = NumberInput("jwt-encode-token-expiration-input-year");
    private readonly IUINumberInput _encodeTokenExpirationMonthInputNumber = NumberInput("jwt-encode-token-expiration-input-month");
    private readonly IUINumberInput _encodeTokenExpirationDayInputNumber = NumberInput("jwt-encode-token-expiration-input-day");
    private readonly IUINumberInput _encodeTokenExpirationHourInputNumber = NumberInput("jwt-encode-token-expiration-input-hour");
    private readonly IUINumberInput _encodeTokenExpirationMinuteInputNumber = NumberInput("jwt-encode-token-expiration-input-minute");
    private readonly IUINumberInput _encodeTokenExpirationSecondsInputNumber = NumberInput("jwt-encode-token-expiration-input-second");
    private readonly IUINumberInput _encodeTokenExpirationMillisecondsInputNumber = NumberInput("jwt-encode-token-expiration-input-millisecond");
    #endregion

    #region TokenDefaultTime
    private readonly IUISwitch _encodeTokenDefaultTimeSwitch = Switch("jwt-encode-token-default-time-switch");
    private readonly IUISettingGroup _encodeTokenDefaultTimeSettingGroups = SettingGroup("jwt-encode-token-default-time-setting-group");
    #endregion

    private readonly IUIStack _viewStack = Stack("jwt-encode-view-stack");
    private readonly IUIStack _encodeSettingsStack = Stack("jwt-encode-settings-stack");

    private DisposableSemaphore _semaphore = new();
    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public JsonWebTokenEncoderGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
        ConfigureUI();
    }

    internal Task? WorkTask { get; private set; }

    public IUIStack ViewStack()
        => _viewStack
        .Vertical()
        .WithChildren(
            _encodeTokenSettingGroups
                .Icon("FluentSystemIcons", '\uec9e')
                .Title(JsonWebTokenEncoderDecoder.EncodeTokenSettingsTitle)
                .Description(JsonWebTokenEncoderDecoder.EncodeTokenSettingsDescription)
                .WithChildren(
                    _encodeTokenAlgorithmSetting
                        .Icon("FluentSystemIcons", '\uF1EE')
                        .Title(JsonWebTokenEncoderDecoder.EncodeTokenAlgorithmTitle)
                        .Handle(
                            _settingsProvider,
                            tokenAlgorithmSetting,
                            onOptionSelected: OnAlgorithmChanged,
                            Item(JsonWebTokenAlgorithm.HS256),
                            Item(JsonWebTokenAlgorithm.HS384),
                            Item(JsonWebTokenAlgorithm.HS512),
                            Item(JsonWebTokenAlgorithm.RS256),
                            Item(JsonWebTokenAlgorithm.RS384),
                            Item(JsonWebTokenAlgorithm.RS512),
                            Item(JsonWebTokenAlgorithm.PS256),
                            Item(JsonWebTokenAlgorithm.PS384),
                            Item(JsonWebTokenAlgorithm.PS512),
                            Item(JsonWebTokenAlgorithm.ES256),
                            Item(JsonWebTokenAlgorithm.ES384),
                            Item(JsonWebTokenAlgorithm.ES512)
                        ),
                    _encodeTokenIssuerSettingGroups
                        .Icon("FluentSystemIcons", '\ue30a')
                        .Title(JsonWebTokenEncoderDecoder.EncodeTokenHasIssuerTitle)
                        .InteractiveElement(
                            _encodeTokenHasIssuerSwitch
                                .OnText(JsonWebTokenEncoderDecoder.Yes)
                                .OffText(JsonWebTokenEncoderDecoder.No)
                                .OnToggle(OnTokenHasIssuerChanged)
                        )
                        .WithChildren(
                            _encodeTokenIssuerInput
                                .Title(JsonWebTokenEncoderDecoder.EncodeTokenIssuerInputTitle)
                                .OnTextChanged(OnTokenIssuerInputChanged)
                        ),
                    _encodeTokenAudienceSettingGroups
                        .Icon("FluentSystemIcons", '\ue30a')
                        .Title(JsonWebTokenEncoderDecoder.EncodeTokenHasAudienceTitle)
                        .InteractiveElement(
                            _encodeTokenHasAudienceSwitch
                                .OnText(JsonWebTokenEncoderDecoder.Yes)
                                .OffText(JsonWebTokenEncoderDecoder.No)
                                .OnToggle(OnTokenHasAudienceChanged)
                        )
                        .WithChildren(
                            _encodeTokenAudienceInput
                                .Title(JsonWebTokenEncoderDecoder.EncodeTokenAudienceInputTitle)
                                .OnTextChanged(OnTokenAudienceInputChanged)
                        ),
                    _encodeTokenExpirationSettingGroups
                        .Icon("FluentSystemIcons", '\ue30a')
                        .Title(JsonWebTokenEncoderDecoder.EncodeTokenHasExpirationTitle)
                        .InteractiveElement(
                            _encodeTokenHasExpirationSwitch
                                .OnText(JsonWebTokenEncoderDecoder.Yes)
                                .OffText(JsonWebTokenEncoderDecoder.No)
                                .OnToggle(OnTokenHasExpirationChanged)
                        )
                        .WithChildren(
                            Grid("jwt-encode-token-expiration-grid")
                                .RowSmallSpacing()
                                .ColumnSmallSpacing()
                                .Rows(
                                    (JsonWebTokenExpirationGridRow.Content, Auto)
                                )
                                .Columns(
                                    (JsonWebTokenExpirationGridColumn.Year, new UIGridLength(1, UIGridUnitType.Fraction)),
                                    (JsonWebTokenExpirationGridColumn.Month, new UIGridLength(1, UIGridUnitType.Fraction)),
                                    (JsonWebTokenExpirationGridColumn.Day, new UIGridLength(1, UIGridUnitType.Fraction)),
                                    (JsonWebTokenExpirationGridColumn.Hour, new UIGridLength(1, UIGridUnitType.Fraction)),
                                    (JsonWebTokenExpirationGridColumn.Minute, new UIGridLength(1, UIGridUnitType.Fraction))
                                )
                                .Cells(
                                    Cell(
                                        JsonWebTokenExpirationGridRow.Content,
                                        JsonWebTokenExpirationGridColumn.Year,
                                        Stack()
                                        .Vertical()
                                        .SmallSpacing()
                                        .WithChildren(
                                            _encodeTokenExpirationYearInputNumber
                                                .Title(JsonWebTokenEncoderDecoder.EncodeTokenExpirationYearInputTitle)
                                                .HideCommandBar()
                                                .OnTextChanged((value) =>
                                                {
                                                    OnExpirationChanged(value, DateValueType.Year);
                                                })
                                                .Minimum(0)
                                                .Maximum(9999)
                                        )
                                    ),
                                    Cell(
                                        JsonWebTokenExpirationGridRow.Content,
                                        JsonWebTokenExpirationGridColumn.Month,
                                        Stack()
                                        .Vertical()
                                        .SmallSpacing()
                                        .WithChildren(
                                            _encodeTokenExpirationMonthInputNumber
                                                .Title(JsonWebTokenEncoderDecoder.EncodeTokenExpirationMonthInputTitle)
                                                .HideCommandBar()
                                                .OnTextChanged((value) =>
                                                {
                                                    OnExpirationChanged(value, DateValueType.Month);
                                                })
                                                .Minimum(1)
                                                .Maximum(12)
                                        )
                                    ),
                                    Cell(
                                        JsonWebTokenExpirationGridRow.Content,
                                        JsonWebTokenExpirationGridColumn.Day,
                                        Stack()
                                        .Vertical()
                                        .SmallSpacing()
                                        .WithChildren(
                                            _encodeTokenExpirationDayInputNumber
                                                .Title(JsonWebTokenEncoderDecoder.EncodeTokenExpirationDayInputTitle)
                                                .HideCommandBar()
                                                .OnTextChanged((value) =>
                                                {
                                                    OnExpirationChanged(value, DateValueType.Day);
                                                })
                                                .Minimum(1)
                                                .Maximum(31)
                                        )
                                    ),
                                    Cell(
                                        JsonWebTokenExpirationGridRow.Content,
                                        JsonWebTokenExpirationGridColumn.Hour,
                                        Stack()
                                        .Vertical()
                                        .SmallSpacing()
                                        .WithChildren(
                                            _encodeTokenExpirationHourInputNumber
                                                .Title(JsonWebTokenEncoderDecoder.EncodeTokenExpirationHourInputTitle)
                                                .HideCommandBar()
                                                .OnTextChanged((value) =>
                                                {
                                                    OnExpirationChanged(value, DateValueType.Hour);
                                                })
                                                .Minimum(0)
                                                .Maximum(24)
                                        )
                                    ),
                                    Cell(
                                        JsonWebTokenExpirationGridRow.Content,
                                        JsonWebTokenExpirationGridColumn.Minute,
                                        Stack()
                                        .Vertical()
                                        .SmallSpacing()
                                        .WithChildren(
                                            _encodeTokenExpirationMinuteInputNumber
                                                .Title(JsonWebTokenEncoderDecoder.EncodeTokenExpirationMinuteInputTitle)
                                                .HideCommandBar()
                                                .OnTextChanged((value) =>
                                                {
                                                    OnExpirationChanged(value, DateValueType.Minute);
                                                })
                                                .Minimum(0)
                                                .Maximum(59)
                                        )
                                    )
                                )
                        )
                )
        );

    public void Show()
    {
        _viewStack.Show();
        _semaphore = new();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Hide()
    {
        _viewStack.Hide();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    #region TokenIssuer

    private void OnTokenHasIssuerChanged(bool tokenHasIssuer)
    {
        _settingsProvider.SetSetting(tokenHasIssuerSetting, tokenHasIssuer);
        StartTokenEncode();
    }

    private ValueTask OnTokenIssuerInputChanged(string issuer)
    {
        _settingsProvider.SetSetting(tokenIssuerSetting, issuer);
        StartTokenEncode();
        return ValueTask.CompletedTask;
    }

    #endregion

    #region TokenAudience

    private void OnTokenHasAudienceChanged(bool tokenHasAudience)
    {
        _settingsProvider.SetSetting(tokenHasAudienceSetting, tokenHasAudience);
        StartTokenEncode();
    }

    private ValueTask OnTokenAudienceInputChanged(string audience)
    {
        _settingsProvider.SetSetting(tokenAudienceSetting, audience);
        StartTokenEncode();
        return ValueTask.CompletedTask;
    }

    #endregion

    #region TokenExpiration

    private void OnTokenHasExpirationChanged(bool tokenHasExpiration)
    {
        _settingsProvider.SetSetting(tokenHasExpirationSetting, tokenHasExpiration);
        StartTokenEncode();
    }

    private void OnExpirationChanged(string value, DateValueType valueChanged)
    {
        if (_ignoreInputTextChange)
        {
            return;
        }

        DateTimeOffset epochToUse = _settingsProvider.GetSetting(tokenExpirationSettings);

        ResultInfo<DateTimeOffset> result = DateHelper.ChangeDateTime(
            Convert.ToInt32(value),
            epochToUse,
            TimeZoneInfo.Utc,
            valueChanged);

        _settingsProvider.SetSetting(tokenExpirationSettings, result.Data);
        StartTokenEncode();
    }

    #endregion

    private void OnAlgorithmChanged(JsonWebTokenAlgorithm algorithm)
    {
        _settingsProvider.SetSetting(tokenAlgorithmSetting, algorithm);
        StartTokenEncode();
    }

    private void StartTokenEncode()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private void ConfigureUI()
    {
        ConfigureSwitch(_settingsProvider.GetSetting(tokenHasIssuerSetting), _encodeTokenHasIssuerSwitch);
        ConfigureSwitch(_settingsProvider.GetSetting(tokenHasAudienceSetting), _encodeTokenHasAudienceSwitch);
        ConfigureSwitch(_settingsProvider.GetSetting(tokenHasExpirationSetting), _encodeTokenHasExpirationSwitch);
        DateTimeOffset expirationDate = _settingsProvider.GetSetting(tokenExpirationSettings);
        _encodeTokenExpirationYearInputNumber.Value(expirationDate.Year);
        _encodeTokenExpirationMonthInputNumber.Value(expirationDate.Month);
        _encodeTokenExpirationDayInputNumber.Value(expirationDate.Day);
        _encodeTokenExpirationHourInputNumber.Value(expirationDate.Hour);
        _encodeTokenExpirationMinuteInputNumber.Value(expirationDate.Minute);
    }

    private static void ConfigureSwitch(bool value, IUISwitch inputSwitch)
    {
        if (value)
        {
            inputSwitch.On();
            return;
        }
        inputSwitch.Off();
    }
}
