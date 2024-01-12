using System.Resources;
using System.Security.Claims;

namespace DevToys.Tools.Models;

internal class JwtClaim
{
    public string Key { get; }

    public string Value { get; }

    public JwtClaim(Claim claim)
    {
        Key = claim.Type;
        Value = claim.Value;
    }

    public JwtClaim(string key, string value)
    {
        Key= key;
        Value = value;
    }
}
