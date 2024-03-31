namespace DevToys.Tools.Helpers.JsonWebToken;

using DevToys.Tools.Models;
using DevToys.Tools.Tools.EncodersDecoders.JsonWebToken;
using Microsoft.Extensions.Logging;
using System.Threading;
using Microsoft.IdentityModel.JsonWebTokens;

internal static partial class JsonWebTokenHelper
{
    private const string AuthorizationHeader = "Authorization:";
    private const string BearerScheme = "Bearer";

    /// <summary>
    /// Detects whether the given string is a JWT Token or not.
    /// </summary>
    internal static bool IsValid(string? input)
    {
        ReadOnlySpan<char> inputSpan = input.AsSpan().Trim();

        if (inputSpan.IsEmpty)
        {
            return false;
        }

        if (inputSpan.StartsWith(AuthorizationHeader))
        {
            inputSpan = inputSpan.Slice(AuthorizationHeader.Length).TrimStart();
        }

        if (inputSpan.StartsWith(BearerScheme))
        {
            inputSpan = inputSpan.Slice(BearerScheme.Length).TrimStart();
        }

        try
        {
            JsonWebTokenHandler handler = new();
            JsonWebToken jsonWebToken = handler.ReadJsonWebToken(inputSpan.ToString());
            return jsonWebToken is not null;
        }
        catch
        {
            return false;
        }
    }

    internal static Task<ResultInfo<string>> GetFormattedHeaderAsync(this JsonWebToken jwt, ILogger logger, CancellationToken cancellationToken)
    {
        return GetJsonWebTokenPayloadOrHeaderCore(jwt, isHeader: true, logger, cancellationToken);
    }

    internal static Task<ResultInfo<string>> GetFormattedPayloadAsync(this JsonWebToken jwt, ILogger logger, CancellationToken cancellationToken)
    {
        return GetJsonWebTokenPayloadOrHeaderCore(jwt, isHeader: false, logger, cancellationToken);
    }

    private static async Task<ResultInfo<string>> GetJsonWebTokenPayloadOrHeaderCore(
        JsonWebToken jwt,
        bool isHeader,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        string decodedHeader = Base64Helper.FromBase64ToText(
            isHeader ? jwt.EncodedHeader : jwt.EncodedPayload,
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
            return ResultInfo<string>.Error(isHeader ? JsonWebTokenEncoderDecoder.InvalidHeader : JsonWebTokenEncoderDecoder.InvalidPayload);
        }

        return headerResult;
    }
}
