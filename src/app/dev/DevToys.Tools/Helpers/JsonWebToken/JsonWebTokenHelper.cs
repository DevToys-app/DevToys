using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Helpers.JsonWebToken;

using Microsoft.IdentityModel.JsonWebTokens;

internal static partial class JsonWebTokenHelper
{
    private static readonly string AuthorizationHeader = "Authorization:";
    private static readonly string BearerScheme = "Bearer";

    /// <summary>
    /// Detects whether the given string is a JWT Token or not.
    /// </summary>
    internal static bool IsValid(string? input, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        input = input!.Trim();

        if (input.StartsWith(AuthorizationHeader))
        {
            input = input.Remove(0, AuthorizationHeader.Length).Trim();
        }

        if (input.StartsWith(BearerScheme))
        {
            input = input.Remove(0, BearerScheme.Length).Trim();
        }

        try
        {
            JsonWebTokenHandler handler = new();
            JsonWebToken jsonWebToken = handler.ReadJsonWebToken(input);
            return jsonWebToken is not null;
        }
        catch (Exception ex) //some other exception
        {
            logger.LogError(ex, "Invalid data detected '{input}'", input);
            return false;
        }
    }
}
