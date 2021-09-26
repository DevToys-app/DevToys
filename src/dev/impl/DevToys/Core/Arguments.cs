#nullable enable

using System;

namespace DevToys.Core
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
    }
}
