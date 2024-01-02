using System.Text;
using DevToys.Tools.Helpers.Core;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace DevToys.Tools.Helpers;

internal static partial class JsonHelper
{
    /// <summary>
    /// Detects whether the given string is a valid JSON or not.
    /// </summary>
    internal static async ValueTask<bool> IsValidAsync(string? input, ILogger logger, CancellationToken cancellationToken)
    {
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
            using JsonReader reader = new JsonTextReader(new StringReader(input));
            JToken? jtoken = await JToken.LoadAsync(reader, settings: null, cancellationToken);
            return jtoken is not null;
        }
        catch (JsonReaderException)
        {
            // Exception in parsing json. It likely mean the text isn't a JSON.
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid data detected.");
            return false;
        }
    }

    /// <summary>
    /// Format a string to the specified JSON format.
    /// </summary>
    internal static async Task<ResultInfo<string>> FormatAsync(
        string? input,
        Indentation indentationMode,
        bool sortProperties,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new(string.Empty, false);
        }

        try
        {
            var jsonLoadSettings = new JsonLoadSettings()
            {
                CommentHandling = CommentHandling.Ignore,
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore,
                LineInfoHandling = LineInfoHandling.Load
            };

            JToken jToken;
            using (var jsonReader = new JsonTextReader(new StringReader(input)))
            {
                jsonReader.DateParseHandling = DateParseHandling.None;
                jsonReader.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;

                jToken = await JToken.LoadAsync(jsonReader, jsonLoadSettings, cancellationToken);
            }

            if (sortProperties)
            {
                if (jToken is JObject obj)
                {
                    SortJsonPropertiesAlphabetically(obj);
                }
                else if (jToken is JArray array)
                {
                    SortJsonPropertiesAlphabetically(array);
                }
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

                jToken.WriteTo(jsonTextWriter);
            }

            return new(stringBuilder.ToString());
        }
        catch (JsonReaderException ex)
        {
            return new(ex.Message, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid JSON format '{indentationMode}'", indentationMode);
            return new(ex.Message, false);
        }
    }

    /// <summary>
    /// Get the data of a JSON object in the given JSONPath.
    /// </summary>
    internal static async Task<ResultInfo<string>> TestJsonPathAsync(string json, string jsonPath, ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(jsonPath))
        {
            return new(string.Empty, false);
        }

        try
        {
            using JsonReader reader = new JsonTextReader(new StringReader(json));
            JObject? jsonObject = await JObject.LoadAsync(reader, settings: null, cancellationToken);
            return TestJsonPath(jsonObject, jsonPath, logger, cancellationToken);
        }
        catch (Exception ex) when (ex is OperationCanceledException or JsonReaderException)
        {
            return new(ex.Message, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while testing JsonPath.");
            return new(ex.Message, false);
        }
    }

    /// <summary>
    /// Get the data of a JSON object in the given JSONPath.
    /// </summary>
    internal static ResultInfo<string> TestJsonPath(JObject jsonObject, string jsonPath, ILogger logger, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(logger);
        if (jsonObject is null || string.IsNullOrWhiteSpace(jsonPath))
        {
            return new(string.Empty, false);
        }

        try
        {
            IEnumerable<JToken> tokens = jsonObject.SelectTokens(jsonPath, errorWhenNoMatch: false);

            cancellationToken.ThrowIfCancellationRequested();
            return new(JsonConvert.SerializeObject(tokens, Formatting.Indented), true);
        }
        catch (OperationCanceledException)
        {
            return new(string.Empty, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while testing JsonPath.");
            return new(ex.Message, false);
        }
    }

    /// <summary>
    /// Convert a Yaml string to Json
    /// </summary>
    internal static ResultInfo<string> ConvertFromYaml(
        string? input,
        Indentation indentationMode,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new(string.Empty, false);
        }

        try
        {
            using var stringReader = new StringReader(input);

            IDeserializer deserializer = new DeserializerBuilder()
                .WithNodeTypeResolver(new DecimalYamlTypeResolver())
                .WithNodeTypeResolver(new BooleanYamlTypeResolver())
                .Build();

            object? yamlObject = deserializer.Deserialize(stringReader);

            if (yamlObject is null or string)
            {
                return new(string.Empty, false);
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

            cancellationToken.ThrowIfCancellationRequested();
            return new(stringBuilder.ToString());

        }
        catch (SemanticErrorException ex)
        {
            return new(ex.Message, false);
        }
        catch (OperationCanceledException)
        {
            return new(string.Empty, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Yaml to Json Converter");
            return new(string.Empty, false);
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
            else if (property.Value is JArray array)
            {
                SortJsonPropertiesAlphabetically(array);
            }
        }
    }

    private static void SortJsonPropertiesAlphabetically(JArray jArray)
    {
        foreach (JToken? arrayItem in jArray)
        {
            if (arrayItem is JObject arrayObj)
            {
                SortJsonPropertiesAlphabetically(arrayObj);
            }
            else if (arrayItem is JArray array)
            {
                SortJsonPropertiesAlphabetically(array);
            }
        }
    }
}

