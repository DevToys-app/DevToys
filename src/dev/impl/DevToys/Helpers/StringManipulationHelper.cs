#nullable enable

namespace DevToys.Helpers
{
    internal static class StringManipulationHelper
    {
        internal static bool HasEscapeCharacters(string data)
        {
            return data.Contains("\\n")
                || data.Contains("\\r")
                || data.Contains("\\\\")
                || data.Contains("\\\"")
                || data.Contains("\\t")
                || data.Contains("\\f")
                || data.Contains("\\b");
        }
    }
}
