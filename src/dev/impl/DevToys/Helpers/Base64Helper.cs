#nullable enable

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace DevToys.Helpers
{
    internal static class Base64Helper
    {
        internal static bool IsBase64DataStrict(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }

            data = data!.Trim();

            if (data.Length % 4 != 0)
            {
                return false;
            }

            if (new Regex(@"[^A-Z0-9+/=]", RegexOptions.IgnoreCase).IsMatch(data))
            {
                return false;
            }

            int equalIndex = data.IndexOf('=');
            int length = data.Length;

            if (!(equalIndex == -1 || equalIndex == length - 1 || (equalIndex == length - 2 && data[length - 1] == '=')))
            {
                return false;
            }

            string? decoded;

            try
            {
                byte[]? decodedData = Convert.FromBase64String(data);
                decoded = Encoding.UTF8.GetString(decodedData);
            }
            catch (Exception)
            {
                return false;
            }

            //check for special chars that you know should not be there
            char current;
            for (int i = 0; i < decoded.Length; i++)
            {
                current = decoded[i];
                if (current == 65533)
                {
                    return false;
                }

#pragma warning disable IDE0078 // Use pattern matching
                if (!(current == 0x9
                    || current == 0xA
                    || current == 0xD
                    || (current >= 0x20 && current <= 0xD7FF)
                    || (current >= 0xE000 && current <= 0xFFFD)
                    || (current >= 0x10000 && current <= 0x10FFFF)))
#pragma warning restore IDE0078 // Use pattern matching
                {
                    return false;
                }
            }

            return true;
        }
    }
}
