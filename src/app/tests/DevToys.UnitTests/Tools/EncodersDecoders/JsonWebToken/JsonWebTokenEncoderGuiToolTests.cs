using System.Globalization;
using System.Threading.Tasks;
using DevToys.Core.Tools.Metadata;
using DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;

namespace DevToys.UnitTests.Tools.EncodersDecoders.JsonWebToken;

public sealed class JsonWebTokenEncoderGuiToolTests : MefBasedTest
{
    private const string ToolName = "JsonWebTokenEncoderDecoder";
    private const string BaseAssembly = "DevToys.UnitTests.Tools.TestData";

    private readonly JsonWebTokenEncoderGuiTool _encodeTool;
    private readonly JsonWebTokenEncoderDecoderGuiTool _tool;
    private readonly UIToolView _toolView;

    private readonly IUISwitch _toolMode;

    private readonly IUIInfoBar _infoBar;
    private readonly IUISelectDropDownList _tokenAlgorithm;
    private readonly IUIMultiLineTextInput _tokenInput;
    private readonly IUIMultiLineTextInput _headerInput;
    private readonly IUIMultiLineTextInput _payloadInput;
    private readonly IUIMultiLineTextInput _signatureInput;
    private readonly IUIMultiLineTextInput _privateKeyInput;

    private readonly IUISwitch _tokenIssuersSwitch;
    private readonly IUISwitch _tokenAudiencesSwitch;
    private readonly IUISwitch _tokenExpirationSwitch;
    private readonly IUISingleLineTextInput _tokenIssuersInput;
    private readonly IUISingleLineTextInput _tokenAudiencesInput;

    public JsonWebTokenEncoderGuiToolTests()
        : base(typeof(JsonWebTokenEncoderGuiTool).Assembly)
    {
        _tool = (JsonWebTokenEncoderDecoderGuiTool)MefProvider.ImportMany<IGuiTool, GuiToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "JsonWebTokenEncoderDecoder")
            .Value;

        _toolView = _tool.View;
        _toolMode = (IUISwitch)_toolView.GetChildElementById("jwt-token-conversion-mode-switch");

        _encodeTool = _tool.EncoderGuiTool;
        _infoBar = (IUIInfoBar)_toolView.GetChildElementById("jwt-encode-info-bar");

        var setting = (IUISetting)_toolView.GetChildElementById("jwt-encode-token-algorithm-setting");
        _tokenAlgorithm = (IUISelectDropDownList)setting.InteractiveElement;
        _tokenInput = (IUIMultiLineTextInput)_toolView.GetChildElementById("jwt-encode-token-input");
        _headerInput = (IUIMultiLineTextInput)_toolView.GetChildElementById("jwt-encode-header-input");
        _payloadInput = (IUIMultiLineTextInput)_toolView.GetChildElementById("jwt-encode-payload-input");
        _signatureInput = (IUIMultiLineTextInput)_toolView.GetChildElementById("jwt-encode-signature-input");
        _privateKeyInput = (IUIMultiLineTextInput)_toolView.GetChildElementById("jwt-encode-private-key-input");

        _tokenIssuersSwitch = (IUISwitch)_toolView.GetChildElementById("jwt-encode-token-issuer-switch");
        _tokenAudiencesSwitch = (IUISwitch)_toolView.GetChildElementById("jwt-encode-token-audience-switch");
        _tokenExpirationSwitch = (IUISwitch)_toolView.GetChildElementById("jwt-encode-token-expiration-switch");
        _tokenIssuersInput = (IUISingleLineTextInput)_toolView.GetChildElementById("jwt-encode-token-issuer-input");
        _tokenAudiencesInput = (IUISingleLineTextInput)_toolView.GetChildElementById("jwt-encode-token-audience-input");

        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Fact(DisplayName = "Encode Json Web Token with Invalid Payload should display error")]
    public async Task EncodeTokenWithInvalidPayloadShouldDisplayError()
    {
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Header.json");
        _toolMode.On();
        _payloadInput.Text("xxx");
        await _encodeTool.WorkTask;
        _tokenInput.Text.Should().BeEmpty();
        _headerInput.Text.Should().Be(headerContent);
        _infoBar.Description.Should().StartWith("'x' is an invalid start of a value.");
        _infoBar.Severity.Should().Be(UIInfoBarSeverity.Error);
    }

    [Fact(DisplayName = "Encode Json Web Token with Invalid Signature should display error")]
    public async Task EncodeTokenWithInvalidSignatureShouldDisplayError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.BasicPayload.json");
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Header.json");

        _toolMode.On();
        _tokenAlgorithm.Select(0); // HS256
        _tokenIssuersSwitch.Off();
        _payloadInput.Text(payloadContent);
        await _encodeTool.WorkTask;
        _tokenInput.Text.Should().BeEmpty();
        _headerInput.Text.Should().Be(headerContent);
        _infoBar.Description.Should().Be(JsonWebTokenEncoderDecoder.InvalidSignature);
        _infoBar.Severity.Should().Be(UIInfoBarSeverity.Error);
    }

    [Fact(DisplayName = "Encode Json Web Token with Invalid Issuers should display error")]
    public async Task EncodeTokenWithInvalidIssuersShouldDisplayError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.BasicPayload.json");
        string signatureContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Header.json");

        _toolMode.On();
        _tokenAlgorithm.Select(0); // HS256
        _tokenIssuersSwitch.On();

        _payloadInput.Text(payloadContent);
        _signatureInput.Text(signatureContent);
        _tokenIssuersInput.Text(string.Empty);
        await _encodeTool.WorkTask;
        _tokenInput.Text.Should().BeEmpty();
        _headerInput.Text.Should().Be(headerContent);
        _infoBar.Description.Should().Be(JsonWebTokenEncoderDecoder.ValidIssuersEmptyError);
        _infoBar.Severity.Should().Be(UIInfoBarSeverity.Error);
    }

    [Fact(DisplayName = "Encode Json Web Token with Invalid Audiences should display error")]
    public async Task EncodeTokenWithInvalidAudiencesShouldDisplayError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.BasicPayload.json");
        string signatureContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Header.json");

        _toolMode.On();
        _tokenAlgorithm.Select(0); // HS256
        _tokenAudiencesSwitch.On();

        _payloadInput.Text(payloadContent);
        _signatureInput.Text(signatureContent);
        _tokenAudiencesInput.Text(string.Empty);
        await _encodeTool.WorkTask;
        _tokenInput.Text.Should().BeEmpty();
        _headerInput.Text.Should().Be(headerContent);
        _infoBar.Description.Should().Be(JsonWebTokenEncoderDecoder.ValidAudiencesEmptyError);
        _infoBar.Severity.Should().Be(UIInfoBarSeverity.Error);
    }

    [Fact(DisplayName = "Encode HS Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task EncodeHSTokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string signatureContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Header.json");

        _toolMode.On();
        _tokenAlgorithm.Select(0); // HS256
        _tokenAudiencesSwitch.On();
        _tokenAudiencesInput.Text("DevToys");
        _tokenIssuersSwitch.On();
        _tokenIssuersInput.Text("DevToys");
        _payloadInput.Text(payloadContent);
        _signatureInput.Text(signatureContent);
        await _encodeTool.WorkTask;
        _tokenInput.Text.Should().NotBeNullOrWhiteSpace();
        _headerInput.Text.Should().Be(headerContent);
    }

    [Fact(DisplayName = "Encode RS Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task EncodeRSTokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string privateKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS256-PrivateKey.txt");
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS256-Header.json");

        _toolMode.On();
        _tokenAlgorithm.Select(3); // RS256
        _tokenAudiencesSwitch.On();
        _tokenAudiencesInput.Text("DevToys");
        _tokenIssuersSwitch.On();
        _tokenIssuersInput.Text("DevToys");
        _payloadInput.Text(payloadContent);
        _privateKeyInput.Text(privateKeyContent);
        await _encodeTool.WorkTask;
        _tokenInput.Text.Should().NotBeNullOrWhiteSpace();
        _headerInput.Text.Should().Be(headerContent);
    }

    [Fact(DisplayName = "Encode PS Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task EncodePSTokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string privateKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS384-PrivateKey.txt");
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS384-Header.json");

        _toolMode.On();
        _tokenAlgorithm.Select(7); // PS384
        _tokenAudiencesSwitch.On();
        _tokenAudiencesInput.Text("DevToys");
        _tokenIssuersSwitch.On();
        _tokenIssuersInput.Text("DevToys");
        _payloadInput.Text(payloadContent);
        _privateKeyInput.Text(privateKeyContent);
        await _encodeTool.WorkTask;
        _tokenInput.Text.Should().NotBeNullOrWhiteSpace();
        _headerInput.Text.Should().Be(headerContent);
    }

    [Fact(DisplayName = "Encode ES Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task EncodeESTokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string privateKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES512-PrivateKey.txt");
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES512-Header.json");

        _toolMode.On();
        _tokenAlgorithm.Select(11); // ES512
        _tokenAudiencesSwitch.On();
        _tokenAudiencesInput.Text("DevToys");
        _tokenIssuersSwitch.On();
        _tokenIssuersInput.Text("DevToys");
        _payloadInput.Text(payloadContent);
        _privateKeyInput.Text(privateKeyContent);
        await _encodeTool.WorkTask;
        _tokenInput.Text.Should().NotBeNullOrWhiteSpace();
        _headerInput.Text.Should().Be(headerContent);
    }
}
