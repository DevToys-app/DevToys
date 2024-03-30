namespace DevToys.Tools.Helpers.JsonWebToken;

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
}
