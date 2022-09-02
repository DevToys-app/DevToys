#nullable enable

using DevToys;

namespace DevToys.Models.JwtDecoderEncoder
{
    public record EncoderParameters
    {
        public bool HasExpiration { get; set; }

        public bool HasAudience { get; set; }

        public bool HasIssuer { get; set; }

        public bool HasDefaultTime { get; set; }
    }
}
