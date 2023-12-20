using System.Data;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DevToys.Tools.Helpers;

internal static class JsonTableHelper
{
    internal static ConvertResult ConvertFromJson(string? text, CopyFormat? format, CancellationToken cancellationToken)
    {
        JObject[]? array = ParseJsonArray(text);
        if (array == null)
        {
            return new(null, "", ConvertResultError.NotJsonArray);
        }

        JObject[] flattened = array.Select(o => FlattenJsonObject(o)).ToArray();

        string[] properties = flattened
            .SelectMany(o => o.Properties())
            .Select(p => p.Name)
            .Distinct()
            .ToArray();

        if (properties.Length == 0)
        {
            return new(null, "", ConvertResultError.NoProperties);
        }

        var table = new DataGridContents(properties, new());

        StringBuilder? clipboard = null;
        char separator = '\0';
        if (format.HasValue)
        {
            separator = format switch
            {
                CopyFormat.TSV => '\t',
                CopyFormat.CSV => ',',
                CopyFormat.FSV => ';',
                _ => throw new NotSupportedException($"Unhandled {nameof(CopyFormat)}: {format}"),
            };
            clipboard = new StringBuilder();
            clipboard.AppendLine(string.Join(separator, properties));
        }

        foreach (JObject obj in flattened)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string[] values = properties
                .Select(p => obj[p]?.ToString() ?? "") // JObject indexer conveniently returns null for unknown properties
                .ToArray();

            table.Rows.Add(values);

            if (format.HasValue)
            {
                clipboard!.AppendLine(string.Join(separator, values));
            }
        }

        return new(table, clipboard?.ToString() ?? string.Empty, ConvertResultError.None);
    }

    internal record ConvertResult(DataGridContents? Data, string Text, ConvertResultError Error);

    internal record DataGridContents(string[] Headings, List<string[]> Rows);

    internal enum ConvertResultError { None, NotJsonArray, NoProperties }

    internal enum CopyFormat
    {
        /// <summary>
        /// Tab separated values
        /// </summary>
        TSV,

        /// <summary>
        /// Comma separated values
        /// </summary>
        CSV,

        /// <summary>
        /// Semicolon separated values (CSV French)
        /// </summary>
        FSV,
    }

    /// <summary>
    /// Parse the text to an array of JObject, or null if the text does not represent a JSON array of objects.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static JObject[]? ParseJsonArray(string? text)
    {
        try
        {
            // Coalesce to empty string to prevent ArgumentNullException (returns null instead).
            var array = JsonConvert.DeserializeObject(text ?? "") as JArray;
            return array?.Cast<JObject>().ToArray();
        }
        catch (JsonException)
        {
            return null;
        }
        catch (InvalidCastException)
        {
            return null;
        }
    }

    internal static JObject FlattenJsonObject(JObject json)
    {
        var flattened = new JObject();

        foreach (KeyValuePair<string, JToken?> kv in json)
        {
            if (kv.Value is JObject jobj)
            {
                // Flatten objects by prefixing their property names with the parent property name, underscore separated.
                foreach (KeyValuePair<string, JToken?> kv2 in FlattenJsonObject(jobj))
                {
                    flattened.Add($"{kv.Key}_{kv2.Key}", kv2.Value);
                }
            }
            else if (kv.Value is JValue)
            {
                flattened[kv.Key] = kv.Value;
            }
            // else strip out any array values
        }

        return flattened;
    }
}
