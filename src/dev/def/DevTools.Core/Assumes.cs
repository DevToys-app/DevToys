using System;

namespace DevTools.Core
{
    public static class Assumes
    {
        public static T NotNull<T>(T? value, string argumentName)
        {
            if (value == null)
            {
                throw new NullReferenceException(argumentName);
            }

            return value;
        }

        public static string NotNullOrEmpty(string? value, string argumentName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new NullReferenceException(argumentName);
            }

            return value!;
        }

        public static string NotNullOrWhiteSpace(string? value, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new NullReferenceException(argumentName);
            }

            return value!;
        }
    }
}
