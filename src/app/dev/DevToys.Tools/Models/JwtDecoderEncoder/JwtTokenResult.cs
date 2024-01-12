namespace DevToys.Tools.Models;

internal class JwtTokenResult
{
    public string? Token { get; set; }

    public string? Header { get; set; }

    public string? Payload { get; set; }
    public string? Signature { get; set; }

    public string? PublicKey { get; set; }

    public string? PrivateKey { get; set; }

    public JwtAlgorithm TokenAlgorithm { get; set; }

    public List<JwtClaim> HeaderClaims { get; set; } = new List<JwtClaim>();

    public List<JwtClaim> PayloadClaims { get; set; } = new List<JwtClaim>();
}
