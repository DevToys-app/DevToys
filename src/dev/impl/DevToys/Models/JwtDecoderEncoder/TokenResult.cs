#nullable enable


namespace DevToys.Models.JwtDecoderEncoder
{
    public class TokenResult
    {
        public string? Token { get; set; }

        public string? Header { get; set; }

        public string? Payload { get; set; }

        public string? Signature { get; set; }

        public string? PublicKey { get; set; }

        public string? PrivateKey { get; set; }

        public JwtAlgorithm TokenAlgorithm { get; set; }
    }
}
