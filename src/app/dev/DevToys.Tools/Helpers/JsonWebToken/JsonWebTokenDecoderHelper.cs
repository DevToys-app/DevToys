using System.Security.Cryptography;
using System.Text;
using DevToys.Tools.Models;
using DevToys.Tools.Models.JwtDecoderEncoder;
using DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DevToys.Tools.Helpers.JsonWebToken;

using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using YamlDotNet.Core.Tokens;

internal static partial class JsonWebTokenDecoderHelper
{
    private static readonly List<string> _dateFields = new() { "exp", "nbf", "iat", "auth_time", "updated_at" };

    public static ResultInfo<JsonWebTokenAlgorithm?> GetTokenAlgorithm(string token, ILogger logger)
    {
        Guard.IsNotNullOrWhiteSpace(token);
        try
        {
            JsonWebTokenHandler handler = new();
            JsonWebToken jsonWebToken = handler.ReadJsonWebToken(token);
            if (!Enum.TryParse(jsonWebToken.Alg, out JsonWebTokenAlgorithm jwtAlgorithm))
            {
                return new ResultInfo<JsonWebTokenAlgorithm?>(null, false);
            }
            return new ResultInfo<JsonWebTokenAlgorithm?>(jwtAlgorithm, true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid token detected");
            return new ResultInfo<JsonWebTokenAlgorithm?>(null, false);
        }
    }

    public static async ValueTask<ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>> DecodeTokenAsync(
        DecoderParameters decodeParameters,
        TokenParameters tokenParameters,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNull(decodeParameters);
        Guard.IsNotNull(tokenParameters);
        Guard.IsNotNullOrWhiteSpace(tokenParameters.Token);

        var tokenResult = new JsonWebTokenResult();

        try
        {
            IdentityModelEventSource.ShowPII = true;
            JsonWebTokenHandler handler = new();
            JsonWebToken jsonWebToken = handler.ReadJsonWebToken(tokenParameters.Token);

            string decodedHeader = Base64Helper.FromBase64ToText(
                jsonWebToken.EncodedHeader,
                Base64Encoding.Utf8,
                logger,
                cancellationToken);
            ResultInfo<string> headerResult = await JsonHelper.FormatAsync(
                decodedHeader,
                Indentation.TwoSpaces,
                false,
                logger,
                cancellationToken);
            if (!headerResult.HasSucceeded)
            {
                return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(JsonWebTokenEncoderDecoder.InvalidHeader, ResultInfoSeverity.Error);
            }
            tokenResult.Header = headerResult.Data;

            string decodedpayload = Base64Helper.FromBase64ToText(
                jsonWebToken.EncodedPayload,
                Base64Encoding.Utf8,
                logger,
                cancellationToken);
            ResultInfo<string> payloadResult = await JsonHelper.FormatAsync(
                decodedpayload,
                Indentation.TwoSpaces,
                false,
                logger,
                cancellationToken);
            if (!payloadResult.HasSucceeded)
            {
                return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(JsonWebTokenEncoderDecoder.InvalidPayload, ResultInfoSeverity.Error);
            }
            tokenResult.Payload = payloadResult.Data;
            tokenResult.PayloadClaims = ProcessClaims(payloadResult.Data, jsonWebToken.Claims);

            if (decodeParameters.ValidateSignature)
            {
                ResultInfo<JsonWebTokenResult, ResultInfoSeverity> signatureValid = await ValidateTokenSignatureAsync(handler, decodeParameters, tokenParameters, tokenResult);
                if (signatureValid.Severity == ResultInfoSeverity.Error)
                {
                    return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(signatureValid.ErrorMessage!, signatureValid.Severity);
                }
                else if (signatureValid.Severity == ResultInfoSeverity.Warning)
                {
                    return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(tokenResult, signatureValid.ErrorMessage!, signatureValid.Severity);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid token detected");
            return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(ex.Message, ResultInfoSeverity.Error);
        }

        return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(tokenResult, ResultInfoSeverity.Success);
    }

    /// <summary>
    /// Validate the token using the Signing Credentials 
    /// </summary>
    private static async Task<ResultInfo<JsonWebTokenResult, ResultInfoSeverity>> ValidateTokenSignatureAsync(
        JsonWebTokenHandler handler,
        DecoderParameters decodeParameters,
        TokenParameters tokenParameters,
        JsonWebTokenResult tokenResult)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateActor = decodeParameters.ValidateActors,
            ValidateLifetime = decodeParameters.ValidateLifetime,
            ValidateIssuer = decodeParameters.ValidateIssuers,
            ValidateAudience = decodeParameters.ValidateAudiences
        };

        if (decodeParameters.ValidateIssuersSigningKey)
        {
            ResultInfo<SigningCredentials> signingCredentials = GetSigningCredentials(tokenParameters);
            if (!signingCredentials.HasSucceeded)
            {
                return new ResultInfo<JsonWebTokenResult, ResultInfoSeverity>(signingCredentials.ErrorMessage!, ResultInfoSeverity.Error);
            }

            validationParameters.ValidateIssuerSigningKey = decodeParameters.ValidateIssuersSigningKey;
            validationParameters.IssuerSigningKey = signingCredentials.Data!.Key;
            validationParameters.TryAllIssuerSigningKeys = true;
        }
        else
        {
            // Create a custom signature validator that does nothing so it always passes in this mode
            validationParameters.SignatureValidator = (token, _) => new JsonWebToken(token);
        }

        // check if the token issuers are part of the user provided issuers
        if (decodeParameters.ValidateIssuers)
        {
            if (tokenParameters.Issuers.Count == 0)
            {
                return new ResultInfo<JsonWebTokenResult, ResultInfoSeverity>(JsonWebTokenEncoderDecoder.ValidIssuersEmptyError, ResultInfoSeverity.Error);
            }
            validationParameters.ValidIssuers = tokenParameters.Issuers;
        }

        // check if the token audience are part of the user provided audiences
        if (decodeParameters.ValidateAudiences)
        {
            if (tokenParameters.Audiences.Count == 0)
            {
                return new ResultInfo<JsonWebTokenResult, ResultInfoSeverity>(JsonWebTokenEncoderDecoder.ValidAudiencesEmptyError, ResultInfoSeverity.Error);
            }
            validationParameters.ValidAudiences = tokenParameters.Audiences;
        }

        try
        {
            TokenValidationResult validationResult = await handler.ValidateTokenAsync(tokenParameters.Token, validationParameters);
            if (!validationResult.IsValid)
            {
                return new ResultInfo<JsonWebTokenResult, ResultInfoSeverity>(validationResult.Exception.Message, ResultInfoSeverity.Error);
            }
            tokenResult.Signature = tokenParameters.Signature;
            tokenResult.PublicKey = tokenParameters.PublicKey;

            if (!decodeParameters.ValidateActors && !decodeParameters.ValidateLifetime &&
                !decodeParameters.ValidateIssuers && !decodeParameters.ValidateAudiences &&
                !decodeParameters.ValidateIssuersSigningKey)
            {
                return new ResultInfo<JsonWebTokenResult, ResultInfoSeverity>(tokenResult, JsonWebTokenEncoderDecoder.TokenNotValidated, ResultInfoSeverity.Warning);
            }
            return new ResultInfo<JsonWebTokenResult, ResultInfoSeverity>(tokenResult, ResultInfoSeverity.Success);
        }
        catch (Exception exception)
        {
            return new ResultInfo<JsonWebTokenResult, ResultInfoSeverity>(exception.Message, ResultInfoSeverity.Error);
        }
    }

    /// <summary>
    /// Get the Signing Credentials depending on the token Algorithm
    /// </summary>
    private static ResultInfo<SigningCredentials> GetSigningCredentials(
        TokenParameters tokenParameters)
    {
        return tokenParameters.TokenAlgorithm switch
        {
            JsonWebTokenAlgorithm.HS256 or
            JsonWebTokenAlgorithm.HS384 or
            JsonWebTokenAlgorithm.HS512 => GetHmacShaSigningCredentials(tokenParameters.Signature, tokenParameters.TokenAlgorithm),
            JsonWebTokenAlgorithm.RS256 or
            JsonWebTokenAlgorithm.RS384 or
            JsonWebTokenAlgorithm.RS512 or
            JsonWebTokenAlgorithm.PS256 or
            JsonWebTokenAlgorithm.PS384 or
            JsonWebTokenAlgorithm.PS512 => GetRsaShaSigningCredentials(tokenParameters.PublicKey, tokenParameters.TokenAlgorithm),
            JsonWebTokenAlgorithm.ES256 or
            JsonWebTokenAlgorithm.ES384 or
            JsonWebTokenAlgorithm.ES512 => GetECDsaSigningCredentials(tokenParameters.PublicKey, tokenParameters.TokenAlgorithm),
            _ => throw new NotSupportedException()
        };
    }

    /// <summary>
    /// Generate a Symmetric Security Key using the token signature (base 64 or plain text)
    /// </summary>
    /// <param name="signature">Token signature</param>
    /// <param name="jwtAlgorithm">
    ///     Supported Algorithm
    ///         HS256, 
    ///         HS384,
    ///         HS512
    /// </param>
    /// <exception cref="NotSupportedException"></exception>
    private static ResultInfo<SigningCredentials> GetHmacShaSigningCredentials(
        string? signature,
        JsonWebTokenAlgorithm jwtAlgorithm)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return new ResultInfo<SigningCredentials>(null!, JsonWebTokenEncoderDecoder.InvalidSignature, false);
        }

        byte[]? signatureByte;
        if (Base64Helper.IsBase64DataStrict(signature))
        {
            signatureByte = Convert.FromBase64String(signature);
        }
        else
        {
            signatureByte = Encoding.UTF8.GetBytes(signature);
        }

        SigningCredentials signingCredentials;
        switch (jwtAlgorithm)
        {
            case JsonWebTokenAlgorithm.HS256:
                byte[] hs256Key = new HMACSHA256(signatureByte).Key;
                var hs256SymmetricSecurityKey = new SymmetricSecurityKey(hs256Key);
                signingCredentials = new SigningCredentials(hs256SymmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
                break;
            case JsonWebTokenAlgorithm.HS384:
                byte[] hs384Key = new HMACSHA384(signatureByte).Key;
                var hs384SymmetricSecurityKey = new SymmetricSecurityKey(hs384Key);
                signingCredentials = new SigningCredentials(hs384SymmetricSecurityKey, SecurityAlgorithms.HmacSha384Signature);
                break;
            case JsonWebTokenAlgorithm.HS512:
                byte[] hs512Key = new HMACSHA512(signatureByte).Key;
                var hs512SymmetricSecurityKey = new SymmetricSecurityKey(hs512Key);
                signingCredentials = new SigningCredentials(hs512SymmetricSecurityKey, SecurityAlgorithms.HmacSha512Signature);
                break;
            default:
                throw new NotSupportedException();
        }
        return new ResultInfo<SigningCredentials>(signingCredentials);
    }

    /// <summary>
    /// Build RSA signing credentials using the token public key
    /// </summary>
    /// <param name="key">Token public key</param>
    /// <param name="jwtAlgorithm">
    ///     Supported Algorithm 
    ///         RS256, 
    ///         RS384,
    ///         RS512,
    ///         PS256, 
    ///         PS384,
    ///         PS512
    /// </param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private static ResultInfo<SigningCredentials> GetRsaShaSigningCredentials(
        string? key,
        JsonWebTokenAlgorithm jwtAlgorithm)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return new ResultInfo<SigningCredentials>(null!, JsonWebTokenEncoderDecoder.InvalidPublicKey, false);
        }

        var rsa = RSA.Create();
        if (key.StartsWith(JsonWebTokenPemEnumeration.PublicKey.PemStart))
        {
            byte[] keyBytes = JsonWebTokenPemEnumeration.GetBytes(JsonWebTokenPemEnumeration.PublicKey, key);
            rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
        }
        else if (key.StartsWith(JsonWebTokenPemEnumeration.RsaPublicKey.PemStart))
        {
            byte[] keyBytes = JsonWebTokenPemEnumeration.GetBytes(JsonWebTokenPemEnumeration.RsaPublicKey, key);
            rsa.ImportRSAPublicKey(keyBytes, out _);
        }
        else
        {
            return new ResultInfo<SigningCredentials>(null!, JsonWebTokenEncoderDecoder.PublicKeyNotSupported, false);
        }

        SigningCredentials signingCredentials;
        switch (jwtAlgorithm)
        {
            case JsonWebTokenAlgorithm.RS256:
                var rs256RsaSecurityKey = new RsaSecurityKey(rsa);
                signingCredentials = new SigningCredentials(rs256RsaSecurityKey, SecurityAlgorithms.RsaSha256Signature);
                break;
            case JsonWebTokenAlgorithm.RS384:
                var rs384SymmetricSecurityKey = new RsaSecurityKey(rsa);
                signingCredentials = new SigningCredentials(rs384SymmetricSecurityKey, SecurityAlgorithms.RsaSha384Signature);
                break;
            case JsonWebTokenAlgorithm.RS512:
                var rs512SymmetricSecurityKey = new RsaSecurityKey(rsa);
                signingCredentials = new SigningCredentials(rs512SymmetricSecurityKey, SecurityAlgorithms.RsaSha512Signature);
                break;
            case JsonWebTokenAlgorithm.PS256:
                var ps256RsaSecurityKey = new RsaSecurityKey(rsa);
                signingCredentials = new SigningCredentials(ps256RsaSecurityKey, SecurityAlgorithms.RsaSsaPssSha256Signature);
                break;
            case JsonWebTokenAlgorithm.PS384:
                var ps384SymmetricSecurityKey = new RsaSecurityKey(rsa);
                signingCredentials = new SigningCredentials(ps384SymmetricSecurityKey, SecurityAlgorithms.RsaSsaPssSha384Signature);
                break;
            case JsonWebTokenAlgorithm.PS512:
                var ps512SymmetricSecurityKey = new RsaSecurityKey(rsa);
                signingCredentials = new SigningCredentials(ps512SymmetricSecurityKey, SecurityAlgorithms.RsaSsaPssSha512Signature);
                break;
            default:
                throw new NotSupportedException();
        }
        return new ResultInfo<SigningCredentials>(signingCredentials);
    }

    /// <summary>
    /// Build ECDsa signing credentials using the token public key
    /// </summary>
    /// <param name="key">Token public key</param>
    /// <param name="jwtAlgorithm">
    ///     Supported Algorithm 
    ///         ES256, 
    ///         ES384,
    ///         ES512
    /// </param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private static ResultInfo<SigningCredentials> GetECDsaSigningCredentials(
        string? key,
        JsonWebTokenAlgorithm jwtAlgorithm)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return new ResultInfo<SigningCredentials>(null!, JsonWebTokenEncoderDecoder.InvalidPublicKey, false);
        }

        ECDsa ecd;
        if (OperatingSystem.IsWindows())
        {
            ecd = ECDsaCng.Create();
        }
        else
        {
            ecd = ECDsaOpenSsl.Create();
        }

        if (key.StartsWith(JsonWebTokenPemEnumeration.PublicKey.PemStart))
        {
            byte[] keyBytes = JsonWebTokenPemEnumeration.GetBytes(JsonWebTokenPemEnumeration.PublicKey, key);
            ecd.ImportSubjectPublicKeyInfo(keyBytes, out _);
        }
        else if (key.StartsWith(JsonWebTokenPemEnumeration.ECDPublicKey.PemStart))
        {
            byte[] keyBytes = JsonWebTokenPemEnumeration.GetBytes(JsonWebTokenPemEnumeration.ECDPublicKey, key);
            ecd.ImportECPrivateKey(keyBytes, out _);
        }
        else
        {
            return new ResultInfo<SigningCredentials>(null!, JsonWebTokenEncoderDecoder.PublicKeyNotSupported, false);
        }

        SigningCredentials signingCredentials;
        switch (jwtAlgorithm)
        {
            case JsonWebTokenAlgorithm.ES256:
                var es256RsaSecurityKey = new ECDsaSecurityKey(ecd);
                signingCredentials = new SigningCredentials(es256RsaSecurityKey, SecurityAlgorithms.EcdsaSha256Signature);
                break;
            case JsonWebTokenAlgorithm.ES384:
                var es384SymmetricSecurityKey = new ECDsaSecurityKey(ecd);
                signingCredentials = new SigningCredentials(es384SymmetricSecurityKey, SecurityAlgorithms.EcdsaSha384Signature);
                break;
            case JsonWebTokenAlgorithm.ES512:
                var es512SymmetricSecurityKey = new ECDsaSecurityKey(ecd);
                signingCredentials = new SigningCredentials(es512SymmetricSecurityKey, SecurityAlgorithms.EcdsaSha512Signature);
                break;
            default:
                throw new NotSupportedException();
        }
        return new ResultInfo<SigningCredentials>(signingCredentials);
    }

    private static List<JsonWebTokenClaim> ProcessClaims(ReadOnlySpan<char> data, IEnumerable<Claim> claims)
    {
        List<JsonWebTokenClaim> processedClaims = new();

        foreach (Claim claim in claims)
        {
            int claimStartPosition = data.IndexOf(claim.Type);
            TextSpan span = new(claimStartPosition, claim.Type.Length);
            JsonWebTokenClaim processedClaim = new(claim.Type, claim.Value, span);
            if (_dateFields.Contains(claim.Type) && long.TryParse(claim.Value, out long value))
            {
                processedClaim.Value = $"{DateTimeOffset.FromUnixTimeSeconds(value).ToLocalTime()} ({claim.Value})";
            }
            processedClaims.Add(processedClaim);
        }

        return processedClaims;
    }
}
