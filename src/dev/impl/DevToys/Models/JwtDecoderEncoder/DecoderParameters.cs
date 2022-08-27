#nullable enable

using DevToys;

namespace DevToys.Models.JwtDecoderEncoder
{
    public class DecoderParameters
    {
        public bool ValidateSignature { get; set; }

        public bool ValidateActor { get; set; }

        public bool ValidateLifetime { get; set; }

        public bool ValidateIssuer { get; set; }

        public bool ValidateAudience { get; set; }

    }
}
