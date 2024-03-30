namespace DevToys.Tools.Models;

internal class TokenParameters
{
    public string? Token { get; set; }

    public string? Payload { get; set; }

    public string? Signature { get; set; }

    public bool IsSignatureInBase64Format { get; set; }

    public string? PublicKey { get; set; }

    public string? PrivateKey { get; set; }

    public JsonWebTokenAlgorithm TokenAlgorithm { get; set; }

    public int? ExpirationYear { get; set; }

    public int? ExpirationMonth { get; set; }

    public int? ExpirationDay { get; set; }

    public int? ExpirationHour { get; set; }

    public int? ExpirationMinute { get; set; }

    public HashSet<string> Issuers { get; set; } = new HashSet<string>();

    public HashSet<string> Audiences { get; set; } = new HashSet<string>();

    public void DefineExpirationDate(DateTimeOffset dateTimeOffset)
    {
        ExpirationYear = dateTimeOffset.Year;
        ExpirationMonth = dateTimeOffset.Month;
        ExpirationDay = dateTimeOffset.Day;
        ExpirationHour = dateTimeOffset.Hour;
        ExpirationMinute = dateTimeOffset.Minute;
    }
}
