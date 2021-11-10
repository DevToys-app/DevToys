#nullable enable

using System;
using System.IdentityModel.Tokens.Jwt;

namespace DevToys.Helpers
{
    internal static class JwtHelper
    {
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

            try
            {
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(input);
                return jwtSecurityToken is not null;
            }
            catch (Exception) //some other exception
            {
                return false;
            }
        }
    }
}
