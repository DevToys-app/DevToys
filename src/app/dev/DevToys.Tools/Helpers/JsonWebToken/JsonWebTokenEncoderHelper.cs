using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using static DevToys.Tools.Helpers.JsonWebToken.JsonWebTokenEncoderDecoderHelper;
namespace DevToys.Tools.Helpers.JsonWebToken;

using DevToys.Api;
using Microsoft.IdentityModel.JsonWebTokens;

internal static class JsonWebTokenEncoderHelper
{
    public static ResultInfo<JsonWebTokenResult?> GenerateToken(
        EncoderParameters encodeParameters,
        TokenParameters tokenParameters,
        ILogger logger)
    {
        Guard.IsNotNull(encodeParameters);
        Guard.IsNotNull(tokenParameters);
        Guard.IsNotNullOrWhiteSpace(tokenParameters.Payload);

        JsonWebTokenResult tokenResult = new();

        try
        {
            IdentityModelEventSource.ShowPII = true;
            Dictionary<string, object>? payload = JsonSerializer.Deserialize<Dictionary<string, object>>(tokenParameters.Payload);

            ResultInfo<SigningCredentials> signingCredentials = GetSigningCredentials(tokenParameters, true);

            if (!signingCredentials.HasSucceeded)
            {
                return ResultInfo<JsonWebTokenResult?>.Error(signingCredentials.Message!);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = payload,
                SigningCredentials = signingCredentials.Data,
                IssuedAt = null,
                Expires = null,
            };

            if (encodeParameters.HasIssuer)
            {
                tokenDescriptor.Issuer = string.Join(',', tokenParameters.Issuers);

                if (string.IsNullOrWhiteSpace(tokenDescriptor.Issuer))
                {
                    return ResultInfo<JsonWebTokenResult?>.Error(JsonWebTokenEncoderDecoder.ValidIssuersEmptyError);
                }
            }

            if (encodeParameters.HasAudience)
            {
                tokenDescriptor.Audience = string.Join(',', tokenParameters.Audiences);

                if (string.IsNullOrWhiteSpace(tokenDescriptor.Audience))
                {
                    return ResultInfo<JsonWebTokenResult?>.Error(JsonWebTokenEncoderDecoder.ValidAudiencesEmptyError);
                }
            }

            if (encodeParameters.HasExpiration)
            {
                if (!tokenParameters.ExpirationYear.HasValue
                    || !tokenParameters.ExpirationMonth.HasValue
                    || !tokenParameters.ExpirationDay.HasValue
                    || !tokenParameters.ExpirationHour.HasValue
                    || !tokenParameters.ExpirationMinute.HasValue)
                {
                    return ResultInfo<JsonWebTokenResult?>.Error(JsonWebTokenEncoderDecoder.InvalidExpiration);
                }

                tokenDescriptor.Expires = DateTime.UtcNow
                    .AddYears(tokenParameters.ExpirationYear ?? 0)
                    .AddMonths(tokenParameters.ExpirationMonth ?? 0)
                    .AddDays(tokenParameters.ExpirationDay ?? 0)
                    .AddHours(tokenParameters.ExpirationHour ?? 0)
                    .AddMinutes(tokenParameters.ExpirationMinute ?? 0);
            }

            var handler = new JsonWebTokenHandler
            {
                SetDefaultTimesOnTokenCreation = false
            };

            if (encodeParameters.HasDefaultTime)
            {
                handler.SetDefaultTimesOnTokenCreation = true;
                tokenDescriptor.Expires = DateTime.UtcNow.AddHours(1);
            }
            string token = handler.CreateToken(tokenDescriptor);
            tokenResult.Token = token;
            tokenResult.Payload = tokenParameters.Payload;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid Payload detected");
            return ResultInfo<JsonWebTokenResult?>.Error(ex.Message);
        }

        return tokenResult;
    }
}

internal static class JsonWebTokenEncoderDecoderHelper
{
    /// <summary>
    /// Get the Signing Credentials depending on the token Algorithm
    /// </summary>
    internal static ResultInfo<SigningCredentials> GetSigningCredentials(TokenParameters tokenParameters, bool forEncode) => tokenParameters.TokenAlgorithm switch
    {
        JsonWebTokenAlgorithm.HS256 or
        JsonWebTokenAlgorithm.HS384 or
        JsonWebTokenAlgorithm.HS512 => GetHmacShaSigningCredentials(tokenParameters.Signature, tokenParameters.IsSignatureInBase64Format, tokenParameters.TokenAlgorithm),
        JsonWebTokenAlgorithm.RS256 or
        JsonWebTokenAlgorithm.RS384 or
        JsonWebTokenAlgorithm.RS512 or
        JsonWebTokenAlgorithm.PS256 or
        JsonWebTokenAlgorithm.PS384 or
        JsonWebTokenAlgorithm.PS512 => GetRsaShaSigningCredentials(forEncode ? tokenParameters.PrivateKey : tokenParameters.PublicKey, tokenParameters.TokenAlgorithm),
        JsonWebTokenAlgorithm.ES256 or
        JsonWebTokenAlgorithm.ES384 or
        JsonWebTokenAlgorithm.ES512 => GetECDsaSigningCredentials(forEncode ? tokenParameters.PrivateKey : tokenParameters.PublicKey, tokenParameters.TokenAlgorithm),
        _ => throw new NotSupportedException()
    };

    /// <summary>
    /// Generate a Symmetric Security Key using the token signature (base 64 or plain text)
    /// </summary>
    /// <param name="signature">Token signature</param>
    /// <param name="jsonWebTokenAlgorithm">
    ///     Supported Algorithm
    ///         HS256, 
    ///         HS384,
    ///         HS512
    /// </param>
    /// <exception cref="NotSupportedException"></exception>
    private static ResultInfo<SigningCredentials> GetHmacShaSigningCredentials(
        string? signature,
        bool isSignatureInBase64Format,
        JsonWebTokenAlgorithm jwtAlgorithm)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return ResultInfo<SigningCredentials>.Error(JsonWebTokenEncoderDecoder.InvalidSignature);
        }

        byte[] signatureByte = isSignatureInBase64Format ? Convert.FromBase64String(signature) : Encoding.UTF8.GetBytes(signature);

        (byte[] hashKey, string algorithm) = jwtAlgorithm switch
        {
            JsonWebTokenAlgorithm.HS256 => (new HMACSHA256(signatureByte).Key, SecurityAlgorithms.HmacSha256),
            JsonWebTokenAlgorithm.HS384 => (new HMACSHA384(signatureByte).Key, SecurityAlgorithms.HmacSha384),
            JsonWebTokenAlgorithm.HS512 => (new HMACSHA512(signatureByte).Key, SecurityAlgorithms.HmacSha512),
            _ => throw new NotSupportedException()
        };

        var symmetricSecurityKey = new SymmetricSecurityKey(hashKey);
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, algorithm);

        return signingCredentials;
    }

    private static ResultInfo<SigningCredentials> GetRsaShaSigningCredentials(string? key, JsonWebTokenAlgorithm algorithm)
    {
        Debug.Assert(algorithm.ToString()[..2] is "RS" or "PS");

        return CreateSigningCredentials(key, algorithm, RSA.Create, x => new RsaSecurityKey(x));
    }

    private static ResultInfo<SigningCredentials> GetECDsaSigningCredentials(string? key, JsonWebTokenAlgorithm algorithm)
    {
        Debug.Assert(algorithm.ToString()[..2] is "ES");
        return CreateSigningCredentials(key, algorithm, ECDsa.Create, x => new ECDsaSecurityKey(x));
    }

    private static ResultInfo<SigningCredentials> CreateSigningCredentials<TAlgorithm>(
        string? key,
        JsonWebTokenAlgorithm jsonWebTokenAlgorithm,
        Func<TAlgorithm> algorithmFactory,
        Func<TAlgorithm, AsymmetricSecurityKey> securityKeyFactory) where TAlgorithm : AsymmetricAlgorithm
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return ResultInfo<SigningCredentials>.Error(JsonWebTokenEncoderDecoder.InvalidPrivateKey);
        }

        TAlgorithm ecd = algorithmFactory();

        ecd.ImportFromPem(key);

        var signingCredentials = new SigningCredentials(securityKeyFactory(ecd), jsonWebTokenAlgorithm.ToString());

        return signingCredentials;
    }
}

