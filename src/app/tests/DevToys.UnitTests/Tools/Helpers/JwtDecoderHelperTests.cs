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
    private const string ToolName = "JwtEncoderDecoder";
    private const string BaseAssembly = "DevToys.UnitTests.Tools.TestData";

    public JwtDecoderHelperTests()
    {
        _logger = new MockILogger();
    }

    [Fact(DisplayName = "Decode Jwt Token with invalid parameters should throw argument exception")]
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
        var tokenParameters = new TokenParameters("eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ");

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
        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-BasicToken.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
        };
        var tokenParameters = new TokenParameters(tokenContent);

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
        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-BasicToken.txt");
        string signatureContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuers = true
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            Signature = signatureContent
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
        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-BasicToken.txt");
        string signatureContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuers = true,
            ValidateAudiences = true,
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            Signature = signatureContent,
            Issuers = ["devtoys"]
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
        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-ComplexToken.txt");
        string signatureContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true,
            ValidateLifetime = true
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            Signature = signatureContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
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
        string headerContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Header.json");
        string payloadContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-ComplexToken.txt");
        string signatureContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-Signature.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            Signature = signatureContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenResult = result.Data;
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

    [Fact(DisplayName = "Decode RS256 Public Key Jwt Token with Valid Token, Signature Validation Activated and Invalid Public Key should return false")]
    public async Task DecodeRS256PublicKeyTokenWithValidTokenAndSignatureValidationAndInvalidPublicKeyShouldReturnError()
    {
        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS256-PublicKey-ComplexToken.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true
        };
        var tokenParameters = new TokenParameters(tokenContent);

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
        string headerContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS256-Header.json");
        string payloadContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS256-PublicKey-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS256-PublicKey.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode RS256 RSA Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeRS256RsaPublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS256-Header.json");
        string payloadContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS256-RsaPublicKey-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS256-RsaPublicKey.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode RS384 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeRS384PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS384-Header.json");
        string payloadContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS384-PublicKey-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS384-PublicKey.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode RS512 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeRS512PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS512-Header.json");
        string payloadContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS512-PublicKey-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS512-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    #endregion

    #region PS

    [Fact(DisplayName = "Decode PS256 Public Key Jwt Token with Valid Token, Signature Validation Activated and Invalid Public Key should return false")]
    public async Task DecodePS256PublicKeyTokenWithValidTokenAndSignatureValidationAndInvalidPublicKeyShouldReturnError()
    {
        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.PS.PS256-PublicKey-ComplexToken.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true
        };
        var tokenParameters = new TokenParameters(tokenContent);

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
        string headerContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.PS.PS256-Header.json");
        string payloadContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.PS.PS256-PublicKey-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.PS.PS256-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode PS384 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodePS384PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.PS.PS384-Header.json");
        string payloadContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.PS.PS384-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.PS.PS384-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode PS512 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodePS512PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.PS.PS512-Header.json");
        string payloadContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.PS.PS512-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.PS.PS512-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    #endregion

    #region ES

    [Fact(DisplayName = "Decode ES256 Public Key Jwt Token with Valid Token, Signature Validation Activated and Invalid Public Key should return false")]
    public async Task DecodeES256PublicKeyTokenWithValidTokenAndSignatureValidationAndInvalidPublicKeyShouldReturnError()
    {
        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ES.ES256-ComplexToken.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true
        };
        var tokenParameters = new TokenParameters(tokenContent);

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
        string headerContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ES.ES256-Header.json");
        string payloadContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ES.ES256-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ES.ES256-PublicKey.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode ES384 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeES384PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ES.ES384-Header.json");
        string payloadContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ES.ES384-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ES.ES384-PublicKey.txt");

        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    [Fact(DisplayName = "Decode ES512 Public Key Jwt Token with Valid Token and valid parameters should return decoded token")]
    public async Task DecodeES512PublicKeyTokenWithValidTokenAndValidParametersShouldReturnDecodedToken()
    {
        string headerContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ES.ES512-Header.json");
        string payloadContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ComplexPayload.json");

        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ES.ES512-ComplexToken.txt");
        string publicKeyContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ES.ES512-PublicKey.txt");
        var decodeParameters = new DecoderParameters()
        {
            ValidateSignature = true,
            ValidateIssuersSigningKey = true,
            ValidateIssuers = true,
            ValidateAudiences = true
        };
        var tokenParameters = new TokenParameters(tokenContent)
        {
            PublicKey = publicKeyContent,
            Issuers = ["devtoys"],
            Audiences = ["devtoys"]
        };

        ResultInfo<JwtTokenResult, ResultInfoSeverity> result = await JwtDecoderHelper.DecodeTokenAsync(
            decodeParameters,
            tokenParameters,
            new MockILogger(), CancellationToken.None);
        result.Severity.Should().Be(ResultInfoSeverity.Success);
        result.Data.Should().NotBeNull();
        JwtTokenResult tokenResult = result.Data;
        tokenResult.Header.Should().NotBeNull();
        tokenResult.Payload.Should().NotBeNull();

        ResultInfo<string> formattedHeader = await GetFormattedDataAsync(headerContent);
        ResultInfo<string> formattedPayload = await GetFormattedDataAsync(payloadContent);

        tokenResult.Header.Should().Be(formattedHeader.Data);
        tokenResult.Payload.Should().Be(formattedPayload.Data);
    }

    #endregion

    #region GetTokenAlgorithm

    [Fact(DisplayName = "Get Jwt Token Algorithm with invalid parameters should throw argument exception")]
    public void GetTokenAlgorithmWithInvalidParametersShouldThrowArgumentException()
    {
        Func<ResultInfo<JwtAlgorithm?>> result = () => JwtDecoderHelper.GetTokenAlgorithm(null, _logger);
        result.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Get Jwt Token Algorithm with invalid token should return error")]
    public void GetTokenAlgorithmWithInvalidTokenShouldReturnError()
    {
        string tokenContent = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwib2JqZWN0Ijp7Ik9iamVjdF";

        ResultInfo<JwtAlgorithm?> result = JwtDecoderHelper.GetTokenAlgorithm(tokenContent, _logger);
        result.HasSucceeded.Should().BeFalse();
    }

    [Fact(DisplayName = "Get Jwt Token Algorithm with HS256 token should return HS256")]
    public async Task GetTokenAlgorithmWithInvalidTokenShouldJwtAlgorithmHS256()
    {
        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.HS.HS256-BasicToken.txt");

        ResultInfo<JwtAlgorithm?> result = JwtDecoderHelper.GetTokenAlgorithm(tokenContent, _logger);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(JwtAlgorithm.HS256);
    }

    [Fact(DisplayName = "Get Jwt Token Algorithm with PS384 token should return PS384")]
    public async Task GetTokenAlgorithmWithInvalidTokenShouldJwtAlgorithmPS384()
    {
        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.PS.PS384-ComplexToken.txt");

        ResultInfo<JwtAlgorithm?> result = JwtDecoderHelper.GetTokenAlgorithm(tokenContent, _logger);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(JwtAlgorithm.PS384);
    }

    [Fact(DisplayName = "Get Jwt Token Algorithm with RS512 token should return RS512")]
    public async Task GetTokenAlgorithmWithInvalidTokenShouldJwtAlgorithmRS512()
    {
        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.RS.RS512-PublicKey-ComplexToken.txt");

        ResultInfo<JwtAlgorithm?> result = JwtDecoderHelper.GetTokenAlgorithm(tokenContent, _logger);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(JwtAlgorithm.RS512);
    }

    [Fact(DisplayName = "Get Jwt Token Algorithm with ES256 token should return ES256")]
    public async Task GetTokenAlgorithmWithInvalidTokenShouldJwtAlgorithmES256()
    {
        string tokenContent = await TestDataProvider.GetFileContent($"{BaseAssembly}.{ToolName}.ES.ES256-ComplexToken.txt");

        ResultInfo<JwtAlgorithm?> result = JwtDecoderHelper.GetTokenAlgorithm(tokenContent, _logger);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(JwtAlgorithm.ES256);
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
