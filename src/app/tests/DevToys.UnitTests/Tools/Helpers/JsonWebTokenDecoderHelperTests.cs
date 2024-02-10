using System.Threading;
using System.Threading.Tasks;
using DevToys.Tools.Helpers;
using DevToys.Tools.Helpers.JsonWebToken;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Tools.Helpers;

public class JsonWebTokenDecoderHelperTests
{
    private readonly ILogger _logger;
    private const string ToolName = "JsonWebTokenEncoderDecoder";
    private const string BaseAssembly = "DevToys.UnitTests.Tools.TestData";

    public JsonWebTokenDecoderHelperTests()
    {
        _logger = new MockILogger();
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid parameters should throw argument exception")]
    public async Task DecodeTokenWithInvalidParametersShouldThrowArgumentException()
    {
        Func<Task> result = () => JsonWebTokenDecoderHelper.DecodeTokenAsync(null, null, _logger, CancellationToken.None).AsTask();

        await result.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Token should return false")]
    public async Task DecodeTokenWithInvalidTokenShouldReturnError()
    {
        var decodeParameters = new DecoderParameters();
        var tokenParameters = new TokenParameters
        {
            Token = "eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ"
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Public Key should return false")]
    public async Task DecodeTokenWithInvalidPublicKeyShouldReturnError()
    {
        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-BasicToken.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.RS256
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Signature should return false")]
    public async Task DecodeTokenWithInvalidSignatureShouldReturnError()
    {
        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-BasicToken.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Issuers should return false")]
    public async Task DecodeTokenWithInvalidIssuersShouldReturnError()
    {
        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-BasicToken.txt");
        string signatureContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuers = true
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            Signature = signatureContent
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Audiences should return false")]
    public async Task DecodeTokenWithInvalidAudiencesShouldReturnError()
    {
        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-BasicToken.txt");
        string signatureContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            Signature = signatureContent
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Expired Lifetime should return false")]
    public async Task DecodeTokenWithInvalidExpiredLifetimeShouldReturnError()
    {
        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-BasicToken.txt");
        string signatureContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateLifetime = true
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            Signature = signatureContent
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    // TODO add other HS tests

    #region HS

    [Fact(DisplayName = "Decode HS256 Json Web Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeHS256TokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Header.json");
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-ComplexToken.txt");
        string signatureContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.HS256,
            Signature = signatureContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JsonWebTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();
        tokenResult.Signature.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    #endregion

    #region RS

    [Fact(DisplayName = "Decode RS256 Public Key Json Web Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeRS256PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS256-Header.json");
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS256-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS256-PublicKey.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.RS256,
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JsonWebTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode RS384 Public Key Json Web Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeRS384PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS384-Header.json");
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS384-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS384-PublicKey.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.RS384,
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JsonWebTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode RS512 Public Key Json Web Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeRS512PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS512-Header.json");
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS512-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS512-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.RS512,
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JsonWebTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    #endregion

    #region PS

    [Fact(DisplayName = "Decode PS256 Public Key Json Web Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodePS256PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS256-Header.json");
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS256-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS256-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.PS256,
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JsonWebTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode PS384 Public Key Json Web Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodePS384PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS384-Header.json");
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS384-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS384-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.PS384,
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JsonWebTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode PS512 Public Key Json Web Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodePS512PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS512-Header.json");
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS512-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS512-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.PS512,
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JsonWebTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    #endregion

    #region ES

    [Fact(DisplayName = "Decode ES256 Public Key Json Web Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeES256PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES256-Header.json");
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES256-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES256-PublicKey.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.ES256,
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JsonWebTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode ES384 Public Key Json Web Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeES384PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES384-Header.json");
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES384-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES384-PublicKey.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.ES384,
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JsonWebTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode ES512 Public Key Json Web Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeES512PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES512-Header.json");
        string payloadContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES512-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES512-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.ES512,
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JsonWebTokenResult, ResultInfoSeverity> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JsonWebTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    #endregion

    #region GetTokenAlgorithm

    [Fact(DisplayName = "Get Json Web Token Algorithm with invalid parameters should throw argument exception")]
    public void GetTokenAlgorithmWithInvalidParametersShouldThrowArgumentException()
    {
        Func<ResultInfo<JsonWebTokenAlgorithm?>> result = () => JsonWebTokenDecoderHelper.GetTokenAlgorithm(null, _logger);
        result.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Get Json Web Token Algorithm with invalid token should return error")]
    public void GetTokenAlgorithmWithInvalidTokenShouldReturnError()
    {
        string tokenContent = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwib2JqZWN0Ijp7Ik9iamVjdF";

        ResultInfo<JsonWebTokenAlgorithm?> result = JsonWebTokenDecoderHelper.GetTokenAlgorithm(tokenContent, _logger);
        result.HasSucceeded.Should().BeFalse();
    }

    [Fact(DisplayName = "Get Json Web Token Algorithm with HS256 token should return HS256")]
    public async Task GetTokenAlgorithmWithInvalidTokenShouldJwtAlgorithmHS256()
    {
        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-BasicToken.txt");

        ResultInfo<JsonWebTokenAlgorithm?> result = JsonWebTokenDecoderHelper.GetTokenAlgorithm(tokenContent, _logger);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(JsonWebTokenAlgorithm.HS256);
    }

    [Fact(DisplayName = "Get Json Web Token Algorithm with PS384 token should return PS384")]
    public async Task GetTokenAlgorithmWithInvalidTokenShouldJwtAlgorithmPS384()
    {
        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.PS.PS384-ComplexToken.txt");

        ResultInfo<JsonWebTokenAlgorithm?> result = JsonWebTokenDecoderHelper.GetTokenAlgorithm(tokenContent, _logger);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(JsonWebTokenAlgorithm.PS384);
    }

    [Fact(DisplayName = "Get Json Web Token Algorithm with RS512 token should return RS512")]
    public async Task GetTokenAlgorithmWithInvalidTokenShouldJwtAlgorithmRS512()
    {
        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.RS.RS512-ComplexToken.txt");

        ResultInfo<JsonWebTokenAlgorithm?> result = JsonWebTokenDecoderHelper.GetTokenAlgorithm(tokenContent, _logger);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(JsonWebTokenAlgorithm.RS512);
    }

    [Fact(DisplayName = "Get Json Web Token Algorithm with ES256 token should return ES256")]
    public async Task GetTokenAlgorithmWithInvalidTokenShouldJwtAlgorithmES256()
    {
        string tokenContent = await TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.ES.ES256-ComplexToken.txt");

        ResultInfo<JsonWebTokenAlgorithm?> result = JsonWebTokenDecoderHelper.GetTokenAlgorithm(tokenContent, _logger);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(JsonWebTokenAlgorithm.ES256);
    }

    #endregion

    private async Task<ResultInfo<string>> GetFormattedDataAsync(string rawData)
        => await JsonHelper.FormatAsync(
                rawData,
                Indentation.TwoSpaces,
                false,
                _logger,
                CancellationToken.None);
}
