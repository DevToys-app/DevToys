using System;

namespace DevToys.Models
{
    public sealed class JwtAlgorithmDisplayPair : IEquatable<JwtAlgorithmDisplayPair>
    {
        public static readonly JwtAlgorithmDisplayPair HS256 = new(nameof(HS256), JwtAlgorithm.HS256);
        public static readonly JwtAlgorithmDisplayPair HS384 = new(nameof(HS384), JwtAlgorithm.HS384);
        public static readonly JwtAlgorithmDisplayPair HS512 = new(nameof(HS512), JwtAlgorithm.HS512);
        public static readonly JwtAlgorithmDisplayPair RS256 = new(nameof(RS256), JwtAlgorithm.RS256);
        public static readonly JwtAlgorithmDisplayPair RS384 = new(nameof(RS384), JwtAlgorithm.RS384);
        public static readonly JwtAlgorithmDisplayPair RS512 = new(nameof(RS512), JwtAlgorithm.RS512);
        public static readonly JwtAlgorithmDisplayPair ES256 = new(nameof(ES256), JwtAlgorithm.ES256);
        public static readonly JwtAlgorithmDisplayPair ES384 = new(nameof(ES384), JwtAlgorithm.ES384);
        public static readonly JwtAlgorithmDisplayPair ES512 = new(nameof(ES512), JwtAlgorithm.ES512);
        public static readonly JwtAlgorithmDisplayPair PS256 = new(nameof(PS256), JwtAlgorithm.PS256);
        public static readonly JwtAlgorithmDisplayPair PS384 = new(nameof(PS384), JwtAlgorithm.PS384);
        public static readonly JwtAlgorithmDisplayPair PS512 = new(nameof(PS512), JwtAlgorithm.PS512);

        public string DisplayName { get; }

        public JwtAlgorithm Value { get; }

        private JwtAlgorithmDisplayPair(string displayName, JwtAlgorithm value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public bool Equals(JwtAlgorithmDisplayPair other) => other.Value == Value;
    }
}
