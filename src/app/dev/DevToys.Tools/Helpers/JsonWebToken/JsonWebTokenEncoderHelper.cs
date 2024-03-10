using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DevToys.Tools.Helpers.Core;
using DevToys.Tools.Models;
using DevToys.Tools.Models.JwtDecoderEncoder;
using DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DevToys.Tools.Helpers.JsonWebToken;

using Microsoft.IdentityModel.JsonWebTokens;

internal static partial class JsonWebTokenEncoderHelper
{
    private static readonly JsonSerializerOptions options = new()
    {
        Converters = {
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
            if (payload is null)
            {
                //return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(JsonWebTokenEncoderDecoder.ValidIssuersEmptyError, ResultInfoSeverity.Error);
            }

            ResultInfo<SigningCredentials> signingCredentials = GetSigningCredentials(tokenParameters);
            if (!signingCredentials.HasSucceeded)
            {
                return new ResultInfo<JsonWebTokenResult?, ResultInfoSeverity>(signingCredentials.ErrorMessage!, ResultInfoSeverity.Error);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = payload,
                SigningCredentials = signingCredentials.Data,
                IssuedAt = DateTime.UtcNow,
                Expires = null
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
            JsonWebTokenAlgorithm.PS512 => GetRsaShaSigningCredentials(tokenParameters.PrivateKey, tokenParameters.TokenAlgorithm),
            JsonWebTokenAlgorithm.ES256 or
            JsonWebTokenAlgorithm.ES384 or
            JsonWebTokenAlgorithm.ES512 => GetECDsaSigningCredentials(tokenParameters.PrivateKey, tokenParameters.TokenAlgorithm),
            _ => throw new NotSupportedException()
        };
    }

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
        JsonWebTokenAlgorithm jsonWebTokenAlgorithm)
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
        switch (jsonWebTokenAlgorithm)
        {
            case JsonWebTokenAlgorithm.HS256:
                byte[] hs256Key = new HMACSHA256(signatureByte).Key;
                var hs256SymmetricSecurityKey = new SymmetricSecurityKey(hs256Key);
                signingCredentials = new SigningCredentials(hs256SymmetricSecurityKey, SecurityAlgorithms.HmacSha256);
                break;
            case JsonWebTokenAlgorithm.HS384:
                byte[] hs384Key = new HMACSHA384(signatureByte).Key;
                var hs384SymmetricSecurityKey = new SymmetricSecurityKey(hs384Key);
                signingCredentials = new SigningCredentials(hs384SymmetricSecurityKey, SecurityAlgorithms.HmacSha384);
                break;
            case JsonWebTokenAlgorithm.HS512:
                byte[] hs512Key = new HMACSHA512(signatureByte).Key;
                var hs512SymmetricSecurityKey = new SymmetricSecurityKey(hs512Key);
                signingCredentials = new SigningCredentials(hs512SymmetricSecurityKey, SecurityAlgorithms.HmacSha512);
                break;
            default:
                throw new NotSupportedException();
        }
        return new ResultInfo<SigningCredentials>(signingCredentials);
    }

    /// <summary>
    /// Build RSA signing credentials using the token private key
    /// </summary>
    /// <param name="key">Token private key</param>
    /// <param name="jsonWebTokenAlgorithm">
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
        JsonWebTokenAlgorithm jsonWebTokenAlgorithm)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return new ResultInfo<SigningCredentials>(null!, JsonWebTokenEncoderDecoder.InvalidPrivateKey, false);
        }

        var rsa = RSA.Create();
        if (key.StartsWith(JsonWebTokenPemEnumeration.PrivateKey.PemStart))
        {
            byte[] keyBytes = JsonWebTokenPemEnumeration.GetBytes(JsonWebTokenPemEnumeration.PrivateKey, key);
            rsa.ImportPkcs8PrivateKey(keyBytes, out _);
        }
        else if (key.StartsWith(JsonWebTokenPemEnumeration.PrivateKey.PemStart))
        {
            byte[] keyBytes = JsonWebTokenPemEnumeration.GetBytes(JsonWebTokenPemEnumeration.PrivateKey, key);
            rsa.ImportPkcs8PrivateKey(keyBytes, out _);
        }
        else if (key.StartsWith(JsonWebTokenPemEnumeration.RsaPrivateKey.PemStart))
        {
            byte[] keyBytes = JsonWebTokenPemEnumeration.GetBytes(JsonWebTokenPemEnumeration.RsaPrivateKey, key);
            rsa.ImportRSAPrivateKey(keyBytes, out _);
        }
        else
        {
            return new ResultInfo<SigningCredentials>(null!, JsonWebTokenEncoderDecoder.PrivateKeyNotSupported, false);
        }

        SigningCredentials signingCredentials;
        switch (jsonWebTokenAlgorithm)
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
    /// Build ECDsa signing credentials using the token private key
    /// </summary>
    /// <param name="key">Token public key</param>
    /// <param name="jsonWebTokenAlgorithm">
    ///     Supported Algorithm 
    ///         ES256, 
    ///         ES384,
    ///         ES512
    /// </param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private static ResultInfo<SigningCredentials> GetECDsaSigningCredentials(
        string? key,
        JsonWebTokenAlgorithm jsonWebTokenAlgorithm)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return new ResultInfo<SigningCredentials>(null!, JsonWebTokenEncoderDecoder.InvalidPrivateKey, false);
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

        if (key.StartsWith(JsonWebTokenPemEnumeration.PrivateKey.PemStart))
        {
            byte[] keyBytes = JsonWebTokenPemEnumeration.GetBytes(JsonWebTokenPemEnumeration.PrivateKey, key);
            ecd.ImportPkcs8PrivateKey(keyBytes, out _);
        }
        else if (key.StartsWith(JsonWebTokenPemEnumeration.ECDPrivateKey.PemStart))
        {
            byte[] keyBytes = JsonWebTokenPemEnumeration.GetBytes(JsonWebTokenPemEnumeration.ECDPrivateKey, key);
            ecd.ImportECPrivateKey(keyBytes, out _);
        }
        else
        {
            return new ResultInfo<SigningCredentials>(null!, JsonWebTokenEncoderDecoder.PublicKeyNotSupported, false);
        }

        SigningCredentials signingCredentials;
        switch (jsonWebTokenAlgorithm)
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

    private static void HandleExpiration(
        this SecurityTokenDescriptor tokenDescriptor,
        TokenParameters tokenParameters)
    {
        DateTime expirationDate = DateTime.UtcNow;
        if (tokenParameters.ExpirationYear.HasValue)
        {
            expirationDate.AddYears(tokenParameters.ExpirationYear.Value);
        }
        if (tokenParameters.ExpirationMonth.HasValue)
        {
            expirationDate.AddYears(tokenParameters.ExpirationMonth.Value);
        }
        if (tokenParameters.ExpirationDay.HasValue)
        {
            expirationDate.AddYears(tokenParameters.ExpirationDay.Value);
        }
        if (tokenParameters.ExpirationHour.HasValue)
        {
            expirationDate.AddYears(tokenParameters.ExpirationHour.Value);
        }
        if (tokenParameters.ExpirationMinute.HasValue)
        {
            expirationDate.AddYears(tokenParameters.ExpirationMinute.Value);
        }
        tokenDescriptor.Expires = expirationDate;
    }
}
