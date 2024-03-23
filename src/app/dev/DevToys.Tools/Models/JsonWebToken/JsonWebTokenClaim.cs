using System.Security.Claims;

namespace DevToys.Tools.Models;

internal class JsonWebTokenClaim
{
    public string Key { get; }

    public TextSpan Span { get; }

    public string Value { get; set; }

    public JsonWebTokenClaim(string key, string value, TextSpan span)
    {
        Key = key;
        Value = value;
        Span = span;
    }
}
