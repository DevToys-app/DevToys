#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Text;
using DevToys.Core;
using DevToys.Helpers.JsonYaml.Core;
using DevToys.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace DevToys.Helpers.JsonYaml
{
    internal static class JsonHelper
    {
        /// <summary>
        /// Detects whether the given string is a valid JSON or not.
        /// </summary>
        internal static bool IsValid(string? input)
        {
            input = input?.Trim();

            if (input == null)
            {
                return true;
            }

            if (long.TryParse(input, out _))
            {
                return false;
            }

            try
            {
                var jtoken = JToken.Parse(input);
                return jtoken is not null;
            }
            catch (JsonReaderException)
            {
                // Exception in parsing json. It likely mean the text isn't a JSON.
                return false;
            }
            catch (Exception ex) //some other exception
            {
                Logger.LogFault("Check if string is JSON", ex);
                return false;
            }
        }

        /// <summary>
        /// Format a string to the specified JSON format.
        /// </summary>
        internal static string Format(string? input, Indentation indentationMode, bool sortProperties)
        {
            if (input == null || !IsValid(input))
            {
                return string.Empty;
            }

            try
            {
                var jsonLoadSettings = new JsonLoadSettings()
                {
                    CommentHandling = CommentHandling.Ignore,
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore,
                    LineInfoHandling = LineInfoHandling.Load
                };

                JObject jObject;
                using (var jsonReader = new JsonTextReader(new StringReader(input)))
                {
                    jsonReader.DateParseHandling = DateParseHandling.None;
                    jsonReader.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;

                    jObject = JObject.Load(jsonReader, jsonLoadSettings);
                }

                if (sortProperties)
                {
                    SortJsonPropertiesAlphabetically(jObject);
                }

                var stringBuilder = new StringBuilder();
                using (var stringWriter = new StringWriter(stringBuilder))
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    switch (indentationMode)
                    {
                        case Indentation.TwoSpaces:
                            jsonTextWriter.Formatting = Formatting.Indented;
                            jsonTextWriter.IndentChar = ' ';
                            jsonTextWriter.Indentation = 2;
                            break;
                        case Indentation.FourSpaces:
                            jsonTextWriter.Formatting = Formatting.Indented;
                            jsonTextWriter.IndentChar = ' ';
                            jsonTextWriter.Indentation = 4;
                            break;
                        case Indentation.OneTab:
                            jsonTextWriter.Formatting = Formatting.Indented;
                            jsonTextWriter.IndentChar = '\t';
                            jsonTextWriter.Indentation = 1;
                            break;
                        case Indentation.Minified:
                            jsonTextWriter.Formatting = Formatting.None;
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    jsonTextWriter.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    jsonTextWriter.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;

                    jObject.WriteTo(jsonTextWriter);
                }

                return stringBuilder.ToString();
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

        /// <summary>
        /// Convert a Yaml string to Json
        /// </summary>
        internal static string? ConvertFromYaml(string? input, Indentation indentationMode)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            try
            {
                using var stringReader = new StringReader(input);

                IDeserializer deserializer = new DeserializerBuilder()
                    .WithNodeTypeResolver(new DecimalYamlTypeResolver())
                    .Build();

                object? yamlObject = deserializer.Deserialize(stringReader);

                if (yamlObject is null or string)
                {
                    return null;
                }

                var stringBuilder = new StringBuilder();
                using (var stringWriter = new StringWriter(stringBuilder))
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    switch (indentationMode)
                    {
                        case Indentation.TwoSpaces:
                            jsonTextWriter.Formatting = Formatting.Indented;
                            jsonTextWriter.IndentChar = ' ';
                            jsonTextWriter.Indentation = 2;
                            break;

                        case Indentation.FourSpaces:
                            jsonTextWriter.Formatting = Formatting.Indented;
                            jsonTextWriter.IndentChar = ' ';
                            jsonTextWriter.Indentation = 4;
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                    var jsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings()
                    {
                        Converters = { new DecimalJsonConverter() }
                    });
                    jsonSerializer.Serialize(jsonTextWriter, yamlObject);
                }

                return stringBuilder.ToString();
            }
            catch (SemanticErrorException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogFault("Yaml to Json Converter", ex);
                return string.Empty;
            }
        }

        private static void SortJsonPropertiesAlphabetically(JObject jObject)
        {
            var properties = jObject.Properties().ToList();
            foreach (JProperty? property in properties)
            {
                property.Remove();
            }

            foreach (JProperty? property in properties.OrderBy(p => p.Name))
            {
                jObject.Add(property);
                if (property.Value is JObject obj)
                {
                    SortJsonPropertiesAlphabetically(obj);
                }
            }
        }
    }
}
