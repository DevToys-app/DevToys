namespace DevToys.Tools.Models;

internal record JsonWebTokenClaim(string Key, string Value, string? FormattedValue, TextSpan Span)
{
    public string ActualValue => FormattedValue ?? Value;
}

