#nullable enable

using System;
using System.IdentityModel.Tokens.Jwt;

namespace DevToys.Helpers
{
    internal static class JwtHelper
    {
        private static readonly string AuthorizationHeader = "Authorization:";
        private static readonly string BearerScheme = "Bearer";

        /// <summary>
        /// Detects whether the given string is a JWT Token or not.
        /// </summary>
        internal static bool IsValid(string? input)
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
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(input.Trim());
                return jwtSecurityToken is not null;
            }
            catch (Exception) //some other exception
            {
                return false;
            }
        }
    }
}
