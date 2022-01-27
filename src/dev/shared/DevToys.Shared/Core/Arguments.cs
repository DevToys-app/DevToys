using System;

namespace DevToys.Shared.Core
{
    public static class Arguments
    {
        public static T NotNull<T>(T? value, string argumentName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }

            return value;
        }

        public static string NotNullOrEmpty(string? value, string argumentName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(argumentName);
            }

            return value!;
        }

        public static string NotNullOrWhiteSpace(string? value, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(argumentName);
            }

            return value!;
        }

        public static long NotZeroOrBelow(long value, string argumentName)
        {
            if (value <= 0)
            {
                throw new ArgumentException("Value can't be zero or below zero", argumentName);
            }

            return value;
        }
    }
}
