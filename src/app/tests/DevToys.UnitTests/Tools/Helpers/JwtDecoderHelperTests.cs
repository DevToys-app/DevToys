using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Tools.Helpers;
using DevToys.Tools.Helpers.Jwt;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.EncodersDecoders.Jwt;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Tools.Helpers;

public class JwtDecoderHelperTests
{
    private readonly ILogger _logger;
    private readonly string _baseTestDataDirectory = Path.Combine("Tools", "TestData", nameof(JwtEncoderDecoder));

    public JwtDecoderHelperTests()
    {
        _logger = new MockILogger();
    }

    [Fact(DisplayName = "Decode Jwt Token with Invalid parameters shoult throw argument exception")]
    public void DecodeTokenWithInvalidParametersShouldThrowArgumentException()
    {
        Func<ValueTask<ResultInfo<JwtTokenResult, ResultInfoSeverity>>> result = ()
            => JwtDecoderHelper.DecodeTokenAsync(null, null, _logger, CancellationToken.None);
        result.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Decode Jwt Token with Invalid Token should return false")]
    public async Task DecodeTokenWithTokenShouldReturnError()
    {
        var decodeParameters = new DecoderParameters();
        var tokenParameters = new TokenParameters()
        {
            Token = "eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ"
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    // TODO add other HS tests

    #region HS

    [Fact(DisplayName = "Decode HS256 Jwt Token with Valid Token, Signature Validation Activated and Invalid Signature should return false")]
    public async Task DecodeHS256TokenWithValidTokenAndSignatureValidationAndInvalidSignatureShouldReturnError()
    {
        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "HS", "BasicToken.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true,
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath)
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.ErrorMessage.Should().Be(JwtEncoderDecoder.SignatureInvalid);
    }

    [Fact(DisplayName = "Decode HS256 Jwt Token with Valid Token, Signature Validation Activated and Invalid Issuers should return false")]
    public async Task DecodeHS256TokenWithValidTokenAndSignatureValidationAndInvalidIssuersShouldReturnError()
    {
        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "HS", "BasicToken.txt");
        string signatureFilePath = Path.Combine(_baseTestDataDirectory, "HS", "Signature.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuer = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            Signature = File.ReadAllText(signatureFilePath)
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.ErrorMessage.Should().Be(JwtEncoderDecoder.ValidIssuersEmptyError);
    }

    [Fact(DisplayName = "Decode HS256 Jwt Token with Valid Token, Signature Validation Activated and Invalid Audiences should return false")]
    public async Task DecodeHS256TokenWithValidTokenAndSignatureValidationAndInvalidAudiencesShouldReturnError()
    {
        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "HS", "BasicToken.txt");
        string signatureFilePath = Path.Combine(_baseTestDataDirectory, "HS", "Signature.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuer = true,
            ValidateAudience = true,
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            Signature = File.ReadAllText(signatureFilePath),
            ValidIssuers = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.ErrorMessage.Should().Be(JwtEncoderDecoder.ValidAudiencesEmptyError);
    }

    [Fact(DisplayName = "Decode HS256 Jwt Token with Valid Token, Signature Validation Activated and Expired Lifetime should return false")]
    public async Task DecodeHS256TokenWithValidTokenAndSignatureValidationAndExpiredLifetimeShouldReturnError()
    {
        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "HS", "ComplexToken.txt");
        string signatureFilePath = Path.Combine(_baseTestDataDirectory, "HS", "Signature.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            Signature = File.ReadAllText(signatureFilePath),
            ValidIssuers = ["devtoys"],
            ValidAudiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact(DisplayName = "Decode HS256 Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeHS256TokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerFilePath = Path.Combine(_baseTestDataDirectory, "HS", "Header.json");
        string payloadFilePath = Path.Combine(_baseTestDataDirectory, "ComplexPayload.json");

        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "HS", "ComplexToken.txt");
        string signatureFilePath = Path.Combine(_baseTestDataDirectory, "HS", "Signature.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            Signature = File.ReadAllText(signatureFilePath),
            ValidIssuers = ["devtoys"],
            ValidAudiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenContent = result.Data;
        tokenContent.Header.Should().NotBeNull();
        tokenContent.Payload.Should().NotBeNull();
        tokenContent.Signature.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(File.ReadAllText(headerFilePath));
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(File.ReadAllText(payloadFilePath));

        tokenContent.Header.Should().Be(formattedHeader.Data);
        tokenContent.Payload.Should().Be(formattedPayload.Data);
    }

    #endregion

    #region RS

    [Fact(DisplayName = "Decode RS256 Public Key Jwt Token with Valid Token, Signature Validation Activated and Invalid Public Key should return false")]
    public async Task DecodeRS256PublicKeyTokenWithValidTokenAndSignatureValidationAndInvalidPublicKeyShouldReturnError()
    {
        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "RS", "RS256-PublicKey-ComplexToken.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.ErrorMessage.Should().Be(JwtEncoderDecoder.PublicKeyInvalid);
    }

    [Fact(DisplayName = "Decode RS256 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeRS256PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerFilePath = Path.Combine(_baseTestDataDirectory, "RS", "RS256-Header.json");
        string payloadFilePath = Path.Combine(_baseTestDataDirectory, "ComplexPayload.json");

        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "RS", "RS256-PublicKey-ComplexToken.txt");
        string publicKeyFilePath = Path.Combine(_baseTestDataDirectory, "RS", "RS256-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            PublicKey = File.ReadAllText(publicKeyFilePath),
            ValidIssuers = ["devtoys"],
            ValidAudiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenContent = result.Data;
        tokenContent.Header.Should().NotBeNull();
        tokenContent.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(File.ReadAllText(headerFilePath));
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(File.ReadAllText(payloadFilePath));

        tokenContent.Header.Should().Be(formattedHeader.Data);
        tokenContent.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode RS256 RSA Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeRS256RsaPublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerFilePath = Path.Combine(_baseTestDataDirectory, "RS", "RS256-Header.json");
        string payloadFilePath = Path.Combine(_baseTestDataDirectory, "ComplexPayload.json");

        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "RS", "RS256-RsaPublicKey-ComplexToken.txt");
        string publicKeyFilePath = Path.Combine(_baseTestDataDirectory, "RS", "RS256-RsaPublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            PublicKey = File.ReadAllText(publicKeyFilePath),
            ValidIssuers = ["devtoys"],
            ValidAudiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenContent = result.Data;
        tokenContent.Header.Should().NotBeNull();
        tokenContent.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(File.ReadAllText(headerFilePath));
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(File.ReadAllText(payloadFilePath));

        tokenContent.Header.Should().Be(formattedHeader.Data);
        tokenContent.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode RS384 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeRS384PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerFilePath = Path.Combine(_baseTestDataDirectory, "RS", "RS384-Header.json");
        string payloadFilePath = Path.Combine(_baseTestDataDirectory, "ComplexPayload.json");

        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "RS", "RS384-PublicKey-ComplexToken.txt");
        string publicKeyFilePath = Path.Combine(_baseTestDataDirectory, "RS", "RS384-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            PublicKey = File.ReadAllText(publicKeyFilePath),
            ValidIssuers = ["devtoys"],
            ValidAudiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenContent = result.Data;
        tokenContent.Header.Should().NotBeNull();
        tokenContent.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(File.ReadAllText(headerFilePath));
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(File.ReadAllText(payloadFilePath));

        tokenContent.Header.Should().Be(formattedHeader.Data);
        tokenContent.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode RS512 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeRS512PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerFilePath = Path.Combine(_baseTestDataDirectory, "RS", "RS512-Header.json");
        string payloadFilePath = Path.Combine(_baseTestDataDirectory, "ComplexPayload.json");

        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "RS", "RS512-PublicKey-ComplexToken.txt");
        string publicKeyFilePath = Path.Combine(_baseTestDataDirectory, "RS", "RS512-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            PublicKey = File.ReadAllText(publicKeyFilePath),
            ValidIssuers = ["devtoys"],
            ValidAudiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenContent = result.Data;
        tokenContent.Header.Should().NotBeNull();
        tokenContent.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(File.ReadAllText(headerFilePath));
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(File.ReadAllText(payloadFilePath));

        tokenContent.Header.Should().Be(formattedHeader.Data);
        tokenContent.Payload.Should().Be(formattedPayload.Data);
    }

    #endregion

    #region PS

    [Fact(DisplayName = "Decode PS256 Public Key Jwt Token with Valid Token, Signature Validation Activated and Invalid Public Key should return false")]
    public async Task DecodePS256PublicKeyTokenWithValidTokenAndSignatureValidationAndInvalidPublicKeyShouldReturnError()
    {
        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "PS", "PS256-PublicKey-ComplexToken.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.ErrorMessage.Should().Be(JwtEncoderDecoder.PublicKeyInvalid);
    }

    [Fact(DisplayName = "Decode PS256 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodePS256PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerFilePath = Path.Combine(_baseTestDataDirectory, "PS", "PS256-Header.json");
        string payloadFilePath = Path.Combine(_baseTestDataDirectory, "ComplexPayload.json");

        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "PS", "PS256-PublicKey-ComplexToken.txt");
        string publicKeyFilePath = Path.Combine(_baseTestDataDirectory, "PS", "PS256-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            PublicKey = File.ReadAllText(publicKeyFilePath),
            ValidIssuers = ["devtoys"],
            ValidAudiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenContent = result.Data;
        tokenContent.Header.Should().NotBeNull();
        tokenContent.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(File.ReadAllText(headerFilePath));
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(File.ReadAllText(payloadFilePath));

        tokenContent.Header.Should().Be(formattedHeader.Data);
        tokenContent.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode PS384 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodePS384PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerFilePath = Path.Combine(_baseTestDataDirectory, "PS", "PS384-Header.json");
        string payloadFilePath = Path.Combine(_baseTestDataDirectory, "ComplexPayload.json");

        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "PS", "PS384-ComplexToken.txt");
        string publicKeyFilePath = Path.Combine(_baseTestDataDirectory, "PS", "PS384-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            PublicKey = File.ReadAllText(publicKeyFilePath),
            ValidIssuers = ["devtoys"],
            ValidAudiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenContent = result.Data;
        tokenContent.Header.Should().NotBeNull();
        tokenContent.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(File.ReadAllText(headerFilePath));
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(File.ReadAllText(payloadFilePath));

        tokenContent.Header.Should().Be(formattedHeader.Data);
        tokenContent.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode PS512 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodePS512PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerFilePath = Path.Combine(_baseTestDataDirectory, "PS", "PS512-Header.json");
        string payloadFilePath = Path.Combine(_baseTestDataDirectory, "ComplexPayload.json");

        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "PS", "PS512-ComplexToken.txt");
        string publicKeyFilePath = Path.Combine(_baseTestDataDirectory, "PS", "PS512-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            PublicKey = File.ReadAllText(publicKeyFilePath),
            ValidIssuers = ["devtoys"],
            ValidAudiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenContent = result.Data;
        tokenContent.Header.Should().NotBeNull();
        tokenContent.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(File.ReadAllText(headerFilePath));
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(File.ReadAllText(payloadFilePath));

        tokenContent.Header.Should().Be(formattedHeader.Data);
        tokenContent.Payload.Should().Be(formattedPayload.Data);
    }

    #endregion

    #region ES

    [Fact(DisplayName = "Decode ES256 Public Key Jwt Token with Valid Token, Signature Validation Activated and Invalid Public Key should return false")]
    public async Task DecodeES256PublicKeyTokenWithValidTokenAndSignatureValidationAndInvalidPublicKeyShouldReturnError()
    {
        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "ES", "ES256-ComplexToken.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Error);
        result.ErrorMessage.Should().Be(JwtEncoderDecoder.PublicKeyInvalid);
    }

    [Fact(DisplayName = "Decode ES256 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeES256PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerFilePath = Path.Combine(_baseTestDataDirectory, "ES", "ES256-Header.json");
        string payloadFilePath = Path.Combine(_baseTestDataDirectory, "ComplexPayload.json");

        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "ES", "ES256-ComplexToken.txt");
        string publicKeyFilePath = Path.Combine(_baseTestDataDirectory, "ES", "ES256-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            PublicKey = File.ReadAllText(publicKeyFilePath),
            ValidIssuers = ["devtoys"],
            ValidAudiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenContent = result.Data;
        tokenContent.Header.Should().NotBeNull();
        tokenContent.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(File.ReadAllText(headerFilePath));
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(File.ReadAllText(payloadFilePath));

        tokenContent.Header.Should().Be(formattedHeader.Data);
        tokenContent.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode ES384 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeES384PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerFilePath = Path.Combine(_baseTestDataDirectory, "ES", "ES384-Header.json");
        string payloadFilePath = Path.Combine(_baseTestDataDirectory, "ComplexPayload.json");

        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "ES", "ES384-ComplexToken.txt");
        string publicKeyFilePath = Path.Combine(_baseTestDataDirectory, "ES", "ES384-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            PublicKey = File.ReadAllText(publicKeyFilePath),
            ValidIssuers = ["devtoys"],
            ValidAudiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenContent = result.Data;
        tokenContent.Header.Should().NotBeNull();
        tokenContent.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(File.ReadAllText(headerFilePath));
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(File.ReadAllText(payloadFilePath));

        tokenContent.Header.Should().Be(formattedHeader.Data);
        tokenContent.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode ES512 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeES512PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerFilePath = Path.Combine(_baseTestDataDirectory, "ES", "ES512-Header.json");
        string payloadFilePath = Path.Combine(_baseTestDataDirectory, "ComplexPayload.json");

        string tokenFilePath = Path.Combine(_baseTestDataDirectory, "ES", "ES512-ComplexToken.txt");
        string publicKeyFilePath = Path.Combine(_baseTestDataDirectory, "ES", "ES512-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
        var tokenParameters = new TokenParameters()
        {
            Token = File.ReadAllText(tokenFilePath),
            PublicKey = File.ReadAllText(publicKeyFilePath),
            ValidIssuers = ["devtoys"],
            ValidAudiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenContent = result.Data;
        tokenContent.Header.Should().NotBeNull();
        tokenContent.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(File.ReadAllText(headerFilePath));
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(File.ReadAllText(payloadFilePath));

        tokenContent.Header.Should().Be(formattedHeader.Data);
        tokenContent.Payload.Should().Be(formattedPayload.Data);
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
