namespace DevToys.Tools.Models;

internal class TokenParameters
{
    public string? Token { get; set; }

    public string? Payload { get; set; }

    public string? Signature { get; set; }

    public string? PublicKey { get; set; }

    public string? PrivateKey { get; set; }

    public JwtAlgorithm TokenAlgorithm { get; set; }

    public int ExpirationYear { get; set; }

    public int ExpirationMonth { get; set; }

    public int ExpirationDay { get; set; }

    public int ExpirationHour { get; set; }

    public int ExpirationMinute { get; set; }

    public HashSet<string> ValidIssuers { get; set; } = new HashSet<string>();

    public HashSet<string> ValidAudiences { get; set; } = new HashSet<string>();

}
