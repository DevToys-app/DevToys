﻿using DevToys.Tools.Models;
using DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using static DevToys.Tools.Helpers.JsonWebToken.JsonWebTokenEncoderDecoderHelper;

namespace DevToys.Tools.Helpers.JsonWebToken;

using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

internal static class JsonWebTokenDecoderHelper
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
            ResultInfo<SigningCredentials> signingCredentials = GetSigningCredentials(tokenParameters, false);
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
