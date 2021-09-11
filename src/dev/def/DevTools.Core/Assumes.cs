using System;

namespace DevTools.Core
{
    public static class Assumes
    {
        public static void NotNull<T>(T? value, string argumentName)
        {
            if (value == null)
            {
                throw new NullReferenceException(argumentName);
            }
        }

        public static void NotNullOrEmpty(string? value, string argumentName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new NullReferenceException(argumentName);
            }
        }

        public static void NotNullOrWhiteSpace(string? value, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new NullReferenceException(argumentName);
            }
        }
    }
}
