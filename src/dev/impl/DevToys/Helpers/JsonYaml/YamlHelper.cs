#nullable enable

using System;
using System.Dynamic;
using DevToys.Core;
using DevToys.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace DevToys.Helpers.JsonYaml
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
                return false;
            }

            input = input!.Trim();

            try
            {
                object? result = new DeserializerBuilder().Build().Deserialize<object>(input);
                return result is not null and not string;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Convert a Json string to Yaml
        /// </summary>
        internal static string? ConvertFromJson(string? input, Indentation indentationMode)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            try
            {
                object? jsonObject = null;
                var token = JToken.Parse(input!);
                if (token is null)
                {
                    return string.Empty;
                }

                JsonSerializerSettings defaultJsonSerializerSettings = new()
                {
                    FloatParseHandling = FloatParseHandling.Decimal
                };

                if (token is JArray)
                {
                    jsonObject = JsonConvert.DeserializeObject<ExpandoObject[]>(input!, defaultJsonSerializerSettings);
                }
                else
                {
                    jsonObject = JsonConvert.DeserializeObject<ExpandoObject>(input!, defaultJsonSerializerSettings);
                }


                if (jsonObject is not null and not string)
                {
                    int indent = 0;
                    indent = indentationMode switch
                    {
                        Indentation.TwoSpaces => 2,
                        Indentation.FourSpaces => 4,
                        _ => throw new NotSupportedException(),
                    };
                    var serializer
                        = Serializer.FromValueSerializer(
                            new SerializerBuilder().BuildValueSerializer(),
                            EmitterSettings.Default.WithBestIndent(indent).WithIndentedSequences());

                    string? yaml = serializer.Serialize(jsonObject);
                    if (string.IsNullOrWhiteSpace(yaml))
                    {
                        return string.Empty;
                    }

                    return yaml;
                }
                return string.Empty;
            }
            catch (JsonReaderException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogFault("Yaml to Json Converter", ex);
                return string.Empty;
            }
        }

    }
}
