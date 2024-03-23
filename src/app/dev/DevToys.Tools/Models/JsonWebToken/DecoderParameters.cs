namespace DevToys.Tools.Models;

internal record DecoderParameters
{
    public bool ValidateSignature { get; set; }

    public bool ValidateIssuersSigningKey { get; set; }

    public bool ValidateActors { get; set; }

    public bool ValidateLifetime { get; set; }

    public bool ValidateIssuers { get; set; }

    public bool ValidateAudiences { get; set; }
}
