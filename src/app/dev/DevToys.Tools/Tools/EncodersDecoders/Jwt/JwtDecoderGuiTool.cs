using System.Reflection;
using System.Resources;
using DevToys.Api;
using DevToys.Tools.Helpers.Jwt;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.EncodersDecoders.Jwt;

internal sealed partial class JwtDecoderGuiTool
{
    /// <summary>
    /// Define if we want to validate the token or not
    /// </summary>
    private static readonly SettingDefinition<bool> validateTokenSetting
        = new(
            name: $"{nameof(JwtDecoderGuiTool)}.{nameof(validateTokenSetting)}",
            defaultValue: false);

    /// <summary>
    /// Define if we want to validate the token issuer signing key or not
    /// </summary>
    private static readonly SettingDefinition<bool> validateTokenIssuersSigningKeySetting
        = new(
            name: $"{nameof(JwtDecoderGuiTool)}.{nameof(validateTokenIssuersSigningKeySetting)}",
            defaultValue: false);

    #region TokenIssuers
    /// <summary>
    /// Define if we want to validate the token issuers or not
    /// </summary>
    private static readonly SettingDefinition<bool> validateTokenIssuersSetting
        = new(
            name: $"{nameof(JwtDecoderGuiTool)}.{nameof(validateTokenIssuersSetting)}",
            defaultValue: false);

    /// <summary>
    /// Define the issuers list
    /// </summary>
    private static readonly SettingDefinition<string> tokenIssuersSetting
        = new(
            name: $"{nameof(JwtDecoderGuiTool)}.{nameof(tokenIssuersSetting)}",
            defaultValue: string.Empty);
    #endregion

    #region TokenAudiences
    /// <summary>
    /// Define if we want to validate the token audiences or not
    /// </summary>
    private static readonly SettingDefinition<bool> validateTokenAudiencesSetting
        = new(
            name: $"{nameof(JwtDecoderGuiTool)}.{nameof(validateTokenAudiencesSetting)}",
            defaultValue: false);

    /// <summary>
    /// Define the audiences list
    /// </summary>
    private static readonly SettingDefinition<string> tokenAudiencesSetting
        = new(
            name: $"{nameof(JwtDecoderGuiTool)}.{nameof(tokenAudiencesSetting)}",
            defaultValue: string.Empty);
    #endregion

    /// <summary>
    /// Define if we want to validate the token lifetime or not
    /// </summary>
    private static readonly SettingDefinition<bool> validateTokenLifetimeSetting
        = new(
            name: $"{nameof(JwtDecoderGuiTool)}.{nameof(validateTokenLifetimeSetting)}",
            defaultValue: false);

    /// <summary>
    /// Define if we want to validate the token actors
    /// </summary>
    private static readonly SettingDefinition<bool> validateTokenActorsSetting
        = new(
            name: $"{nameof(JwtDecoderGuiTool)}.{nameof(validateTokenActorsSetting)}",
            defaultValue: false);

    private bool _showHeaderClaim;
    private bool _showPayloadClaim;
    private JwtAlgorithm? _currentAlgorithm;

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;

    private readonly IUIInfoBar _infoBar = InfoBar();
    private readonly IUIStack _viewStack = Stack("jwt-decode-view-stack");
    private readonly IUIStack _decodeSettingsStack = Stack("jwt-decode-settings-stack");

    #region Settings
    private readonly IUISwitch _validateTokenSwitch = Switch("jwt-decode-validate-token-switch");
    private readonly IUISetting _validateTokenSetting = Setting("jwt-decode-validate-token-setting");
    private readonly IUISettingGroup _validateTokenSettingGroups = SettingGroup("jwt-decode-validate-token-setting");
    #endregion

    #region TokenIssuerSigningKey
    private readonly IUISwitch _validateTokenIssuerSigningKeySwitch = Switch("jwt-decode-validate-token-issuer-signing-key-switch");
    private readonly IUISetting _validateTokenIssuerSigningKeySetting = Setting("jwt-decode-validate-token-issuer-signing-key-setting");
    #endregion

    #region TokenIssuer
    private readonly IUISwitch _validateTokenIssuersSwitch = Switch("jwt-decode-validate-token-issuers-switch");
    private readonly IUISettingGroup _validateTokenIssuersSettingGroups = SettingGroup("jwt-decode-validate-token-issuers-setting-group");
    private readonly IUISingleLineTextInput _validateTokenIssuersInput = SingleLineTextInput("jwt-decode-validate-token-issuers-input");
    #endregion

    #region TokenAudiences
    private readonly IUISwitch _validateTokenAudiencesSwitch = Switch("jwt-decode-validate-token-audiences-switch");
    private readonly IUISettingGroup _validateTokenAudiencesSettingGroups = SettingGroup("jwt-decode-validate-token-audiences-setting-group");
    private readonly IUISingleLineTextInput _validateTokenAudiencesInput = SingleLineTextInput("jwt-decode-validate-token-audiences-input");
    #endregion

    #region TokenLifetime
    private readonly IUISwitch _validateLifetimeSwitch = Switch("jwt-decode-validate-token-lifetime-switch");
    private readonly IUISetting _validateLifetimeSetting = Setting("jwt-decode-validate-token-lifetime-setting");
    #endregion

    #region TokenActors
    private readonly IUISwitch _validateActorsSwitch = Switch("jwt-decode-validate-token-actors-switch");
    private readonly IUISetting _validateActorsSetting = Setting("jwt-decode-validate-token-actors-setting");
    #endregion

    private readonly IUIMultiLineTextInput _tokenInput = MultilineTextInput("jwt-decode-token-input");
    private readonly IUIMultiLineTextInput _signatureInput = MultilineTextInput("jwt-decode-signature-input");
    private readonly IUIMultiLineTextInput _publicKeyInput = MultilineTextInput("jwt-decode-public-key-input");

    #region Header

    private readonly IUIStack _headerCommandBarStack = Stack("jwt-decode-header-stack");
    private readonly IUIDataGrid _headerClaimsDataGrid = DataGrid("jwt-decode-header-claims-data-grid");
    private readonly IUIButton _headerToggleClaimsButton = Button("jwt-decode-header-toggle-claims-button");
    private readonly IUIMultiLineTextInput _headerInput = MultilineTextInput("jwt-decode-header-input", "json");

    #endregion

    #region Payload

    private readonly IUIStack _payloadCommandBarStack = Stack("jwt-decode-payload-stack");
    private readonly IUIDataGrid _payloadClaimsDataGrid = DataGrid("jwt-decode-payload-claims-data-grid");
    private readonly IUIButton _payloadToggleClaimsButton = Button("jwt-decode-payload-toggle-claims-button");
    private readonly IUIMultiLineTextInput _payloadInput = MultilineTextInput("jwt-decode-payload-input", "json");

    #endregion

    private static readonly List<string> DateFields = new() { "exp", "nbf", "iat", "auth_time", "updated_at" };

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public JwtDecoderGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
        ConfigureUI();
        GetTokenAlgorithm();
    }

    internal Task? WorkTask { get; private set; }

    public IUIStack ViewStack()
        => _viewStack
        .Vertical()
        .WithChildren(
            _validateTokenSetting
                .Icon("FluentSystemIcons", '\ueac9')
                .Title(JwtEncoderDecoder.DecodeValidateTokenTitle)
                .InteractiveElement(
                    _validateTokenSwitch
                        .OnText(JwtEncoderDecoder.Yes)
                        .OffText(JwtEncoderDecoder.No)
                        .OnToggle(OnValidateTokenChanged)
                ),
            _validateTokenSettingGroups
                .Icon("FluentSystemIcons", '\uec9e')
                .Title(JwtEncoderDecoder.DecodeValidateTokenSettingsTitle)
                .Description(JwtEncoderDecoder.DecodeValidateTokenSettingsDescription)
                .WithChildren(
                    _validateTokenIssuerSigningKeySetting
                        .Icon("FluentSystemIcons", '\ue30a')
                        .Title(JwtEncoderDecoder.DecodeValidateTokenIssuersSigningKeyTitle)
                        .InteractiveElement(
                            _validateTokenIssuerSigningKeySwitch
                                .OnText(JwtEncoderDecoder.Yes)
                                .OffText(JwtEncoderDecoder.No)
                                .OnToggle(OnValidateTokenIssuersSigningKey)
                        ),
                    _validateTokenIssuersSettingGroups
                        .Icon("FluentSystemIcons", '\ue30a')
                        .Title(JwtEncoderDecoder.DecodeValidateTokenIssuersTitle)
                        .InteractiveElement(
                            _validateTokenIssuersSwitch
                                .OnText(JwtEncoderDecoder.Yes)
                                .OffText(JwtEncoderDecoder.No)
                                .OnToggle(OnValidateTokenIssuers)
                        )
                        .WithChildren(
                            _validateTokenIssuersInput
                                .Title(JwtEncoderDecoder.DecodeValidateTokenIssuersInputLabel)
                                .OnTextChanged(OnTokenIssuersInputChanged)
                        ),
                    _validateTokenAudiencesSettingGroups
                        .Icon("FluentSystemIcons", '\ue30a')
                        .Title(JwtEncoderDecoder.DecodeValidateTokenAudiencesTitle)
                        .InteractiveElement(
                            _validateTokenAudiencesSwitch
                                .OnText(JwtEncoderDecoder.Yes)
                                .OffText(JwtEncoderDecoder.No)
                                .OnToggle(OnValidateTokenAudiences)
                        )
                        .WithChildren(
                            _validateTokenAudiencesInput
                                .Title(JwtEncoderDecoder.DecodeValidateTokenAudiencesInputLabel)
                                .OnTextChanged(OnTokenAudiencesInputChanged)
                        ),
                    _validateLifetimeSetting
                        .Icon("FluentSystemIcons", '\ue30a')
                        .Title(JwtEncoderDecoder.DecodeValidateTokenLifetimeTitle)
                        .InteractiveElement(
                            _validateLifetimeSwitch
                                .OnText(JwtEncoderDecoder.Yes)
                                .OffText(JwtEncoderDecoder.No)
                                .OnToggle(OnValidateTokenLifetime)
                        ),
                    _validateActorsSetting
                        .Icon("FluentSystemIcons", '\ue30a')
                        .Title(JwtEncoderDecoder.DecodeValidateTokenActorsTitle)
                        .InteractiveElement(
                            _validateActorsSwitch
                                .OnText(JwtEncoderDecoder.Yes)
                                .OffText(JwtEncoderDecoder.No)
                                .OnToggle(OnValidateTokenActors)
                        )
                ),
            _infoBar
                .NonClosable(),
            _tokenInput
                .Title(JwtEncoderDecoder.TokenInputTitle)
                .OnTextChanged(OnTokenInputChanged),
            SplitGrid()
                .Vertical()
                .WithLeftPaneChild(
                    Stack()
                        .Vertical()
                        .WithChildren(
                            _headerInput
                                .Title(JwtEncoderDecoder.HeaderInputTitle)
                                .ReadOnly()
                                .CommandBarExtraContent(
                                    _headerCommandBarStack
                                        .Horizontal()
                                        .WithChildren(
                                            _headerToggleClaimsButton
                                                .Icon("FluentSystemIcons", '\uf4a5')
                                                .OnClick(OnHeaderClaimClicked)
                                        )
                                ),
                            _headerClaimsDataGrid
                                .Extendable()
                                .CommandBarExtraContent(
                                    _headerCommandBarStack
                                        .Horizontal()
                                        .WithChildren(
                                            _headerToggleClaimsButton
                                                .Icon("FluentSystemIcons", '\uf4a5')
                                                .OnClick(OnHeaderClaimClicked)
                                        )
                                )
                                .WithColumns(JwtEncoderDecoder.ClaimTypeTitle, JwtEncoderDecoder.ClaimValueTitle, JwtEncoderDecoder.ClaimDescriptionTitle)
                        )
                )
                .WithRightPaneChild(
                    Stack()
                        .Vertical()
                        .WithChildren(
                            _payloadInput
                                .Title(JwtEncoderDecoder.PayloadInputTitle)
                                .ReadOnly()
                                .Extendable()
                                .CommandBarExtraContent(
                                    _payloadCommandBarStack
                                        .Horizontal()
                                        .WithChildren(
                                            _payloadToggleClaimsButton
                                                .Icon("FluentSystemIcons", '\uf4a5')
                                                .OnClick(OnPayloadClaimClicked)
                                        )
                                ),
                            _payloadClaimsDataGrid
                                .Extendable()
                                .CommandBarExtraContent(
                                    _payloadCommandBarStack
                                        .Horizontal()
                                        .WithChildren(
                                            _payloadToggleClaimsButton
                                                .Icon("FluentSystemIcons", '\uf4a5')
                                                .OnClick(OnPayloadClaimClicked)
                                        )
                                )
                                .WithColumns(JwtEncoderDecoder.ClaimTypeTitle, JwtEncoderDecoder.ClaimValueTitle, JwtEncoderDecoder.ClaimDescriptionTitle)
                        )
                ),
            _signatureInput
                .Title(JwtEncoderDecoder.SignatureInputTitle)
                .OnTextChanged(OnTokenInputChanged),
            _publicKeyInput
                .Title(JwtEncoderDecoder.PublicKeyInputTitle)
                .OnTextChanged(OnTokenInputChanged)
        );

    public void Show()
    {
        _viewStack.Show();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
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
        _settingsProvider.SetSetting(JwtDecoderGuiTool.validateTokenSetting, validateToken);
        ConfigureUI();
        GetTokenAlgorithm();
        StartTokenDecode();
    }

    #region TokenIssuers

    private void OnValidateTokenIssuersSigningKey(bool validateTokenIssuersSigningKey)
    {
        _settingsProvider.SetSetting(JwtDecoderGuiTool.validateTokenIssuersSigningKeySetting, validateTokenIssuersSigningKey);
        StartTokenDecode();
    }

    private void OnValidateTokenIssuers(bool validateTokenIssuers)
    {
        _settingsProvider.SetSetting(JwtDecoderGuiTool.validateTokenIssuersSetting, validateTokenIssuers);
        StartTokenDecode();
    }

    private ValueTask OnTokenIssuersInputChanged(string issuers)
    {
        _settingsProvider.SetSetting(JwtDecoderGuiTool.tokenIssuersSetting, issuers);
        StartTokenDecode();
        return ValueTask.CompletedTask;
    }

    #endregion

    #region TokenAudiences

    private void OnValidateTokenAudiences(bool validateTokenAudiences)
    {
        _settingsProvider.SetSetting(JwtDecoderGuiTool.validateTokenAudiencesSetting, validateTokenAudiences);
        StartTokenDecode();
    }

    private ValueTask OnTokenAudiencesInputChanged(string audiences)
    {
        _settingsProvider.SetSetting(JwtDecoderGuiTool.tokenAudiencesSetting, audiences);
        StartTokenDecode();
        return ValueTask.CompletedTask;
    }

    #endregion

    #region TokenLifetime

    private void OnValidateTokenLifetime(bool validateTokenLifetime)
    {
        _settingsProvider.SetSetting(JwtDecoderGuiTool.validateTokenLifetimeSetting, validateTokenLifetime);
        StartTokenDecode();
    }

    #endregion

    #region TokenActors

    private void OnValidateTokenActors(bool validateTokenActors)
    {
        _settingsProvider.SetSetting(JwtDecoderGuiTool.validateTokenActorsSetting, validateTokenActors);
        StartTokenDecode();
    }

    #endregion

    private void OnHeaderClaimClicked()
    {
        if (_showHeaderClaim)
        {
            _headerInput.Show();
            _headerClaimsDataGrid.Hide();
            _showHeaderClaim = false;
            return;
        }

        _headerInput.Hide();
        _headerClaimsDataGrid.Show();
        _showHeaderClaim = true;
    }

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
            return;
        }

        TokenParameters tokenParameters = new(_tokenInput.Text);

        DecoderParameters decoderParameters = new();
        bool validateTokenSignature = _settingsProvider.GetSetting(validateTokenSetting);
        if (validateTokenSignature)
        {
            decoderParameters.ValidateSignature = validateTokenSignature;
            decoderParameters.ValidateActors = _settingsProvider.GetSetting(validateTokenActorsSetting);
            decoderParameters.ValidateLifetime = _settingsProvider.GetSetting(validateTokenLifetimeSetting);
            decoderParameters.ValidateIssuersSigningKey = _settingsProvider.GetSetting(validateTokenIssuersSigningKeySetting);

            bool validateIssuers = _settingsProvider.GetSetting(validateTokenIssuersSetting);
            if (validateIssuers)
            {
                decoderParameters.ValidateIssuers = validateIssuers;
                tokenParameters.Issuers = _settingsProvider.GetSetting(tokenIssuersSetting).Split(',').ToHashSet();
            }

            bool validateAudiences = _settingsProvider.GetSetting(validateTokenAudiencesSetting);
            if (validateAudiences)
            {
                decoderParameters.ValidateAudiences = validateAudiences;
                tokenParameters.Audiences = _settingsProvider.GetSetting(tokenAudiencesSetting).Split(',').ToHashSet();
            }
        }

        if (_currentAlgorithm is JwtAlgorithm.HS256 ||
            _currentAlgorithm is JwtAlgorithm.HS384 ||
            _currentAlgorithm is JwtAlgorithm.HS512)
        {
            tokenParameters.Signature = _signatureInput.Text;
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
            ResultInfo<JwtTokenResult?, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
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
                    BuildClaimsDataGrid(_headerInput, _headerClaimsDataGrid, result.Data.HeaderClaims);
                    BuildClaimsDataGrid(_payloadInput, _payloadClaimsDataGrid, result.Data.PayloadClaims);
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

    private void ConfigureUI()
    {
        bool validateToken = _settingsProvider.GetSetting(JwtDecoderGuiTool.validateTokenSetting);
        if (validateToken)
        {
            _validateTokenSwitch.On();
            _validateTokenSettingGroups.Show();
            _signatureInput.Show();
        }
        else
        {
            _validateTokenSwitch.Off();
            _validateTokenSettingGroups.Hide();
            _signatureInput.Hide();
            _publicKeyInput.Hide();
            _infoBar.Close();
        }
        _headerClaimsDataGrid.Hide();
        _payloadClaimsDataGrid.Hide();
    }

    private void GetTokenAlgorithm()
    {
        bool validateToken = _settingsProvider.GetSetting(JwtDecoderGuiTool.validateTokenSetting);
        if (validateToken && !string.IsNullOrWhiteSpace(_tokenInput.Text))
        {
            ResultInfo<JwtAlgorithm?> tokenAlgorithm = JwtDecoderHelper.GetTokenAlgorithm(_tokenInput.Text, _logger);
            if (!tokenAlgorithm.HasSucceeded)
            {
                _infoBar
                    .Description(tokenAlgorithm.ErrorMessage)
                    .Error()
                    .Show();
                return;
            }
            _currentAlgorithm = tokenAlgorithm.Data;
            if (tokenAlgorithm.Data is JwtAlgorithm.HS256 ||
                tokenAlgorithm.Data is JwtAlgorithm.HS384 ||
                tokenAlgorithm.Data is JwtAlgorithm.HS512)
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

    private static void BuildClaimsDataGrid(IUIMultiLineTextInput multilineInput, IUIDataGrid dataGrid, List<JwtClaim> claims)
    {
        dataGrid.Rows.Clear();
        var rows = new List<IUIDataGridRow>();
        var tooltips = new List<UIHoverTooltip>();
        foreach (JwtClaim claim in claims)
        {
            IUIDataGridCell typeCell = Cell(claim.Key);
            IUIDataGridCell valueCell = Cell(claim.Value);

            string? localizedDescription = JwtEncoderDecoder.ResourceManager.GetString(claim.Key);
            if (!string.IsNullOrWhiteSpace(localizedDescription))
            {
                IUIDataGridCell descriptionCell = Cell(localizedDescription);
                rows.Add(Row(null, typeCell, valueCell, descriptionCell));
                UIHoverTooltip tooltip = new(claim.Key, localizedDescription);
                tooltips.Add(tooltip);
            }
        }
        multilineInput.Tooltip(tooltips.ToArray());
        dataGrid.Rows.AddRange(rows);
    }
}
