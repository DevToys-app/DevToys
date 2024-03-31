using System.Threading;
using System.Threading.Tasks;
using DevToys.Tools.Helpers;
using DevToys.Tools.Helpers.JsonWebToken;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;
using static DevToys.UnitTests.Tools.Helpers.JsonWebTokenEncoderDecoderDataProvider;

namespace DevToys.UnitTests.Tools.Helpers;

public class JsonWebTokenDecoderHelperTests
{
    private readonly ILogger _logger;

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

        ResultInfo<JsonWebTokenResult> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Public Key should return false")]
    public async Task DecodeTokenWithInvalidPublicKeyShouldReturnError()
    {
        string tokenContent = await GetToken(JsonWebTokenAlgorithm.RS256);

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

        ResultInfo<JsonWebTokenResult> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Signature should return false")]
    public async Task DecodeTokenWithInvalidSignatureShouldReturnError()
    {
        string tokenContent = await GetToken(JsonWebTokenAlgorithm.HS256);

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
        };
        var tokenParameters = new TokenParameters
        {
            Token = tokenContent,
        };

        ResultInfo<JsonWebTokenResult> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Issuers should return false")]
    public async Task DecodeTokenWithInvalidIssuersShouldReturnError()
    {
        string tokenContent = await GetToken(JsonWebTokenAlgorithm.HS256);
        string signatureContent = await GetSharedAlgorithmFile(JsonWebTokenAlgorithm.HS256, "Signature.txt");

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

        ResultInfo<JsonWebTokenResult> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Audiences should return false")]
    public async Task DecodeTokenWithInvalidAudiencesShouldReturnError()
    {
        string tokenContent = await GetToken(JsonWebTokenAlgorithm.HS256);
        string signatureContent = await GetSharedAlgorithmFile(JsonWebTokenAlgorithm.HS256, "Signature.txt");

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

        ResultInfo<JsonWebTokenResult> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact(DisplayName = "Decode Json Web Token with Invalid Expired Lifetime should return false")]
    public async Task DecodeTokenWithInvalidExpiredLifetimeShouldReturnError()
    {
        string tokenContent = await GetToken(JsonWebTokenAlgorithm.HS256);
        string signatureContent = await GetSharedAlgorithmFile(JsonWebTokenAlgorithm.HS256, "Signature.txt");

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

        ResultInfo<JsonWebTokenResult> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Theory(DisplayName = "Decode Asymmetric Algorithms Public Key Json Web Token with Valid Token and valid parameters should return decoded token")]
    [MemberData(nameof(AsymmetricAlgorithms), MemberType = typeof(JsonWebTokenEncoderDecoderDataProvider))]
    internal async Task DecodeAsymmetricAlgorithmsPublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken(JsonWebTokenAlgorithm algorithm)
    {
        string headerContent = await GetHeader(algorithm);
        string payloadContent = await GetToolFile("ComplexPayload.json");
        string tokenContent = await GetToken(algorithm);
        string publicKeyContent = await GetSigningKey(algorithm, "PublicKey.txt");

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
            TokenAlgorithm = algorithm,
            PublicKey = publicKeyContent,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
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

    [Theory(DisplayName = "Decode HS* Json Web Token with Valid Token and valid parameters should return decoded token")]
    [InlineData(JsonWebTokenAlgorithm.HS256)]
    [InlineData(JsonWebTokenAlgorithm.HS384)]
    [InlineData(JsonWebTokenAlgorithm.HS512)]
    internal async Task DecodeHSTokenWithValidTokenAndValidParametersShouldReturnDecodedToken(JsonWebTokenAlgorithm algorithm)
    {
        string headerContent = await GetHeader(algorithm);
        string payloadContent = await GetToolFile("ComplexPayload.json");

        string tokenContent = await GetToken(algorithm);
        string signatureContent = await GetSharedAlgorithmFile(algorithm, "Signature.txt");

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
            TokenAlgorithm = algorithm,
            Signature = signatureContent,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult> result = await JsonWebTokenDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            _logger,
            CancellationToken.None);

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

    private Task<ResultInfo<string>> GetFormattedDataAsync(string rawData) => JsonHelper.FormatAsync(
        rawData,
        Indentation.TwoSpaces,
        false,
        _logger,
        CancellationToken.None);
}
