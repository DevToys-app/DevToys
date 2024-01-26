using System.Text.Json;
using System.Text.Json.Nodes;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace DevToys.Tools.Helpers;

internal static partial class YamlHelper
{
    /// <summary>
    /// Detects whether the given string is a valid YAML or not.
    /// </summary>
    internal static bool IsValid(string? input, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        input = input!.Trim();

        if (long.TryParse(input, out _))
        {
            return false;
        }

        /// We check if the first and last characters are the ones that are expected for JSON.
        /// TODO: check if we can use a YamlDotNet in strict mode to not parse JSON.
        int length = input.Length - 1;
        if ((input[0].Equals('{') && input[length].Equals('}')) ||
            (input[0].Equals('[') && input[length].Equals(']')))
        {
            return false;
        }

        try
        {
            object? result = new DeserializerBuilder().Build().Deserialize<object>(input);
            return result is not null and not string;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid data detected.");
            return false;
        }
    }

    /// <summary>
    /// Convert a Json string to Yaml
    /// </summary>
    internal static ResultInfo<string> ConvertFromJson(
        string? input,
        Indentation indentation,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new(string.Empty, false);
        }

        try
        {

            var token = JsonNode.Parse(input, documentOptions: new() { CommentHandling = JsonCommentHandling.Skip });
            if (token is null)
            {
                return new(string.Empty, false);
            }

            object? jsonObject = ConvertJTokenToObject(token, 0);
            cancellationToken.ThrowIfCancellationRequested();

            if (jsonObject is not null and not string)
            {
                int indent = 0;
                indent = indentation switch
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
                    return new(string.Empty, false);
                }
                cancellationToken.ThrowIfCancellationRequested();
                return new(yaml);
            }
            return new(string.Empty, false);
        }
        catch (JsonException ex)
        {
            return new(ex.Message, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Yaml to Json Converter");
            return new(string.Empty, false);
        }
    }

    private static dynamic? ParseValue(JsonValue token)
    {
        var elem = (JsonElement)token.GetValue<object>();
        return elem.ValueKind switch
        {
            JsonValueKind.String => elem.GetString(),
            JsonValueKind.Number => elem.GetDecimal(),
            JsonValueKind.False => false,
            JsonValueKind.True => true,
            JsonValueKind.Null => null,
            JsonValueKind.Undefined or
            JsonValueKind.Object or
            JsonValueKind.Array or
            _ => throw new NotSupportedException(),
        };
    }

    private static object? ConvertJTokenToObject(JsonNode? node, int level)
    {
        if (node is null)
        {
            return null;
        }

        if (level > 10)
        {
            throw new InvalidDataException($"Json structure is not supported: nested level in array is too deep. ({level}).");
        }
        return node switch
        {
            JsonValue val => ParseValue(val),
            JsonArray arr => arr.Select(o => ConvertJTokenToObject(o, level + 1)).ToList(),
            JsonObject obj => obj.AsObject().ToDictionary(x => x.Key, x => ConvertJTokenToObject(x.Value, level)),
            null => null,
            _ => throw new InvalidOperationException("Unexpected token: " + node)
        };
    }
}
