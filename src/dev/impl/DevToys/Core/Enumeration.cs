using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DevToys.Core
{
    public abstract class Enumeration : IComparable
    {
        public string Name { get; }

        public string Code { get; }

        protected Enumeration(string name, string code)
        {
            Name = Arguments.NotNullOrWhiteSpace(name, nameof(name));
            Code = Arguments.NotNullOrWhiteSpace(code, nameof(code));
        }

        public static IEnumerable<T> GetAll<T>() where T : Enumeration
        {
            return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                        .Select(f => f.GetValue(null))
                        .Cast<T>();
        }

        public static T FromCode<T>(string code) where T : Enumeration
        {
            T matchingItem = Parse<T, string>(code, "code", item => item.Code == code);
            return matchingItem;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Enumeration otherValue)
            {
                return false;
            }

            bool typeMatches = GetType().Equals(obj.GetType());
            bool valueMatches = Code.Equals(otherValue.Code);

            return typeMatches && valueMatches;
        }

        public override int GetHashCode() => Code.GetHashCode();

        public int CompareTo(object other) => Code.CompareTo(((Enumeration)other).Code);

        private static T Parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration
        {
            T matchingItem = GetAll<T>().FirstOrDefault(predicate);

            if (matchingItem == null)
            {
                throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");
            }

            return matchingItem;
        }

    }
}
