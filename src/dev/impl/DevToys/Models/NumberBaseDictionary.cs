using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevToys.Models
{
    public class NumberBaseDictionary : IEquatable<NumberBaseDictionary>
    {
        public static readonly NumberBaseDictionary Base16Dictionary = new(
            _dictionary: "0123456789ABCDEF".ToCharArray(), 
            formatting: true);

        public static readonly NumberBaseDictionary RFC4648Base32Dictionary = new(
            _dictionary: "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray(),
            formatting: false);

        public static readonly NumberBaseDictionary RFC4648Base32ExHexDictionary = new(
            _dictionary: "0123456789ABCDEFGHIJKLMNOPQRSTUV".ToCharArray(),
            formatting: false);

        public static readonly NumberBaseDictionary RFC4648Base64Dictionary = new(
            _dictionary: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray(),
            formatting: false);

        public static readonly NumberBaseDictionary RFC4648Base64UrlEncodeDictionary = new(
             _dictionary: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".ToCharArray(),
             formatting: false);
        public static NumberBaseDictionary DefaultDictionary { get; } = RFC4648Base64Dictionary;

        public char[] Dictionary { get; }
        public bool AllowsFormatting { get; }

        public NumberBaseDictionary(char[] _dictionary, bool formatting)
        {
            Dictionary = _dictionary;
            AllowsFormatting = formatting;
        }

        public static implicit operator NumberBaseDictionary(char[] dict)
        {
            return new NumberBaseDictionary(dict, dict.All(c => char.IsLower(c)));
        }

        public char this[int index]
        {
            get
            {
                return Dictionary[index];
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is not NumberBaseDictionary other)
            { 
                return false;
            }
            return Equals(other);
        }

        public bool Equals(NumberBaseDictionary other)
        {
            return AllowsFormatting == other.AllowsFormatting && Dictionary.SequenceEqual(other.Dictionary);
        }
    }
}
