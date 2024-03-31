using System.Threading.Tasks;
using DevToys.Tools.Helpers.JsonWebToken;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;
using Microsoft.Extensions.Logging;
using static DevToys.UnitTests.Tools.Helpers.JsonWebTokenEncoderDecoderDataProvider;

namespace DevToys.UnitTests.Tools.Helpers;

public class JsonWebTokenEncoderHelperTests
{
    private readonly ILogger _logger;

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

        ResultInfo<JsonWebTokenResult> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact(DisplayName = "Encode Json Web Token with Invalid Signature should return false")]
    public async Task GenerateTokenWithInvalidSignatureShouldReturnError()
    {
        string payloadContent = await GetToolFile("BasicPayload.json");

        var encoderParameters = new EncoderParameters();
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            TokenAlgorithm = JsonWebTokenAlgorithm.HS256
        };

        ResultInfo<JsonWebTokenResult> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.Message.Should().Be(JsonWebTokenEncoderDecoder.InvalidSignature);
    }

    [Fact(DisplayName = "Encode Json Web Token with Invalid Issuers should return false")]
    public async Task GenerateTokenWithInvalidIssuersShouldReturnError()
    {
        string payloadContent = await GetToolFile("BasicPayload.json");
        string signatureContent = await GetSharedAlgorithmFile(JsonWebTokenAlgorithm.HS256, "Signature.txt");

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

        ResultInfo<JsonWebTokenResult> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.Message.Should().Be(JsonWebTokenEncoderDecoder.ValidIssuersEmptyError);
    }

    [Fact(DisplayName = "Encode Json Web Token with Invalid Audience should return false")]
    public async Task GenerateTokenWithInvalidAudiencesShouldReturnError()
    {
        string payloadContent = await GetToolFile("BasicPayload.json");
        string signatureContent = await GetSharedAlgorithmFile(JsonWebTokenAlgorithm.HS256, "Signature.txt");

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

        ResultInfo<JsonWebTokenResult> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.Message.Should().Be(JsonWebTokenEncoderDecoder.ValidAudiencesEmptyError);
    }

    [Fact(DisplayName = "Encode Json Web Token with Invalid Expiration should return false")]
    public async Task GenerateTokenWithInvalidExpirationShouldReturnError()
    {
        string payloadContent = await GetToolFile("BasicPayload.json");
        string signatureContent = await GetSharedAlgorithmFile(JsonWebTokenAlgorithm.HS256, "Signature.txt");

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

        ResultInfo<JsonWebTokenResult> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.Message.Should().Be(JsonWebTokenEncoderDecoder.InvalidExpiration);
    }

    [Theory(DisplayName = "Encode JWT using Asymmetric Algorithms with Valid Payload and Parameters")]
    [MemberData(nameof(AsymmetricAlgorithms), MemberType = typeof(JsonWebTokenEncoderDecoderDataProvider))]
    internal async Task EncodeJWTAsymmetricValidPayloadValidParametersReturnsToken(JsonWebTokenAlgorithm algorithm)
    {
        string payloadContent = await GetToolFile("ComplexPayload.json");
        string privateKeyContent = await GetSigningKey(algorithm, "PrivateKey.txt");
        string expectedResult = await GetToken(algorithm);

        var encoderParameters = new EncoderParameters()
        {
            HasIssuer = false,
            HasAudience = false
        };

        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            PrivateKey = privateKeyContent,
            TokenAlgorithm = algorithm,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());

        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();

        int lastIndexOfDot = result.Data.Token.LastIndexOf('.');
        //exclude the signature,
        result.Data.Token[..lastIndexOfDot].Should().Be(expectedResult[..lastIndexOfDot]);
    }


    [Theory(DisplayName = "Encode HMAC JWT with Valid Payload and Parameters Should Return Valid Token")]
    [InlineData(JsonWebTokenAlgorithm.HS256, false)]
    [InlineData(JsonWebTokenAlgorithm.HS256, true)]
    [InlineData(JsonWebTokenAlgorithm.HS384, false)]
    [InlineData(JsonWebTokenAlgorithm.HS384, true)]
    [InlineData(JsonWebTokenAlgorithm.HS512, false)]
    [InlineData(JsonWebTokenAlgorithm.HS512, true)]
    internal async Task EncodeHMACJWTValidPayloadValidParametersReturnsValidToken(JsonWebTokenAlgorithm algorithm, bool isSignatureInBase64Format)
    {
        string payloadContent = await GetToolFile("ComplexPayload.json");
        string signatureFileName = isSignatureInBase64Format ? "Signature-Base64.txt" : "Signature.txt";
        string signatureContent = await GetSharedAlgorithmFile(JsonWebTokenAlgorithm.HS256, signatureFileName);
        string expectedResult = await GetToken(algorithm);

        var encoderParameters = new EncoderParameters() { HasIssuer = true, HasAudience = true };
        var tokenParameters = new TokenParameters
        {
            Payload = payloadContent,
            Signature = signatureContent,
            IsSignatureInBase64Format = isSignatureInBase64Format,
            TokenAlgorithm = algorithm,
            Issuers = ["DevToys"],
            Audiences = ["DevToys"]
        };

        ResultInfo<JsonWebTokenResult> result = JsonWebTokenEncoderHelper.GenerateToken(
            encoderParameters,
            tokenParameters,
            new MockILogger());

        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().Be(expectedResult);
    }
}

public static class JsonWebTokenEncoderDecoderDataProvider
{
    public const string ToolName = "JsonWebTokenEncoderDecoder";
    public const string BaseAssembly = "DevToys.UnitTests.Tools.TestData";

    internal static Task<string> GetToolFile(string fileName) => TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.{fileName}");

    internal static Task<string> GetSharedAlgorithmFile(JsonWebTokenAlgorithm algorithm, string fileName)
    {
        string algorithmPrefix = algorithm.ToString()[0..2];
        return TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.{algorithmPrefix}.{fileName}");
    }

    internal static Task<string> GetAlgorithmFile(JsonWebTokenAlgorithm algorithm, string fileName)
    {
        string algorithmPrefix = algorithm.ToString()[0..2];
        string algorithmSuffix = algorithm.ToString()[2..];
        return TestDataProvider.GetEmbeddedFileContent($"{BaseAssembly}.{ToolName}.{algorithmPrefix}._{algorithmSuffix}.{fileName}");
    }

    internal static Task<string> GetToken(JsonWebTokenAlgorithm algorithm) => GetAlgorithmFile(algorithm, "Token.txt");

    internal static Task<string> GetHeader(JsonWebTokenAlgorithm algorithm) => GetAlgorithmFile(algorithm, "Header.json");

    internal static bool HasSharedSigningKey(JsonWebTokenAlgorithm algorithm)
    {
        return algorithm is not (JsonWebTokenAlgorithm.ES256 or JsonWebTokenAlgorithm.ES384 or JsonWebTokenAlgorithm.ES512);
    }

    internal static Task<string> GetSigningKey(JsonWebTokenAlgorithm algorithm, string fileName)
    {
        if (HasSharedSigningKey(algorithm))
        {
            return GetSharedAlgorithmFile(algorithm, fileName);
        }

        return GetAlgorithmFile(algorithm, fileName);
    }

    public static IEnumerable<object[]> AsymmetricAlgorithms =>
    [
        [JsonWebTokenAlgorithm.RS256],
        [JsonWebTokenAlgorithm.RS384],
        [JsonWebTokenAlgorithm.RS512],

        [JsonWebTokenAlgorithm.ES256],
        [JsonWebTokenAlgorithm.ES384],
        [JsonWebTokenAlgorithm.ES512],

        [JsonWebTokenAlgorithm.PS256],
        [JsonWebTokenAlgorithm.PS384],
        [JsonWebTokenAlgorithm.PS512],
    ];
}
