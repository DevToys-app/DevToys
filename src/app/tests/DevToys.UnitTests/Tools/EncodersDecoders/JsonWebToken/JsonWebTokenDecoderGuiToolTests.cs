using System.Globalization;
using System.Threading.Tasks;
using DevToys.Core.Tools.Metadata;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;
using static DevToys.UnitTests.Tools.Helpers.JsonWebTokenEncoderDecoderDataProvider;

namespace DevToys.UnitTests.Tools.EncodersDecoders.JsonWebToken;

public sealed class JsonWebTokenDecoderGuiToolTests : MefBasedTest
{
    private readonly JsonWebTokenDecoderGuiTool _decodeTool;
    private readonly JsonWebTokenEncoderDecoderGuiTool _tool;
    private readonly UIToolView _toolView;

    private readonly IUISwitch _toolMode;

    private readonly IUIInfoBar _infoBar;

    private readonly IUISwitch _validateTokenSwitch;
    private readonly IUISwitch _validateIssuersSwitch;
    private readonly IUISwitch _validateIssuerSigningKeySwitch;
    private readonly IUISwitch _validateAudiencesSwitch;
    private readonly IUISwitch _validateLifetimeSwitch;
    private readonly IUISwitch _validateActorsSwitch;

    private readonly IUIMultiLineTextInput _tokenInput;
    private readonly IUIMultiLineTextInput _headerInput;
    private readonly IUIMultiLineTextInput _payloadInput;
    private readonly IUIMultiLineTextInput _signatureInput;
    private readonly IUIMultiLineTextInput _publicKeyInput;

    private readonly IUISingleLineTextInput _validateIssuersInput;
    private readonly IUISingleLineTextInput _validateAudiencesInput;

    public JsonWebTokenDecoderGuiToolTests()
        : base(typeof(JsonWebTokenDecoderGuiTool).Assembly)
    {
        _tool = (JsonWebTokenEncoderDecoderGuiTool)MefProvider.ImportMany<IGuiTool, GuiToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "JsonWebTokenEncoderDecoder")
            .Value;

        _toolView = _tool.View;
        _toolMode = (IUISwitch)_toolView.GetChildElementById("jwt-token-conversion-mode-switch");

        _decodeTool = _tool.DecoderGuiTool;
        _infoBar = (IUIInfoBar)_toolView.GetChildElementById("jwt-decode-info-bar");
        _validateTokenSwitch = (IUISwitch)_toolView.GetChildElementById("jwt-decode-validate-token-switch");
        _validateIssuerSigningKeySwitch = (IUISwitch)_toolView.GetChildElementById("jwt-decode-validate-token-issuer-signing-key-switch");
        _validateIssuersSwitch = (IUISwitch)_toolView.GetChildElementById("jwt-decode-validate-token-issuers-switch");
        _validateAudiencesSwitch = (IUISwitch)_toolView.GetChildElementById("jwt-decode-validate-token-audiences-switch");
        _validateLifetimeSwitch = (IUISwitch)_toolView.GetChildElementById("jwt-decode-validate-token-lifetime-switch");
        _validateActorsSwitch = (IUISwitch)_toolView.GetChildElementById("jwt-decode-validate-token-actors-switch");
        _tokenInput = (IUIMultiLineTextInput)_toolView.GetChildElementById("jwt-decode-token-input");
        _headerInput = (IUIMultiLineTextInput)_toolView.GetChildElementById("jwt-decode-header-input");
        _payloadInput = (IUIMultiLineTextInput)_toolView.GetChildElementById("jwt-decode-payload-input");
        _signatureInput = (IUIMultiLineTextInput)_toolView.GetChildElementById("jwt-decode-signature-input");
        _publicKeyInput = (IUIMultiLineTextInput)_toolView.GetChildElementById("jwt-decode-public-key-input");
        _validateIssuersInput = (IUISingleLineTextInput)_toolView.GetChildElementById("jwt-decode-validate-token-issuers-input");
        _validateAudiencesInput = (IUISingleLineTextInput)_toolView.GetChildElementById("jwt-decode-validate-token-audiences-input");

        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Token should return false")]
    public async Task DecodeTokenWithInvalidTokenShouldReturnError()
    {
        _toolMode.Off();
        _tokenInput.Text("eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ");
        await _decodeTool.WorkTask;
        _payloadInput.Text.Should().BeEmpty();
        _headerInput.Text.Should().BeEmpty();
        _infoBar.Description.Should().StartWith("IDX14100: JWT is not well formed, there are no dots (.).");
        _infoBar.Severity.Should().Be(UIInfoBarSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Issuers should return error")]
    public async Task DecodeTokenWithInvalidIssuersShouldReturnError()
    {
        string tokenContent = await GetToken(JsonWebTokenAlgorithm.HS256);
        string signatureContent = await GetSigningKey(JsonWebTokenAlgorithm.HS256, "Signature.txt");

        _toolMode.Off();
        _validateTokenSwitch.On();
        _validateIssuersSwitch.On();
        _tokenInput.Text(tokenContent);
        _signatureInput.Text(signatureContent);
        await _decodeTool.WorkTask;
        _payloadInput.Text.Should().BeEmpty();
        _headerInput.Text.Should().BeEmpty();
        _infoBar.Description.Should().StartWith("IDX10205: Issuer validation failed.");
        _infoBar.Severity.Should().Be(UIInfoBarSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Audience should return false")]
    public async Task DecodeTokenWithInvalidAudienceShouldReturnError()
    {
        string tokenContent = await GetToken(JsonWebTokenAlgorithm.HS256);
        string signatureContent = await GetSigningKey(JsonWebTokenAlgorithm.HS256, "Signature.txt");

        _toolMode.Off();
        _validateTokenSwitch.On();
        _validateAudiencesSwitch.On();
        _tokenInput.Text(tokenContent);
        _signatureInput.Text(signatureContent);
        await _decodeTool.WorkTask;
        _payloadInput.Text.Should().BeEmpty();
        _headerInput.Text.Should().BeEmpty();
        _infoBar.Description.Should().StartWith("IDX10214: Audience validation failed.");
        _infoBar.Severity.Should().Be(UIInfoBarSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Expired Lifetime should return false")]
    public async Task DecodeTokenWithInvalidExpiredLifetimeShouldReturnError()
    {
        string tokenContent = await GetToken(JsonWebTokenAlgorithm.HS256);
        string signatureContent = await GetSigningKey(JsonWebTokenAlgorithm.HS256, "Signature.txt");

        _toolMode.Off();
        _validateTokenSwitch.On();
        _validateLifetimeSwitch.On();
        _tokenInput.Text(tokenContent);
        _signatureInput.Text(signatureContent);
        await _decodeTool.WorkTask;
        _payloadInput.Text.Should().BeEmpty();
        _headerInput.Text.Should().BeEmpty();
        _infoBar.Description.Should().StartWith("IDX10223: Lifetime validation failed.");
        _infoBar.Severity.Should().Be(UIInfoBarSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Valid HS Token should return decoded payload")]
    public async Task DecodeTokenWithValidHSTokenShouldReturnDecodedPayload()
    {
        string headerContent = await GetHeader(JsonWebTokenAlgorithm.HS256);
        string payloadContent = await GetToolFile("ComplexPayload.json");

        string tokenContent = await GetToken(JsonWebTokenAlgorithm.HS256);
        string signatureContent = await GetSigningKey(JsonWebTokenAlgorithm.HS256, "Signature.txt");

        _toolMode.Off();
        _validateTokenSwitch.On();
        _validateActorsSwitch.On();
        _validateIssuersSwitch.On();
        _validateIssuerSigningKeySwitch.On();
        _validateAudiencesSwitch.On();
        _tokenInput.Text(tokenContent);
        _validateIssuersInput.Text("DevToys");
        _validateAudiencesInput.Text("DevToys");
        _signatureInput.Text(signatureContent);
        await _decodeTool.WorkTask;
        _infoBar.Description.Should().Be(JsonWebTokenEncoderDecoder.ValidToken);
        _infoBar.Severity.Should().Be(UIInfoBarSeverity.Success);
        _payloadInput.Text.Should().Be(payloadContent);
        _headerInput.Text.Should().Be(headerContent);
    }

    [Fact(DisplayName = "Decode Json Web Token with Valid RS Token should return decoded payload")]
    public async Task DecodeTokenWithValidRSTokenShouldReturnDecodedPayload()
    {
        string headerContent = await GetHeader(JsonWebTokenAlgorithm.RS256);
        string payloadContent = await GetToolFile("ComplexPayload.json");

        string tokenContent = await GetToken(JsonWebTokenAlgorithm.RS256);
        string publicKeyContent = await GetSigningKey(JsonWebTokenAlgorithm.RS256, "PublicKey.txt");

        _toolMode.Off();
        _validateTokenSwitch.On();
        _validateActorsSwitch.On();
        _validateIssuersSwitch.On();
        _validateIssuerSigningKeySwitch.On();
        _validateAudiencesSwitch.On();
        _tokenInput.Text(tokenContent);
        _validateIssuersInput.Text("DevToys");
        _validateAudiencesInput.Text("DevToys");
        _publicKeyInput.Text(publicKeyContent);
        await _decodeTool.WorkTask;
        _infoBar.Description.Should().Be(JsonWebTokenEncoderDecoder.ValidToken);
        _infoBar.Severity.Should().Be(UIInfoBarSeverity.Success);
        _payloadInput.Text.Should().Be(payloadContent);
        _headerInput.Text.Should().Be(headerContent);
    }

    [Fact(DisplayName = "Decode Json Web Token with Valid PS Token should return decoded payload")]
    public async Task DecodeTokenWithValidPSTokenShouldReturnDecodedPayload()
    {
        string headerContent = await GetHeader(JsonWebTokenAlgorithm.PS512);
        string payloadContent = await GetToolFile("ComplexPayload.json");

        string tokenContent = await GetToken(JsonWebTokenAlgorithm.PS512);
        string publicKeyContent = await GetSigningKey(JsonWebTokenAlgorithm.PS512, "PublicKey.txt");

        _toolMode.Off();
        _validateTokenSwitch.On();
        _validateActorsSwitch.On();
        _validateIssuersSwitch.On();
        _validateIssuerSigningKeySwitch.On();
        _validateAudiencesSwitch.On();
        _tokenInput.Text(tokenContent);
        _validateIssuersInput.Text("DevToys");
        _validateAudiencesInput.Text("DevToys");
        _publicKeyInput.Text(publicKeyContent);
        await _decodeTool.WorkTask;
        _infoBar.Description.Should().Be(JsonWebTokenEncoderDecoder.ValidToken);
        _infoBar.Severity.Should().Be(UIInfoBarSeverity.Success);
        _payloadInput.Text.Should().Be(payloadContent);
        _headerInput.Text.Should().Be(headerContent);
    }

    [Fact(DisplayName = "Decode Json Web Token with Valid ES Token should return decoded payload")]
    public async Task DecodeTokenWithValidESTokenShouldReturnDecodedPayload()
    {
        string headerContent = await GetHeader(JsonWebTokenAlgorithm.ES512);
        string payloadContent = await GetToolFile("ComplexPayload.json");

        string tokenContent = await GetToken(JsonWebTokenAlgorithm.ES512);
        string publicKeyContent = await GetSigningKey(JsonWebTokenAlgorithm.ES512, "PublicKey.txt");

        _toolMode.Off();
        _validateTokenSwitch.On();
        _validateActorsSwitch.On();
        _validateIssuersSwitch.On();
        _validateIssuerSigningKeySwitch.On();
        _validateAudiencesSwitch.On();
        _tokenInput.Text(tokenContent);
        _validateIssuersInput.Text("DevToys");
        _validateAudiencesInput.Text("DevToys");
        _publicKeyInput.Text(publicKeyContent);
        await _decodeTool.WorkTask;
        _infoBar.Description.Should().Be(JsonWebTokenEncoderDecoder.ValidToken);
        _infoBar.Severity.Should().Be(UIInfoBarSeverity.Success);
        _payloadInput.Text.Should().Be(payloadContent);
        _headerInput.Text.Should().Be(headerContent);
    }
}
