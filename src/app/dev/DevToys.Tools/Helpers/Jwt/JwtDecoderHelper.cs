using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using DevToys.Tools.Models;
using DevToys.Tools.Models.JwtDecoderEncoder;
using DevToys.Tools.Tools.EncodersDecoders.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DevToys.Tools.Helpers.Jwt;

internal static partial class JwtDecoderHelper
{
    public static async ValueTask<ResultInfo<JwtTokenResult?, ResultInfoSeverity>> DecodeTokenAsync(
            DecoderParameters decodeParameters,
            TokenParameters tokenParameters,
            ILogger logger,
            CancellationToken cancellationToken)
    {
        Guard.IsNotNull(decodeParameters);
        Guard.IsNotNull(tokenParameters);
        Guard.IsNotNullOrWhiteSpace(tokenParameters.Token);

        var tokenResult = new JwtTokenResult();

        try
        {
            IdentityModelEventSource.ShowPII = true;
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(tokenParameters.Token);

            ResultInfo<string> headerResult = await JsonHelper.FormatAsync(
                jwtSecurityToken.Header.SerializeToJson(),
                Indentation.TwoSpaces,
                false,
                logger,
                cancellationToken);
            if (!headerResult.HasSucceeded)
            {
                return new ResultInfo<JwtTokenResult?, ResultInfoSeverity>(JwtEncoderDecoder.HeaderInvalid, ResultInfoSeverity.Error);
            }
            tokenResult.Header = headerResult.Data;

            ResultInfo<string> payloadResult = await JsonHelper.FormatAsync(
                jwtSecurityToken.Payload.SerializeToJson(),
                Indentation.TwoSpaces,
                false,
                logger,
                cancellationToken);
            if (!payloadResult.HasSucceeded)
            {
                return new ResultInfo<JwtTokenResult?, ResultInfoSeverity>(JwtEncoderDecoder.PayloadInvalid, ResultInfoSeverity.Error);
            }
            tokenResult.Payload = payloadResult.Data;

            tokenResult.TokenAlgorithm = tokenParameters.TokenAlgorithm =
                Enum.TryParse(jwtSecurityToken.SignatureAlgorithm, out JwtAlgorithm parsedAlgorithm)
                    ? parsedAlgorithm
                    : tokenParameters.TokenAlgorithm;

            if (decodeParameters.ValidateSignature)
            {
                ResultInfo<JwtTokenResult> signatureValid = ValidateTokenSignature(handler, decodeParameters, tokenParameters, tokenResult);
                if (!signatureValid.HasSucceeded)
                {
                    return new ResultInfo<JwtTokenResult?, ResultInfoSeverity>(signatureValid.ErrorMessage!, ResultInfoSeverity.Error);
                }
            }

            tokenResult.Claims = jwtSecurityToken.Claims.Select(c => new JwtClaim(c));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid token detected '{decodeParameters}', '{tokenParameters}'", decodeParameters, tokenParameters);
            return new ResultInfo<JwtTokenResult?, ResultInfoSeverity>(ex.Message, ResultInfoSeverity.Error);
        }

        return new ResultInfo<JwtTokenResult?, ResultInfoSeverity>(tokenResult, ResultInfoSeverity.Success);
    }

    /// <summary>
    /// Validate the token using the Signing Credentials 
    /// </summary>
    private static ResultInfo<JwtTokenResult> ValidateTokenSignature(
        JwtSecurityTokenHandler handler,
        DecoderParameters decodeParameters,
        TokenParameters tokenParameters,
        JwtTokenResult tokenResult)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateActor = decodeParameters.ValidateActor,
            ValidateLifetime = decodeParameters.ValidateLifetime,
            ValidateIssuer = decodeParameters.ValidateIssuer,
            ValidateAudience = decodeParameters.ValidateAudience
        };

        if (decodeParameters.ValidateIssuerSigningKey)
        {
            ResultInfo<SigningCredentials> signingCredentials = GetSigningCredentials(tokenParameters);
            if (!signingCredentials.HasSucceeded)
            {
                return new ResultInfo<JwtTokenResult>(null!, signingCredentials.ErrorMessage!, signingCredentials.HasSucceeded);
            }

            validationParameters.ValidateIssuerSigningKey = decodeParameters.ValidateIssuerSigningKey;
            validationParameters.IssuerSigningKey = signingCredentials.Data!.Key;
            validationParameters.TryAllIssuerSigningKeys = true;
        }
        // Todo maybe we can remove this fake signature validator
        //else
        //{
        //    // Create a custom signature validator that does nothing so it always passes in this mode
        //    validationParameters.SignatureValidator = (token, _) => new JwtSecurityToken(token);
        //}

        /// check if the token issuers are part of the user provided issuers
        if (decodeParameters.ValidateIssuer)
        {
            if (tokenParameters.ValidIssuers.Count == 0)
            {
                return new ResultInfo<JwtTokenResult>(null!, JwtEncoderDecoder.ValidIssuersEmptyError, false);
            }
            validationParameters.ValidIssuers = tokenParameters.ValidIssuers;
        }

        /// check if the token audience are part of the user provided audiences
        if (decodeParameters.ValidateAudience)
        {
            if (tokenParameters.ValidAudiences.Count == 0)
            {
                return new ResultInfo<JwtTokenResult>(null!, JwtEncoderDecoder.ValidAudiencesEmptyError, false);
            }
            validationParameters.ValidAudiences = tokenParameters.ValidAudiences;
        }

        try
        {
            handler.ValidateToken(tokenParameters.Token, validationParameters, out _);
            tokenResult.Signature = tokenParameters.Signature;
            tokenResult.PublicKey = tokenParameters.PublicKey;
            return new ResultInfo<JwtTokenResult>(tokenResult);
        }
        catch (Exception exception)
        {
            return new ResultInfo<JwtTokenResult>(null!, exception.Message, false);
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
            JwtAlgorithm.HS256 or
            JwtAlgorithm.HS384 or
            JwtAlgorithm.HS512 => GetHmacShaSigningCredentials(tokenParameters.Signature, tokenParameters.TokenAlgorithm),
            JwtAlgorithm.RS256 or
            JwtAlgorithm.RS384 or
            JwtAlgorithm.RS512 or
            JwtAlgorithm.PS256 or
            JwtAlgorithm.PS384 or
            JwtAlgorithm.PS512 => GetRsaShaSigningCredentials(tokenParameters.PublicKey, tokenParameters.TokenAlgorithm),
            JwtAlgorithm.ES256 or
            JwtAlgorithm.ES384 or
            JwtAlgorithm.ES512 => GetECDsaSigningCredentials(tokenParameters.PublicKey, tokenParameters.TokenAlgorithm),
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
        JwtAlgorithm jwtAlgorithm)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return new ResultInfo<SigningCredentials>(null!, JwtEncoderDecoder.SignatureInvalid, false);
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
            case JwtAlgorithm.HS256:
                byte[] hs256Key = new HMACSHA256(signatureByte).Key;
                var hs256SymmetricSecurityKey = new SymmetricSecurityKey(hs256Key);
                signingCredentials = new SigningCredentials(hs256SymmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
                break;
            case JwtAlgorithm.HS384:
                byte[] hs384Key = new HMACSHA384(signatureByte).Key;
                var hs384SymmetricSecurityKey = new SymmetricSecurityKey(hs384Key);
                signingCredentials = new SigningCredentials(hs384SymmetricSecurityKey, SecurityAlgorithms.HmacSha384Signature);
                break;
            case JwtAlgorithm.HS512:
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
        JwtAlgorithm jwtAlgorithm)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return new ResultInfo<SigningCredentials>(null!, JwtEncoderDecoder.PublicKeyInvalid, false);
        }

        var rsa = RSA.Create();
        if (key.StartsWith(JwtPemEnumeration.PublicKey.PemStart))
        {
            byte[] keyBytes = JwtPemEnumeration.GetBytes(JwtPemEnumeration.PublicKey, key);
            rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
        }
        else if (key.StartsWith(JwtPemEnumeration.RsaPublicKey.PemStart))
        {
            byte[] keyBytes = JwtPemEnumeration.GetBytes(JwtPemEnumeration.RsaPublicKey, key);
            rsa.ImportRSAPublicKey(keyBytes, out _);
        }
        else
        {
            return new ResultInfo<SigningCredentials>(null!, JwtEncoderDecoder.PublicKeyNotSupported, false);
        }

        SigningCredentials signingCredentials;
        switch (jwtAlgorithm)
        {
            case JwtAlgorithm.RS256:
                var rs256RsaSecurityKey = new RsaSecurityKey(rsa);
                signingCredentials = new SigningCredentials(rs256RsaSecurityKey, SecurityAlgorithms.RsaSha256Signature);
                break;
            case JwtAlgorithm.RS384:
                var rs384SymmetricSecurityKey = new RsaSecurityKey(rsa);
                signingCredentials = new SigningCredentials(rs384SymmetricSecurityKey, SecurityAlgorithms.RsaSha384Signature);
                break;
            case JwtAlgorithm.RS512:
                var rs512SymmetricSecurityKey = new RsaSecurityKey(rsa);
                signingCredentials = new SigningCredentials(rs512SymmetricSecurityKey, SecurityAlgorithms.RsaSha512Signature);
                break;
            case JwtAlgorithm.PS256:
                var ps256RsaSecurityKey = new RsaSecurityKey(rsa);
                signingCredentials = new SigningCredentials(ps256RsaSecurityKey, SecurityAlgorithms.RsaSsaPssSha256Signature);
                break;
            case JwtAlgorithm.PS384:
                var ps384SymmetricSecurityKey = new RsaSecurityKey(rsa);
                signingCredentials = new SigningCredentials(ps384SymmetricSecurityKey, SecurityAlgorithms.RsaSsaPssSha384Signature);
                break;
            case JwtAlgorithm.PS512:
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
        JwtAlgorithm jwtAlgorithm)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return new ResultInfo<SigningCredentials>(null!, JwtEncoderDecoder.PublicKeyInvalid, false);
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

        if (key.StartsWith(JwtPemEnumeration.PublicKey.PemStart))
        {
            byte[] keyBytes = JwtPemEnumeration.GetBytes(JwtPemEnumeration.PublicKey, key);
            ecd.ImportSubjectPublicKeyInfo(keyBytes, out _);
        }
        else if (key.StartsWith(JwtPemEnumeration.ECDPublicKey.PemStart))
        {
            byte[] keyBytes = JwtPemEnumeration.GetBytes(JwtPemEnumeration.ECDPublicKey, key);
            ecd.ImportECPrivateKey(keyBytes, out _);
        }
        else
        {
            return new ResultInfo<SigningCredentials>(null!, JwtEncoderDecoder.PublicKeyNotSupported, false);
        }

        SigningCredentials signingCredentials;
        switch (jwtAlgorithm)
        {
            case JwtAlgorithm.ES256:
                var es256RsaSecurityKey = new ECDsaSecurityKey(ecd);
                signingCredentials = new SigningCredentials(es256RsaSecurityKey, SecurityAlgorithms.EcdsaSha256Signature);
                break;
            case JwtAlgorithm.ES384:
                var es384SymmetricSecurityKey = new ECDsaSecurityKey(ecd);
                signingCredentials = new SigningCredentials(es384SymmetricSecurityKey, SecurityAlgorithms.RsaSha384Signature);
                break;
            case JwtAlgorithm.ES512:
                var es512SymmetricSecurityKey = new ECDsaSecurityKey(ecd);
                signingCredentials = new SigningCredentials(es512SymmetricSecurityKey, SecurityAlgorithms.RsaSha512Signature);
                break;
            default:
                throw new NotSupportedException();
        }
        return new ResultInfo<SigningCredentials>(signingCredentials);
    }

    //private static void ValidatePublicKey(string key)
    //{
    //    using var rsa = new RSACryptoServiceProvider();
    //    try
    //    {
    //        rsa.ImportFromPem(key.AsSpan());
    //        if (rsa.PublicOnly)
    //        {
    //            Console.WriteLine("Public RSA key");
    //        }
    //        else
    //        {
    //            RSAParameters rsaParams = rsa.ExportParameters(true);
    //            BigInteger m = new(rsaParams.Modulus, true, true);
    //            BigInteger p = new(rsaParams.P, true, true);
    //            BigInteger q = new(rsaParams.Q, true, true);
    //        }
    //    }
    //    catch (Exception ex)
    //    {

    //    }
    //}
}
