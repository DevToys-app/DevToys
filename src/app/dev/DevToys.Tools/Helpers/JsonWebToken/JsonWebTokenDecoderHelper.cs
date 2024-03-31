using DevToys.Tools.Models;
using DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using static DevToys.Tools.Helpers.JsonWebToken.JsonWebTokenEncoderDecoderHelper;

namespace DevToys.Tools.Helpers.JsonWebToken;

using System.Security.Claims;
using DevToys.Api;
using Microsoft.IdentityModel.JsonWebTokens;

internal static class JsonWebTokenDecoderHelper
{
    private static readonly HashSet<string> _claimDateFields = new() { "exp", "nbf", "iat", "auth_time", "updated_at" };

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

    public static async ValueTask<ResultInfo<JsonWebTokenResult?>> DecodeTokenAsync(
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

            ResultInfo<string> headerResult = await jsonWebToken.GetFormattedHeaderAsync(logger, cancellationToken);
            if (!headerResult.HasSucceeded)
            {
                return ResultInfo<JsonWebTokenResult?>.Error(headerResult.Message!);
            }
            tokenResult.Header = headerResult.Data;

            ResultInfo<string> payloadResult = await jsonWebToken.GetFormattedPayloadAsync(logger, cancellationToken);
            if (!payloadResult.HasSucceeded)
            {
                return ResultInfo<JsonWebTokenResult?>.Error(JsonWebTokenEncoderDecoder.InvalidPayload);
            }
            tokenResult.Payload = payloadResult.Data;
            tokenResult.PayloadClaims = ProcessClaims(payloadResult.Data, jsonWebToken.Claims);

            if (decodeParameters.ValidateSignature)
            {
                return await ValidateTokenSignatureAsync(handler, decodeParameters, tokenParameters, tokenResult);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid token detected");
            return ResultInfo<JsonWebTokenResult?>.Error(ex.Message);
        }

        return tokenResult;
    }

    /// <summary>
    /// Validate the token using the Signing Credentials 
    /// </summary>
    private static async Task<ResultInfo<JsonWebTokenResult?>> ValidateTokenSignatureAsync(
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
                return ResultInfo<JsonWebTokenResult?>.Error(signingCredentials.Message!);
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
                return ResultInfo<JsonWebTokenResult?>.Error(JsonWebTokenEncoderDecoder.ValidIssuersEmptyError);
            }
            validationParameters.ValidIssuers = tokenParameters.Issuers;
        }

        // check if the token audience are part of the user provided audiences
        if (decodeParameters.ValidateAudiences)
        {
            if (tokenParameters.Audiences.Count == 0)
            {
                return ResultInfo<JsonWebTokenResult?>.Error(JsonWebTokenEncoderDecoder.ValidAudiencesEmptyError);
            }
            validationParameters.ValidAudiences = tokenParameters.Audiences;
        }

        try
        {
            TokenValidationResult validationResult = await handler.ValidateTokenAsync(tokenParameters.Token, validationParameters);
            if (!validationResult.IsValid)
            {
                return ResultInfo<JsonWebTokenResult?>.Error(validationResult.Exception.Message);
            }
            tokenResult.Signature = tokenParameters.Signature;
            tokenResult.PublicKey = tokenParameters.PublicKey;

            if (!decodeParameters.ValidateActors
                && !decodeParameters.ValidateLifetime
                && !decodeParameters.ValidateIssuers
                && !decodeParameters.ValidateAudiences
                && !decodeParameters.ValidateIssuersSigningKey)
            {
                return ResultInfo<JsonWebTokenResult?>.Warning(tokenResult, JsonWebTokenEncoderDecoder.TokenNotValidated);
            }
            return tokenResult;
        }
        catch (Exception exception)
        {
            return ResultInfo<JsonWebTokenResult?>.Error(exception.Message);
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
            if (_claimDateFields.Contains(claim.Type) && long.TryParse(claim.Value, out long value))
            {
                processedClaim.Value = $"{DateTimeOffset.FromUnixTimeSeconds(value).ToLocalTime()} ({claim.Value})";
            }
            processedClaims.Add(processedClaim);
        }

        return processedClaims;
    }
}
