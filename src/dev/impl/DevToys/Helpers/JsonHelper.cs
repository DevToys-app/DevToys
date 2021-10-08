#nullable enable

using DevToys.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DevToys.Helpers
{
    internal static class JsonHelper
    {
        /// <summary>
        /// Detects whether the given string is a valid JSON or not.
        /// </summary>
        internal static bool IsValidJson(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return true;
            }

            input = input!.Trim();

            if ((input.StartsWith("{") && input.EndsWith("}")) //For object
                || (input.StartsWith("[") && input.EndsWith("]"))) //For array
            {
                try
                {
                    JToken? jtoken = JToken.Parse(input);
                    return jtoken is not null;
                }
                catch (JsonReaderException)
                {
                    // Exception in parsing json. It likely mean the text isn't a JSON.
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Logger.LogFault("Check is string if JSON", ex);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
