#nullable enable

using System;
using YamlDotNet.Serialization;

namespace DevToys.Helpers
{
    internal static class YamlHelper
    {
        /// <summary>
        /// Detects whether the given string is a valid YAML or not.
        /// </summary>
        internal static bool IsValidYaml(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return true;
            }

            input = input!.Trim();

            try
            {
                object result = new DeserializerBuilder().Build().Deserialize<object>(input);
                return result is not null && result is not string;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
