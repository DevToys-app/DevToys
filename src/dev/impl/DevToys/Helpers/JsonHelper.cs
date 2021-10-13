#nullable enable

using DevToys.Core;
using DevToys.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace DevToys.Helpers
{
    internal static class JsonHelper
    {
        /// <summary>
        /// Detects whether the given string is a valid JSON or not.
        /// </summary>
        internal static bool IsValid(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
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

        /// <summary>
        /// Format a string to the specified JSON format.
        /// </summary>
        internal static string Format(string? input, Indentation indentationMode)
        {
            if (indentationMode == null)
            {
                return string.Empty;
            }

            if (!IsValid(input))
            {
                return string.Empty;
            }

            try
            {
                JToken? jtoken = JToken.Parse(input!);
                if (jtoken is not null)
                {
                    var stringBuilder = new StringBuilder();
                    using (var stringWriter = new StringWriter(stringBuilder))
                    using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                    {
                        if (indentationMode.Equals(Indentation.TwoSpaces))
                        {
                            jsonTextWriter.Formatting = Formatting.Indented;
                            jsonTextWriter.IndentChar = ' ';
                            jsonTextWriter.Indentation = 2;
                        }
                        else if (indentationMode.Equals(Indentation.FourSpaces))
                        {
                            jsonTextWriter.Formatting = Formatting.Indented;
                            jsonTextWriter.IndentChar = ' ';
                            jsonTextWriter.Indentation = 4;
                        }
                        else if (indentationMode.Equals(Indentation.OneTab))
                        {
                            jsonTextWriter.Formatting = Formatting.Indented;
                            jsonTextWriter.IndentChar = '\t';
                            jsonTextWriter.Indentation = 1;
                        }
                        else if (indentationMode.Equals(Indentation.Minified))
                        {
                            jsonTextWriter.Formatting = Formatting.None;
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }

                        jtoken.WriteTo(jsonTextWriter);
                    }

                    return stringBuilder.ToString();
                }

                return string.Empty;
            }
            catch (JsonReaderException ex)
            {
                return ex.Message;
            }
            catch (Exception ex) //some other exception
            {
                Logger.LogFault("Json formatter", ex, $"Indentation: {indentationMode}");
                return ex.Message;
            }
        }
    }
}
