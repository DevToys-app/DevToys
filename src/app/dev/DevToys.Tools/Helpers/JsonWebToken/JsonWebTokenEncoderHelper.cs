using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DevToys.Tools.Helpers.Core;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using static DevToys.Tools.Helpers.JsonWebToken.JsonWebTokenEncoderDecoderHelper;
namespace DevToys.Tools.Helpers.JsonWebToken;

using Microsoft.IdentityModel.JsonWebTokens;

internal static class JsonWebTokenEncoderHelper
{
    private static readonly JsonSerializerOptions options = new()
    {
        Converters =
        {
            new JsonWebTokenPayloadConverter()
        }
    };

    public static ResultInfo<JsonWebTokenResult?, ResultInfoSeverity> GenerateToken(
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
            Dictionary<string, object>? payload = JsonSerializer.Deserialize<Dictionary<string, object>>(tokenParameters.Payload!, options);

            ResultInfo<SigningCredentials> signingCredentials = GetSigningCredentials(tokenParameters, true);

            if (!signingCredentials.HasSucceeded)
            {
                return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(signingCredentials.ErrorMessage!, ResultInfoSeverity.Error);
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
                if (tokenParameters.Issuers.Count == 0)
                {
                    return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(JsonWebTokenEncoderDecoder.ValidIssuersEmptyError, ResultInfoSeverity.Error);
                }

                tokenDescriptor.Issuer = string.Join(',', tokenParameters.Issuers);

                if (string.IsNullOrWhiteSpace(tokenDescriptor.Issuer))
                {
                    return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(JsonWebTokenEncoderDecoder.ValidIssuersEmptyError, ResultInfoSeverity.Error);
                }
            }

            if (encodeParameters.HasAudience)
            {
                if (tokenParameters.Audiences.Count == 0)
                {
                    return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(JsonWebTokenEncoderDecoder.ValidAudiencesEmptyError, ResultInfoSeverity.Error);
                }

                tokenDescriptor.Audience = string.Join(',', tokenParameters.Audiences);
                if (string.IsNullOrWhiteSpace(tokenDescriptor.Audience))
                {
                    return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(JsonWebTokenEncoderDecoder.ValidAudiencesEmptyError, ResultInfoSeverity.Error);
                }
            }

            if (encodeParameters.HasExpiration)
            {
                if (!tokenParameters.ExpirationYear.HasValue || !tokenParameters.ExpirationMonth.HasValue ||
                    !tokenParameters.ExpirationDay.HasValue || !tokenParameters.ExpirationHour.HasValue ||
                    !tokenParameters.ExpirationMinute.HasValue)
                {
                    return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(JsonWebTokenEncoderDecoder.InvalidExpiration, ResultInfoSeverity.Error);
                }
                tokenDescriptor.HandleExpiration(tokenParameters);
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
            return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(ex.Message, ResultInfoSeverity.Error);
        }

        return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(tokenResult, ResultInfoSeverity.Success);
    }

    private static void HandleExpiration(this SecurityTokenDescriptor tokenDescriptor, TokenParameters tokenParameters)
    {
        DateTime expirationDate = DateTime.UtcNow;
        if (tokenParameters.ExpirationYear.HasValue)
        {
            expirationDate.AddYears(tokenParameters.ExpirationYear.Value);
        }
        if (tokenParameters.ExpirationMonth.HasValue)
        {
            expirationDate.AddMonths(tokenParameters.ExpirationMonth.Value);
        }
        if (tokenParameters.ExpirationDay.HasValue)
        {
            expirationDate.AddDays(tokenParameters.ExpirationDay.Value);
        }
        if (tokenParameters.ExpirationHour.HasValue)
        {
            expirationDate.AddHours(tokenParameters.ExpirationHour.Value);
        }
        if (tokenParameters.ExpirationMinute.HasValue)
        {
            expirationDate.AddMinutes(tokenParameters.ExpirationMinute.Value);
        }

        tokenDescriptor.Expires = expirationDate;
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
            return new ResultInfo<SigningCredentials>(null!, JsonWebTokenEncoderDecoder.InvalidSignature, false);
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

        return new ResultInfo<SigningCredentials>(signingCredentials);
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
            return new ResultInfo<SigningCredentials>(null!, JsonWebTokenEncoderDecoder.InvalidPrivateKey, false);
        }

        TAlgorithm ecd = algorithmFactory();

        ecd.ImportFromPem(key);

        var signingCredentials = new SigningCredentials(securityKeyFactory(ecd), jsonWebTokenAlgorithm.ToString());

        return new ResultInfo<SigningCredentials>(signingCredentials);
    }
}

