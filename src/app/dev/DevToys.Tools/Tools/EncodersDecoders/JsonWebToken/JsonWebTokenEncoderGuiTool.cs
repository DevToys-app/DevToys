using DevToys.Tools.Helpers;
using DevToys.Tools.Helpers.JsonWebToken;
using DevToys.Tools.Models;
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

    /// <summary>
    /// Define if the token has an expiration date or not
    /// </summary>
    private static readonly SettingDefinition<bool> tokenHasDefaultTimeSetting
        = new(
            name: $"{nameof(JsonWebTokenEncoderGuiTool)}.{nameof(tokenHasDefaultTimeSetting)}",
            defaultValue: false);

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

    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;

    private readonly IUIInfoBar _infoBar = InfoBar("jwt-encode-info-bar");
    private readonly IUISwitch _encodeTokenHasIssuerSwitch = Switch("jwt-encode-token-issuer-switch");
    private readonly IUISwitch _encodeTokenHasAudienceSwitch = Switch("jwt-encode-token-audience-switch");
    private readonly IUISingleLineTextInput _encodeTokenIssuerInput = SingleLineTextInput("jwt-encode-token-issuer-input");
    private readonly IUISingleLineTextInput _encodeTokenAudienceInput = SingleLineTextInput("jwt-encode-token-audience-input");

    private readonly IUISwitch _encodeTokenHasExpirationSwitch = Switch("jwt-encode-token-expiration-switch");
    private readonly IUIGrid _encodeTokenExpirationGrid = Grid("jwt-encode-token-expiration-grid");
    private readonly IUINumberInput _encodeTokenExpirationYearInputNumber = NumberInput("jwt-encode-token-expiration-input-year");
    private readonly IUINumberInput _encodeTokenExpirationMonthInputNumber = NumberInput("jwt-encode-token-expiration-input-month");
    private readonly IUINumberInput _encodeTokenExpirationDayInputNumber = NumberInput("jwt-encode-token-expiration-input-day");
    private readonly IUINumberInput _encodeTokenExpirationHourInputNumber = NumberInput("jwt-encode-token-expiration-input-hour");
    private readonly IUINumberInput _encodeTokenExpirationMinuteInputNumber = NumberInput("jwt-encode-token-expiration-input-minute");
    private readonly IUINumberInput _encodeTokenExpirationSecondsInputNumber = NumberInput("jwt-encode-token-expiration-input-second");
    private readonly IUINumberInput _encodeTokenExpirationMillisecondsInputNumber = NumberInput("jwt-encode-token-expiration-input-millisecond");

    private readonly IUISwitch _encodeTokenDefaultTimeSwitch = Switch("jwt-encode-token-default-time-switch");

    private readonly IUIStack _viewStack = Stack("jwt-encode-view-stack");
    private readonly IUIStack _encodeSettingsStack = Stack("jwt-encode-settings-stack");

    private readonly IUIMultiLineTextInput _tokenInput = MultilineTextInput("jwt-encode-token-input");
    private readonly IUIMultiLineTextInput _signatureInput = MultilineTextInput("jwt-encode-signature-input");
    private readonly IUIMultiLineTextInput _privateKeyInput = MultilineTextInput("jwt-encode-private-key-input");
    private readonly IUIMultiLineTextInput _headerInput = MultilineTextInput("jwt-encode-header-input", "json");
    private readonly IUIMultiLineTextInput _payloadInput = MultilineTextInput("jwt-encode-payload-input", "json");

    private DisposableSemaphore _semaphore = new();
    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public JsonWebTokenEncoderGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
        ConfigureUIAsync();
    }

    internal Task? WorkTask { get; private set; }

    public IUIStack ViewStack
        => _viewStack
        .Vertical()
        .WithChildren(
            SettingGroup("jwt-encode-validate-token-setting")
                .Icon("FluentSystemIcons", '\uec9e')
                .Title(JsonWebTokenEncoderDecoder.EncodeTokenSettingsTitle)
                .Description(JsonWebTokenEncoderDecoder.EncodeTokenSettingsDescription)
                .WithChildren(
                    Setting("jwt-encode-token-algorithm-setting")
                        .Icon("FluentSystemIcons", '\uF1EE')
                        .Title(JsonWebTokenEncoderDecoder.EncodeTokenAlgorithmTitle)
                        .Handle(
                            _settingsProvider,
                            tokenAlgorithmSetting,
                            onOptionSelected: async (value) => await OnAlgorithmChangedAsync(value),
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
                    SettingGroup("jwt-encode-token-issuer-setting-group")
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
                    SettingGroup("jwt-encode-token-audience-setting-group")
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
                    SettingGroup("jwt-encode-token-expiration-setting-group")
                        .Icon("FluentSystemIcons", '\ue243')
                        .Title(JsonWebTokenEncoderDecoder.EncodeTokenHasExpirationTitle)
                        .InteractiveElement(
                            _encodeTokenHasExpirationSwitch
                                .OnText(JsonWebTokenEncoderDecoder.Yes)
                                .OffText(JsonWebTokenEncoderDecoder.No)
                                .OnToggle(OnTokenHasExpirationChanged)
                        )
                        .WithChildren(
                            _encodeTokenExpirationGrid
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
                        ),
                    Setting("jwt-encode-token-default-time-setting")
                        .Icon("FluentSystemIcons", '\ue36e')
                        .Title(JsonWebTokenEncoderDecoder.EncodeTokenHasAudienceTitle)
                        .InteractiveElement(
                            _encodeTokenDefaultTimeSwitch
                                .OnText(JsonWebTokenEncoderDecoder.Yes)
                                .OffText(JsonWebTokenEncoderDecoder.No)
                                .OnToggle(OnTokenHasDefaultTimeChanged)
                        )
                ),
            _infoBar
                .NonClosable(),
            _tokenInput
                .Title(JsonWebTokenEncoderDecoder.TokenInputTitle)
                .ReadOnly(),
            SplitGrid()
                .Vertical()
                .WithLeftPaneChild(
                    _headerInput
                        .Title(JsonWebTokenEncoderDecoder.HeaderInputTitle)
                        .Extendable()
                        .Language("json")
                        .OnTextChanged(OnTextInputChanged)
                )
                .WithRightPaneChild(
                    _payloadInput
                        .Title(JsonWebTokenEncoderDecoder.PayloadInputTitle)
                        .Extendable()
                        .Language("json")
                        .OnTextChanged(OnTextInputChanged)
                ),
            _signatureInput
                .Title(JsonWebTokenEncoderDecoder.SignatureInputTitle)
                .OnTextChanged(OnTextInputChanged),
            _privateKeyInput
                .Title(JsonWebTokenEncoderDecoder.PrivateKeyInputTitle)
                .OnTextChanged(OnTextInputChanged)

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

    private void OnTokenHasIssuerChanged(bool tokenHasIssuer)
    {
        _settingsProvider.SetSetting(tokenHasIssuerSetting, tokenHasIssuer);
        if (tokenHasIssuer)
        {
            _encodeTokenIssuerInput.Enable();
        }
        else
        {
            _encodeTokenIssuerInput.Disable();
        }
        StartTokenEncode();
    }

    private ValueTask OnTokenIssuerInputChanged(string issuer)
    {
        _settingsProvider.SetSetting(tokenIssuerSetting, issuer);
        StartTokenEncode();
        return ValueTask.CompletedTask;
    }

    private void OnTokenHasAudienceChanged(bool tokenHasAudience)
    {
        _settingsProvider.SetSetting(tokenHasAudienceSetting, tokenHasAudience);
        if (tokenHasAudience)
        {
            _encodeTokenAudienceInput.Enable();
        }
        else
        {
            _encodeTokenAudienceInput.Disable();
        }
        StartTokenEncode();
    }

    private ValueTask OnTokenAudienceInputChanged(string audience)
    {
        _settingsProvider.SetSetting(tokenAudienceSetting, audience);
        StartTokenEncode();
        return ValueTask.CompletedTask;
    }

    private void OnTokenHasDefaultTimeChanged(bool tokenHasDefaultTime)
    {
        _settingsProvider.SetSetting(tokenHasDefaultTimeSetting, tokenHasDefaultTime);
        StartTokenEncode();
    }

    private void OnTokenHasExpirationChanged(bool tokenHasExpiration)
    {
        _settingsProvider.SetSetting(tokenHasExpirationSetting, tokenHasExpiration);
        if (tokenHasExpiration)
        {
            _encodeTokenExpirationGrid.Enable();
        }
        else
        {
            _encodeTokenExpirationGrid.Disable();
        }
        StartTokenEncode();
    }

    private void OnExpirationChanged(string value, DateValueType valueChanged)
    {
        DateTimeOffset epochToUse = _settingsProvider.GetSetting(tokenExpirationSettings);

        ResultInfo<DateTimeOffset> result = DateHelper.ChangeDateTime(
            Convert.ToInt32(value),
            epochToUse,
            TimeZoneInfo.Utc,
            valueChanged);

        _settingsProvider.SetSetting(tokenExpirationSettings, result.Data);
        StartTokenEncode();
    }

    private async Task OnAlgorithmChangedAsync(JsonWebTokenAlgorithm algorithm)
    {
        _settingsProvider.SetSetting(tokenAlgorithmSetting, algorithm);
        await ConfigureTokenAlgorithmUIAsync(algorithm);
        StartTokenEncode();
    }

    private void OnTokenDefaultTimeChanged(bool tokenHasDefaultTime)
    {
        _settingsProvider.SetSetting(tokenHasDefaultTimeSetting, tokenHasDefaultTime);
        StartTokenEncode();
    }

    private void OnTextInputChanged(string text)
    {
        StartTokenEncode();
    }

    private void StartTokenEncode()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        if (string.IsNullOrWhiteSpace(_payloadInput.Text))
        {
            ClearUI();
            return;
        }

        JsonWebTokenAlgorithm tokenAlgorithm = _settingsProvider.GetSetting(tokenAlgorithmSetting);
        TokenParameters tokenParameters = new()
        {
            Payload = _payloadInput.Text,
            TokenAlgorithm = tokenAlgorithm
        };
        EncoderParameters encoderParameters = new()
        {
            HasDefaultTime = _settingsProvider.GetSetting(tokenHasDefaultTimeSetting)
        };
        bool hasAudience = _settingsProvider.GetSetting(tokenHasAudienceSetting);
        if (hasAudience)
        {
            encoderParameters.HasAudience = hasAudience;
            tokenParameters.Audiences = _settingsProvider.GetSetting(tokenAudienceSetting).Split(',').ToHashSet();
        }
        bool hasIssuer = _settingsProvider.GetSetting(tokenHasIssuerSetting);
        if (hasIssuer)
        {
            encoderParameters.HasIssuer = hasIssuer;
            tokenParameters.Issuers = _settingsProvider.GetSetting(tokenIssuerSetting).Split(',').ToHashSet();
        }
        bool hasExpiration = _settingsProvider.GetSetting(tokenHasAudienceSetting);
        if (hasExpiration)
        {
            DateTimeOffset dateTimeOffset = _settingsProvider.GetSetting(tokenExpirationSettings);
            encoderParameters.HasExpiration = hasExpiration;
            tokenParameters.DefineExpirationDate(dateTimeOffset);
        }

        if (tokenAlgorithm is JsonWebTokenAlgorithm.HS256 ||
            tokenAlgorithm is JsonWebTokenAlgorithm.HS384 ||
            tokenAlgorithm is JsonWebTokenAlgorithm.HS512)
        {
            tokenParameters.Signature = _signatureInput.Text;
        }
        else
        {
            tokenParameters.PrivateKey = _privateKeyInput.Text;
        }

        WorkTask = EncodeTokenAsync(
            encoderParameters,
            tokenParameters,
            _cancellationTokenSource.Token);
    }

    private async Task EncodeTokenAsync(
        EncoderParameters encoderParameters,
        TokenParameters tokenParameters,
        CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            ResultInfo<JsonWebTokenResult?, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
                encoderParameters,
                tokenParameters,
                _logger);

            switch (result.Severity)
            {
                case ResultInfoSeverity.Success:
                    _infoBar.Close();
                    _tokenInput.Text(result.Data!.Token!);
                    break;

                case ResultInfoSeverity.Warning:
                    _headerInput.Text(result.Data!.Header!);
                    _payloadInput.Text(result.Data!.Payload!);
                    _infoBar
                        .Description(result.ErrorMessage)
                        .Warning()
                        .Open();
                    break;

                case ResultInfoSeverity.Error:
                    _infoBar
                        .Description(result.ErrorMessage)
                        .Error()
                        .Open();
                    ClearUI();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void ClearUI()
    {
        _infoBar.Close();
        _tokenInput.Text(string.Empty);
    }

    private async Task ConfigureUIAsync()
    {
        JsonWebTokenAlgorithm tokenAlgorithm = _settingsProvider.GetSetting(tokenAlgorithmSetting);
        await ConfigureTokenAlgorithmUIAsync(tokenAlgorithm);

        ConfigureSwitch(_settingsProvider.GetSetting(tokenHasIssuerSetting), _encodeTokenHasIssuerSwitch, _encodeTokenIssuerInput);
        ConfigureSwitch(_settingsProvider.GetSetting(tokenHasAudienceSetting), _encodeTokenHasAudienceSwitch, _encodeTokenAudienceInput);
        ConfigureSwitch(_settingsProvider.GetSetting(tokenHasExpirationSetting), _encodeTokenHasExpirationSwitch, _encodeTokenExpirationGrid);
        ConfigureSwitch(_settingsProvider.GetSetting(tokenHasDefaultTimeSetting), _encodeTokenDefaultTimeSwitch);
        DateTimeOffset expirationDate = _settingsProvider.GetSetting(tokenExpirationSettings);
        _encodeTokenExpirationYearInputNumber.Value(expirationDate.Year);
        _encodeTokenExpirationMonthInputNumber.Value(expirationDate.Month);
        _encodeTokenExpirationDayInputNumber.Value(expirationDate.Day);
        _encodeTokenExpirationHourInputNumber.Value(expirationDate.Hour);
        _encodeTokenExpirationMinuteInputNumber.Value(expirationDate.Minute);
    }

    private async Task ConfigureTokenAlgorithmUIAsync(JsonWebTokenAlgorithm tokenAlgorithm)
    {
        if (tokenAlgorithm is JsonWebTokenAlgorithm.HS256 ||
            tokenAlgorithm is JsonWebTokenAlgorithm.HS384 ||
            tokenAlgorithm is JsonWebTokenAlgorithm.HS512)
        {
            _signatureInput.Show();
            _privateKeyInput.Hide();
        }
        else
        {
            _signatureInput.Hide();
            _privateKeyInput.Show();
        }

        ResultInfo<string> headerContent = await JsonHelper.FormatAsync(
            @"{""alg"": """ + tokenAlgorithm.ToString() + @""", ""typ"": ""JWT""}",
            Indentation.TwoSpaces,
            false,
            _logger,
            CancellationToken.None);

        _headerInput.Text(headerContent.Data!);
    }

    private static void ConfigureSwitch(bool value, IUISwitch inputSwitch, IUIElementWithChildren? input = null)
    {
        if (value)
        {
            inputSwitch.On();
            if (input != null)
            {
                input.Enable();
            }
            return;
        }
        inputSwitch.Off();
        if (input != null)
        {
            input.Disable();
        }
    }
}
