using System.Security.Claims;

namespace DevToys.Tools.Models;

internal class JsonWebTokenClaim
{
    public string Key { get; }

    public string Value { get; }

    public JsonWebTokenClaim(Claim claim)
    {
        Key = claim.Type;
        Value = claim.Value;
    }

    public JsonWebTokenClaim(string key, string value)
    {
        Key = key;
        Value = value;
    }
}
