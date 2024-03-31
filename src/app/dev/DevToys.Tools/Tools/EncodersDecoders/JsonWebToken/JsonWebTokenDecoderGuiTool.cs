using DevToys.Tools.Helpers.JsonWebToken;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;

internal sealed class JsonWebTokenDecoderGuiTool
{
    /// <summary>
    /// Define if we want to validate the token or not
    /// </summary>
    private static readonly SettingDefinition<bool> validateTokenSetting
        = new(
            name: $"{nameof(JsonWebTokenDecoderGuiTool)}.{nameof(validateTokenSetting)}",
            defaultValue: false);

    /// <summary>
    /// Define if we want to validate the token issuer signing key or not
    /// </summary>
    private static readonly SettingDefinition<bool> validateTokenIssuerSigningKeySetting
        = new(
            name: $"{nameof(JsonWebTokenDecoderGuiTool)}.{nameof(validateTokenIssuerSigningKeySetting)}",
            defaultValue: false);

    /// <summary>
    /// Defines whether the signature key is in Base64 format or not (Plain text).
    /// </summary>
    private static readonly SettingDefinition<bool> isSignatureKeyInBase64FormatSwitchSetting
        = new(
            name: $"{nameof(JsonWebTokenDecoderGuiTool)}.{nameof(isSignatureKeyInBase64FormatSwitchSetting)}",
            defaultValue: false);

    #region TokenIssuers
    /// <summary>
    /// Define if we want to validate the token issuers or not
    /// </summary>
    private static readonly SettingDefinition<bool> validateTokenIssuerSetting
        = new(
            name: $"{nameof(JsonWebTokenDecoderGuiTool)}.{nameof(validateTokenIssuerSetting)}",
            defaultValue: false);

    /// <summary>
    /// Define the issuers list
    /// </summary>
    private static readonly SettingDefinition<string> tokenIssuerSetting
        = new(
            name: $"{nameof(JsonWebTokenDecoderGuiTool)}.{nameof(tokenIssuerSetting)}",
            defaultValue: string.Empty);
    #endregion

    #region TokenAudiences
    /// <summary>
    /// Define if we want to validate the token audiences or not
    /// </summary>
    private static readonly SettingDefinition<bool> validateTokenAudiencesSetting
        = new(
            name: $"{nameof(JsonWebTokenDecoderGuiTool)}.{nameof(validateTokenAudiencesSetting)}",
            defaultValue: false);

    /// <summary>
    /// Define the audiences list
    /// </summary>
    private static readonly SettingDefinition<string> tokenAudiencesSetting
        = new(
            name: $"{nameof(JsonWebTokenDecoderGuiTool)}.{nameof(tokenAudiencesSetting)}",
            defaultValue: string.Empty);
    #endregion

    /// <summary>
    /// Define if we want to validate the token lifetime or not
    /// </summary>
    private static readonly SettingDefinition<bool> validateTokenLifetimeSetting
        = new(
            name: $"{nameof(JsonWebTokenDecoderGuiTool)}.{nameof(validateTokenLifetimeSetting)}",
            defaultValue: false);

    /// <summary>
    /// Define if we want to validate the token actors
    /// </summary>
    private static readonly SettingDefinition<bool> validateTokenActorsSetting
        = new(
            name: $"{nameof(JsonWebTokenDecoderGuiTool)}.{nameof(validateTokenActorsSetting)}",
            defaultValue: false);

    private bool _showPayloadClaim;
    private JsonWebTokenAlgorithm _currentAlgorithm = JsonWebTokenAlgorithm.HS256;

    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;

    private readonly IUIInfoBar _infoBar = InfoBar("jwt-decode-info-bar");
    private readonly IUIStack _viewStack = Stack("jwt-decode-view-stack");
    private readonly IUIStack _decodeSettingsStack = Stack("jwt-decode-settings-stack");

    private readonly IUISwitch _validateTokenSwitch = Switch("jwt-decode-validate-token-switch");
    private readonly IUISwitch _validateTokenIssuerSigningKeySwitch = Switch("jwt-decode-validate-token-issuer-signing-key-switch");
    private readonly IUISwitch _isSignatureKeyInBase64FormatSwitch = Switch("jwt-decode-is-signature-key-base64-format-switch");
    private readonly IUISwitch _validateTokenIssuersSwitch = Switch("jwt-decode-validate-token-issuers-switch");
    private readonly IUISwitch _validateTokenAudiencesSwitch = Switch("jwt-decode-validate-token-audiences-switch");
    private readonly IUISwitch _validateLifetimeSwitch = Switch("jwt-decode-validate-token-lifetime-switch");
    private readonly IUISwitch _validateActorsSwitch = Switch("jwt-decode-validate-token-actors-switch");

    private readonly IUISingleLineTextInput _validateTokenIssuersInput = SingleLineTextInput("jwt-decode-validate-token-issuers-input");
    private readonly IUISingleLineTextInput _validateTokenAudiencesInput = SingleLineTextInput("jwt-decode-validate-token-audiences-input");

    private readonly IUIMultiLineTextInput _tokenInput = MultilineTextInput("jwt-decode-token-input");
    private readonly IUIMultiLineTextInput _headerInput = MultilineTextInput("jwt-decode-header-input", "json");
    private readonly IUIMultiLineTextInput _payloadInput = MultilineTextInput("jwt-decode-payload-input", "json");
    private readonly IUIMultiLineTextInput _signatureInput = MultilineTextInput("jwt-decode-signature-input");
    private readonly IUIMultiLineTextInput _publicKeyInput = MultilineTextInput("jwt-decode-public-key-input");

    private readonly IUIDataGrid _payloadClaimsDataGrid = DataGrid("jwt-decode-payload-claims-data-grid");

    private static readonly List<string> dateFields = new() { "exp", "nbf", "iat", "auth_time", "updated_at" };

    private DisposableSemaphore _semaphore = new();
    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public JsonWebTokenDecoderGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
        ConfigureUI();
        GetTokenAlgorithm();
    }

    internal Task? WorkTask { get; private set; }

    public IUIStack ViewStack
        => _viewStack
        .Vertical()
        .WithChildren(
            SettingGroup("jwt-decode-validate-token-setting")
                .Icon("FluentSystemIcons", '\uec9e')
                .Title(JsonWebTokenEncoderDecoder.DecodeValidateTokenSettingsTitle)
                .Description(JsonWebTokenEncoderDecoder.DecodeValidateTokenSettingsDescription)
                .InteractiveElement(
                    _validateTokenSwitch
                        .OnText(JsonWebTokenEncoderDecoder.Yes)
                        .OffText(JsonWebTokenEncoderDecoder.No)
                        .OnToggle(OnValidateTokenChanged)
                )
                .WithChildren(
                    SettingGroup("jwt-decode-validate-token-issuer-signing-key-setting")
                        .Icon("FluentSystemIcons", '\ue30a')
                        .Title(JsonWebTokenEncoderDecoder.DecodeValidateTokenIssuerSigningKeyTitle)
                        .InteractiveElement(
                            _validateTokenIssuerSigningKeySwitch
                                .OnText(JsonWebTokenEncoderDecoder.Yes)
                                .OffText(JsonWebTokenEncoderDecoder.No)
                                .OnToggle(OnValidateTokenIssuerSigningKey)
                        )
                        .WithChildren(
                            _isSignatureKeyInBase64FormatSwitch
                                .AlignHorizontally(UIHorizontalAlignment.Right)
                                .OnText(JsonWebTokenEncoderDecoder.Base64)
                                .OffText(JsonWebTokenEncoderDecoder.PlainText)
                                .OnToggle(OnIsSigningKeyBase64)
                        ),
                    SettingGroup("jwt-decode-validate-token-issuers-setting-group")
                        .Icon("FluentSystemIcons", '\ue30a')
                        .Title(JsonWebTokenEncoderDecoder.DecodeValidateTokenIssuerTitle)
                        .InteractiveElement(
                            _validateTokenIssuersSwitch
                                .OnText(JsonWebTokenEncoderDecoder.Yes)
                                .OffText(JsonWebTokenEncoderDecoder.No)
                                .OnToggle(OnValidateTokenIssuer)
                        )
                        .WithChildren(
                            _validateTokenIssuersInput
                                .Title(JsonWebTokenEncoderDecoder.DecodeValidateTokenIssuerInputLabel)
                                .OnTextChanged(OnTokenIssuerInputChanged)
                        ),
                    SettingGroup("jwt-decode-validate-token-audiences-setting-group")
                        .Icon("FluentSystemIcons", '\ue30a')
                        .Title(JsonWebTokenEncoderDecoder.DecodeValidateTokenAudiencesTitle)
                        .InteractiveElement(
                            _validateTokenAudiencesSwitch
                                .OnText(JsonWebTokenEncoderDecoder.Yes)
                                .OffText(JsonWebTokenEncoderDecoder.No)
                                .OnToggle(OnValidateTokenAudiences)
                        )
                        .WithChildren(
                            _validateTokenAudiencesInput
                                .Title(JsonWebTokenEncoderDecoder.DecodeValidateTokenAudiencesInputLabel)
                                .OnTextChanged(OnTokenAudiencesInputChanged)
                        ),
                    Setting("jwt-decode-validate-token-lifetime-setting")
                        .Icon("FluentSystemIcons", '\ue30a')
                        .Title(JsonWebTokenEncoderDecoder.DecodeValidateTokenLifetimeTitle)
                        .InteractiveElement(
                            _validateLifetimeSwitch
                                .OnText(JsonWebTokenEncoderDecoder.Yes)
                                .OffText(JsonWebTokenEncoderDecoder.No)
                                .OnToggle(OnValidateTokenLifetime)
                        ),
                    Setting("jwt-decode-validate-token-actors-setting")
                        .Icon("FluentSystemIcons", '\ue30a')
                        .Title(JsonWebTokenEncoderDecoder.DecodeValidateTokenActorsTitle)
                        .InteractiveElement(
                            _validateActorsSwitch
                                .OnText(JsonWebTokenEncoderDecoder.Yes)
                                .OffText(JsonWebTokenEncoderDecoder.No)
                                .OnToggle(OnValidateTokenActors)
                        )
                ),
            _infoBar
                .NonClosable(),
            _tokenInput
                .Title(JsonWebTokenEncoderDecoder.TokenInputTitle)
                .AlwaysWrap()
                .OnTextChanged(OnTokenInputChanged),
            SplitGrid()
                .Vertical()
                .WithLeftPaneChild(
                    _headerInput
                        .Title(JsonWebTokenEncoderDecoder.HeaderInputTitle)
                        .ReadOnly()
                )
                .WithRightPaneChild(
                    Stack()
                        .Vertical()
                        .WithChildren(
                            _payloadInput
                                .Title(JsonWebTokenEncoderDecoder.PayloadInputTitle)
                                .ReadOnly()
                                .Extendable()
                                .CommandBarExtraContent(
                                    Stack("jwt-decode-payload-stack")
                                        .Horizontal()
                                        .WithChildren(
                                            Button("jwt-decode-payload-claims-toggle-button")
                                                .Icon("FluentSystemIcons", '\uf4a5')
                                                .OnClick(OnPayloadClaimClicked)
                                        )
                                ),
                            _payloadClaimsDataGrid
                                .Extendable()
                                .CommandBarExtraContent(
                                    Stack("jwt-decode-payload-claims-stack")
                                        .Horizontal()
                                        .WithChildren(
                                            Button("jwt-decode-payload-claims-toggle-button")
                                                .Icon("FluentSystemIcons", '\uf4a5')
                                                .OnClick(OnPayloadClaimClicked)
                                        )
                                )
                                .WithColumns(JsonWebTokenEncoderDecoder.ClaimTypeTitle, JsonWebTokenEncoderDecoder.ClaimValueTitle)
                        )
                ),
            _signatureInput
                .Title(JsonWebTokenEncoderDecoder.SignatureInputTitle)
                .OnTextChanged(OnTokenInputChanged),
            _publicKeyInput
                .Title(JsonWebTokenEncoderDecoder.PublicKeyInputTitle)
                .OnTextChanged(OnTokenInputChanged)
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

    private void OnValidateTokenChanged(bool validateToken)
    {
        _settingsProvider.SetSetting(validateTokenSetting, validateToken);
        ConfigureUI();
        GetTokenAlgorithm();
        StartTokenDecode();
    }

    #region TokenIssuer

    private void OnValidateTokenIssuerSigningKey(bool validateTokenIssuerSigningKey)
    {
        _settingsProvider.SetSetting(validateTokenIssuerSigningKeySetting, validateTokenIssuerSigningKey);
        if (validateTokenIssuerSigningKey)
        {
            _isSignatureKeyInBase64FormatSwitch.Enable();
        }
        else
        {
            _isSignatureKeyInBase64FormatSwitch.Disable();
        }
        StartTokenDecode();
    }

    private void OnIsSigningKeyBase64(bool value)
    {
        _settingsProvider.SetSetting(isSignatureKeyInBase64FormatSwitchSetting, value);
        StartTokenDecode();
    }

    private void OnValidateTokenIssuer(bool validateTokenIssuer)
    {
        _settingsProvider.SetSetting(validateTokenIssuerSetting, validateTokenIssuer);
        if (validateTokenIssuer)
        {
            _validateTokenIssuersInput.Enable();
        }
        else
        {
            _validateTokenIssuersInput.Disable();
        }
        StartTokenDecode();
    }

    private ValueTask OnTokenIssuerInputChanged(string issuer)
    {
        _settingsProvider.SetSetting(tokenIssuerSetting, issuer);
        StartTokenDecode();
        return ValueTask.CompletedTask;
    }

    #endregion

    #region TokenAudiences

    private void OnValidateTokenAudiences(bool validateTokenAudiences)
    {
        _settingsProvider.SetSetting(validateTokenAudiencesSetting, validateTokenAudiences);
        if (validateTokenAudiences)
        {
            _validateTokenAudiencesInput.Enable();
        }
        else
        {
            _validateTokenAudiencesInput.Disable();
        }
        StartTokenDecode();
    }

    private ValueTask OnTokenAudiencesInputChanged(string audiences)
    {
        _settingsProvider.SetSetting(tokenAudiencesSetting, audiences);
        StartTokenDecode();
        return ValueTask.CompletedTask;
    }

    #endregion

    #region TokenLifetime

    private void OnValidateTokenLifetime(bool validateTokenLifetime)
    {
        _settingsProvider.SetSetting(validateTokenLifetimeSetting, validateTokenLifetime);
        StartTokenDecode();
    }

    #endregion

    #region TokenActors

    private void OnValidateTokenActors(bool validateTokenActors)
    {
        _settingsProvider.SetSetting(validateTokenActorsSetting, validateTokenActors);
        StartTokenDecode();
    }

    #endregion

    private void OnPayloadClaimClicked()
    {
        if (_showPayloadClaim)
        {
            _payloadInput.Show();
            _payloadClaimsDataGrid.Hide();
            _showPayloadClaim = false;
            return;
        }

        _payloadInput.Hide();
        _payloadClaimsDataGrid.Show();
        _showPayloadClaim = true;
    }

    private void OnTokenInputChanged(string text)
    {
        GetTokenAlgorithm();
        StartTokenDecode();
    }

    private void StartTokenDecode()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        if (string.IsNullOrWhiteSpace(_tokenInput.Text))
        {
            ClearUI();
            _infoBar.Close();
            return;
        }

        TokenParameters tokenParameters = new()
        {
            Token = _tokenInput.Text
        };

        DecoderParameters decoderParameters = new();
        bool validateTokenSignature = _settingsProvider.GetSetting(validateTokenSetting);
        if (validateTokenSignature)
        {
            decoderParameters.ValidateSignature = validateTokenSignature;
            decoderParameters.ValidateActors = _settingsProvider.GetSetting(validateTokenActorsSetting);
            decoderParameters.ValidateLifetime = _settingsProvider.GetSetting(validateTokenLifetimeSetting);
            decoderParameters.ValidateIssuersSigningKey = _settingsProvider.GetSetting(validateTokenIssuerSigningKeySetting);

            bool validateIssuers = _settingsProvider.GetSetting(validateTokenIssuerSetting);
            if (validateIssuers)
            {
                decoderParameters.ValidateIssuers = validateIssuers;
                tokenParameters.Issuers = _settingsProvider.GetSetting(tokenIssuerSetting).Split(',').ToHashSet();
            }

            bool validateAudiences = _settingsProvider.GetSetting(validateTokenAudiencesSetting);
            if (validateAudiences)
            {
                decoderParameters.ValidateAudiences = validateAudiences;
                tokenParameters.Audiences = _settingsProvider.GetSetting(tokenAudiencesSetting).Split(',').ToHashSet();
            }
        }

        tokenParameters.TokenAlgorithm = _currentAlgorithm;
        if (_currentAlgorithm is JsonWebTokenAlgorithm.HS256 ||
            _currentAlgorithm is JsonWebTokenAlgorithm.HS384 ||
            _currentAlgorithm is JsonWebTokenAlgorithm.HS512)
        {
            tokenParameters.Signature = _signatureInput.Text;
            tokenParameters.IsSignatureInBase64Format = _settingsProvider.GetSetting(isSignatureKeyInBase64FormatSwitchSetting);
        }
        else
        {
            tokenParameters.PublicKey = _publicKeyInput.Text;
        }

        WorkTask = DecodeTokenAsync(
            decoderParameters,
            tokenParameters,
            _cancellationTokenSource.Token);
    }

    private async Task DecodeTokenAsync(
        DecoderParameters decoderParameters,
        TokenParameters tokenParameters,
        CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            ResultInfo<JsonWebTokenResult?> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
                decoderParameters,
                tokenParameters,
                _logger,
                cancellationToken);

            switch (result.Severity)
            {
                case ResultInfoSeverity.Success:
                    _infoBar.Close();
                    _headerInput.Text(result.Data!.Header!);
                    _payloadInput.Text(result.Data!.Payload!);
                    _infoBar
                        .Description(JsonWebTokenEncoderDecoder.ValidToken)
                        .Success()
                        .Open();
                    BuildClaimsDataGrid(_payloadInput, _payloadClaimsDataGrid, result.Data.PayloadClaims);
                    break;

                case ResultInfoSeverity.Warning:
                    _headerInput.Text(result.Data!.Header!);
                    _payloadInput.Text(result.Data!.Payload!);
                    _infoBar
                        .Description(result.Message)
                        .Warning()
                        .Open();
                    break;

                case ResultInfoSeverity.Error:
                    _infoBar
                        .Description(result.Message)
                        .Error()
                        .Open();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void ConfigureUI()
    {
        bool validateToken = _settingsProvider.GetSetting(validateTokenSetting);
        if (validateToken)
        {
            _validateTokenSwitch.On();
            _signatureInput.Show();
            _validateTokenIssuerSigningKeySwitch.Enable();
            _isSignatureKeyInBase64FormatSwitch.Enable();
            _validateTokenIssuersSwitch.Enable();
            _validateTokenAudiencesSwitch.Enable();
            _validateLifetimeSwitch.Enable();
            _validateActorsSwitch.Enable();
        }
        else
        {
            _validateTokenSwitch.Off();
            _signatureInput.Hide();
            _isSignatureKeyInBase64FormatSwitch.Disable();
            _publicKeyInput.Hide();
            _infoBar.Close();
            _validateTokenIssuerSigningKeySwitch.Disable();
            _validateTokenIssuersSwitch.Disable();
            _validateTokenAudiencesSwitch.Disable();
            _validateLifetimeSwitch.Disable();
            _validateActorsSwitch.Disable();
        }

        ConfigureSwitch(_settingsProvider.GetSetting(validateTokenIssuerSigningKeySetting), _validateTokenIssuerSigningKeySwitch);
        ConfigureSwitch(_settingsProvider.GetSetting(validateTokenIssuerSetting), _validateTokenIssuersSwitch, _validateTokenIssuersInput);
        ConfigureSwitch(_settingsProvider.GetSetting(validateTokenAudiencesSetting), _validateTokenAudiencesSwitch, _validateTokenAudiencesInput);
        ConfigureSwitch(_settingsProvider.GetSetting(validateTokenLifetimeSetting), _validateLifetimeSwitch);
        ConfigureSwitch(_settingsProvider.GetSetting(validateTokenActorsSetting), _validateActorsSwitch);

        _validateTokenIssuersInput.Text(_settingsProvider.GetSetting(tokenIssuerSetting));
        _validateTokenAudiencesInput.Text(_settingsProvider.GetSetting(tokenAudiencesSetting));

        _payloadClaimsDataGrid.Hide();
    }

    private void GetTokenAlgorithm()
    {
        bool validateToken = _settingsProvider.GetSetting(validateTokenSetting);
        if (validateToken && !string.IsNullOrWhiteSpace(_tokenInput.Text))
        {
            ResultInfo<JsonWebTokenAlgorithm?> tokenAlgorithm = JsonWebTokenDecoderHelper.GetTokenAlgorithm(_tokenInput.Text, _logger);
            if (!tokenAlgorithm.HasSucceeded)
            {
                _infoBar
                    .Description(tokenAlgorithm.Message)
                    .Error()
                    .Show();
                return;
            }
            _currentAlgorithm = tokenAlgorithm.Data.GetValueOrDefault();
            if (tokenAlgorithm.Data is JsonWebTokenAlgorithm.HS256 ||
                tokenAlgorithm.Data is JsonWebTokenAlgorithm.HS384 ||
                tokenAlgorithm.Data is JsonWebTokenAlgorithm.HS512)
            {
                _signatureInput.Show();
                _publicKeyInput.Hide();
            }
            else
            {
                _signatureInput.Hide();
                _publicKeyInput.Show();
            }
        }
        else if (validateToken)
        {
            _signatureInput.Hide();
            _publicKeyInput.Hide();
        }
    }

    private void ClearUI()
    {
        _headerInput.Text(string.Empty);
        _payloadInput.Text(string.Empty);
    }

    private static void BuildClaimsDataGrid(IUIMultiLineTextInput multilineInput, IUIDataGrid dataGrid, List<JsonWebTokenClaim> claims)
    {
        dataGrid.Rows.Clear();
        var rows = new List<IUIDataGridRow>();
        var tooltips = new List<UIHoverTooltip>();
        foreach (JsonWebTokenClaim claim in claims)
        {
            IUIDataGridCell typeCell = Cell(claim.Key);
            IUIDataGridCell valueCell = Cell(claim.Value);

            string? localizedDescription = JsonWebTokenEncoderDecoder.ResourceManager.GetString(claim.Key);
            if (!string.IsNullOrWhiteSpace(localizedDescription))
            {
                rows.Add(Row(null, typeCell, valueCell));
                UIHoverTooltip tooltip = new(claim.Span, localizedDescription);
                tooltips.Add(tooltip);
            }
        }
        multilineInput.HoverTooltip(tooltips.ToArray());
        dataGrid.Rows.AddRange(rows);
    }

    private static void ConfigureSwitch(bool value, IUISwitch inputSwitch, IUISingleLineTextInput? input = null)
    {
        if (value)
        {
            inputSwitch.On();
            input?.Enable();
            return;
        }
        inputSwitch.Off();
        input?.Disable();
    }
}
