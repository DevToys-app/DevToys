namespace DevToys.Tools.Models;

internal class JsonWebTokenResult
{
    public string? Token { get; set; }

    public string? Header { get; set; }

    public string? Payload { get; set; }
    public string? Signature { get; set; }

    public string? PublicKey { get; set; }

    public string? PrivateKey { get; set; }

    public JsonWebTokenAlgorithm TokenAlgorithm { get; set; }

    public List<JsonWebTokenClaim> PayloadClaims { get; set; } = new List<JsonWebTokenClaim>();
}
