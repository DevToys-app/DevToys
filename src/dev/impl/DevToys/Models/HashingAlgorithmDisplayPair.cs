﻿using System;

namespace DevToys.Models
{
    public sealed class HashingAlgorithmDisplayPair : IEquatable<HashingAlgorithmDisplayPair>
    {
        public static readonly HashingAlgorithmDisplayPair MD5 = new(nameof(MD5), HashingAlgorithm.MD5);
        public static readonly HashingAlgorithmDisplayPair SHA1 = new(nameof(SHA1), HashingAlgorithm.SHA1);
        public static readonly HashingAlgorithmDisplayPair SHA256 = new(nameof(SHA256), HashingAlgorithm.SHA256);
        public static readonly HashingAlgorithmDisplayPair SHA384 = new(nameof(SHA384), HashingAlgorithm.SHA384);
        public static readonly HashingAlgorithmDisplayPair SHA512 = new(nameof(SHA512), HashingAlgorithm.SHA512);

        public string DisplayName { get; }

        public HashingAlgorithm Value { get; }

        private HashingAlgorithmDisplayPair(string displayName, HashingAlgorithm value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public bool Equals(HashingAlgorithmDisplayPair other) => other.Value == Value;
    }
}
