using System.Security.Authentication;
using System.Security.Cryptography;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;

internal sealed partial class JsonWebTokenEncoderGuiTool
{
    /// <summary>
    /// Define if we want to validate the token or not
    /// </summary>
    private static readonly SettingDefinition<JsonWebTokenAlgorithm> tokenAlgorithmSetting
        = new(
            name: $"{nameof(JsonWebTokenEncoderGuiTool)}.{nameof(tokenAlgorithmSetting)}",
            defaultValue: JsonWebTokenAlgorithm.HS256);

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;

    #region Settings
    private readonly IUISettingGroup _encodeTokenSettingGroups = SettingGroup("jwt-encode-validate-token-setting");
    #endregion

    private readonly IUISettingGroup _encodeTokenAlgorithmSettingGroups = SettingGroup("jwt-encode-token-algorithm-setting");

    private readonly IUIStack _viewStack = Stack("jwt-encode-view-stack");
    private readonly IUIStack _encodeSettingsStack = Stack("jwt-encode-settings-stack");

    [ImportingConstructor]
    public JsonWebTokenEncoderGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
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
                    _encodeTokenAlgorithmSettingGroups
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
                        )

                )
        );

    public void Show()
        => _viewStack.Show();

    public void Hide()
        => _viewStack.Hide();


    private void OnAlgorithmChanged(JsonWebTokenAlgorithm hashAlgorithm)
    {
    }
}
