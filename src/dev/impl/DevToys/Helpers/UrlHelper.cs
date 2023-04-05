using System;

namespace DevToys.Helpers
{
    internal static class UrlHelper
    {
        /// <summary>
        /// Url encodes a string.
        /// This is a wrapper function for the built in System.Uri api to accommodate this issue:
        /// https://github.com/microsoft/microsoft-ui-xaml/issues/1826
        /// </summary>
        /// <param name="data">Input to url encode</param>
        /// <returns>Url encoded result</returns>
        internal static string UrlEncode(string data)
        {
            string newLineAdjusted = data.Replace("\r\n", "\n").Replace("\r", "\n");
            string encoded = Uri.EscapeDataString(newLineAdjusted);

            return encoded;
        }
    }
}
