using System.Threading;
using System.Threading.Tasks;
using DevToys.Tools.Helpers;
using DevToys.Tools.Helpers.JsonWebToken;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Tools.Helpers;

public class JsonWebTokenEncoderHelperTests
{
    private readonly ILogger _logger;
    private const string ToolName = "JsonWebTokenEncoderDecoder";
    private const string BaseAssembly = "DevToys.UnitTests.Tools.TestData";

    public JsonWebTokenEncoderHelperTests()
    {
        _logger = new MockILogger();
    }

    [Fact(DisplayName = "Encode Json Web Token with invalid parameters should throw argument exception")]
    public void GenerateTokenWithInvalidParametersShouldThrowArgumentException()
    {
        Action result = () => JsonWebTokenEncoderHelper.GenerateToken(null, null, _logger);

        result.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Encode Json Web Token with Invalid Payload should return false")]
    public void GenerateTokenWithInvalidPayloadShouldReturnError()
    {
        var encoderParameters = new EncoderParameters();
        var tokenParameters = new TokenParameters
        {
            Payload = "xxx"
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact(DisplayName = "Encode Json Web Token with Invalid Signature should return false")]
    public async Task GenerateTokenWithInvalidSignatureShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.BasicPayload.json");

        var encoderParameters = new EncoderParameters();
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.HS256
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.ErrorMessage.Should().Be(JsonWebTokenEncoderDecoder.InvalidSignature);
    }

    [Fact(DisplayName = "Encode Json Web Token with Invalid Issuers should return false")]
    public async Task GenerateTokenWithInvalidIssuersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.BasicPayload.json");
        string signatureContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");

        var encoderParameters = new EncoderParameters()
        {
            HasIssuer = true
        };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            Signature = signatureContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.HS256
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.ErrorMessage.Should().Be(JsonWebTokenEncoderDecoder.ValidIssuersEmptyError);
    }

    [Fact(DisplayName = "Encode Json Web Token with Invalid Audience should return false")]
    public async Task GenerateTokenWithInvalidAudiencesShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.BasicPayload.json");
        string signatureContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");

        var encoderParameters = new EncoderParameters()
        {
            HasAudience = true
        };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            Signature = signatureContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.HS256
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.ErrorMessage.Should().Be(JsonWebTokenEncoderDecoder.ValidAudiencesEmptyError);
    }

    [Fact(DisplayName = "Encode Json Web Token with Invalid Expiration should return false")]
    public async Task GenerateTokenWithInvalidExpirationShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.BasicPayload.json");
        string signatureContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");

        var encoderParameters = new EncoderParameters()
        {
            HasExpiration = true
        };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            Signature = signatureContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.HS256
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.ErrorMessage.Should().Be(JsonWebTokenEncoderDecoder.InvalidExpiration);
    }

    #region HS

    [Fact(DisplayName = "Encode HS256 Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task DecodeHS256TokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string signatureContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");

        var encoderParameters = new EncoderParameters()
        {
            HasIssuer = true,
            HasAudience = true
        };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            Signature = signatureContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.HS256,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region RS

    [Fact(DisplayName = "Encode RS256 Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task DecodeRS256TokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string privateKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS256-RsaPrivateKey.txt");

        var encoderParameters = new EncoderParameters()
        {
            HasIssuer = true,
            HasAudience = true
        };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            PrivateKey = privateKeyContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.RS256,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Encode RS384 Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task DecodeRS384TokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string privateKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS384-PrivateKey.txt");

        var encoderParameters = new EncoderParameters()
        {
            HasIssuer = true,
            HasAudience = true
        };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            PrivateKey = privateKeyContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.RS384,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Encode RS512 Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task DecodeRS512TokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string privateKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS512-PrivateKey.txt");

        var encoderParameters = new EncoderParameters()
        {
            HasIssuer = true,
            HasAudience = true
        };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            PrivateKey = privateKeyContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.RS512,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region PS

    [Fact(DisplayName = "Encode PS256 Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task DecodePS256TokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string privateKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS256-RsaPrivateKey.txt");

        var encoderParameters = new EncoderParameters()
        {
            HasIssuer = true,
            HasAudience = true
        };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            PrivateKey = privateKeyContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.PS256,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Encode PS384 Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task DecodePS384TokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string privateKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS384-PrivateKey.txt");

        var encoderParameters = new EncoderParameters()
        {
            HasIssuer = true,
            HasAudience = true
        };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            PrivateKey = privateKeyContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.PS384,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Encode PS512 Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task DecodePS512TokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string privateKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS512-PrivateKey.txt");

        var encoderParameters = new EncoderParameters()
        {
            HasIssuer = true,
            HasAudience = true
        };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            PrivateKey = privateKeyContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.PS512,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region ES

    [Fact(DisplayName = "Encode ES256 Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task DecodeES256TokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string privateKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES256-PrivateKey.txt");

        var encoderParameters = new EncoderParameters()
        {
            HasIssuer = true,
            HasAudience = true
        };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            PrivateKey = privateKeyContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.ES256,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Encode ES384 Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task DecodeES384TokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string privateKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES384-PrivateKey.txt");

        var encoderParameters = new EncoderParameters()
        {
            HasIssuer = true,
            HasAudience = true
        };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            PrivateKey = privateKeyContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.ES384,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Encode ES512 Json Web Token with Valid Payload and Valid Parameters should return token")]
    public async Task DecodeES512TokenWithValidTokenAndValidParametersShouldReturnError()
    {
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");
        string privateKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES512-PrivateKey.txt");

        var encoderParameters = new EncoderParameters()
        {
            HasIssuer = true,
            HasAudience = true
        };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            PrivateKey = privateKeyContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.ES512,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrWhiteSpace();
    }

    #endregion
}
