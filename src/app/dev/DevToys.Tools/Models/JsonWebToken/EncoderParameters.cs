namespace DevToys.Tools.Models;

internal record EncoderParameters
{
    public bool HasExpiration { get; set; }

    public bool HasAudience { get; set; }

    public bool HasIssuer { get; set; }

    public bool HasDefaultTime { get; set; }
}
