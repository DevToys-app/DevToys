using System.Collections;
using System.Text.RegularExpressions;

namespace DevToys.MauiBlazor.Components;

public class GridTemplateConverter : IEnumerable<string>
{
    private readonly List<string> _convertedData = new();
    private readonly Regex _proportionPattern = new("^[0-9]*\\*$");
    private readonly Regex _fixedSizePattern = new("^[0-9]*$");

    public void AddData(string data, string? min = null, string? max = null)
    {
        if (!string.IsNullOrWhiteSpace(min) || !string.IsNullOrWhiteSpace(max))
        {
            _convertedData.Add($"minmax({ConvertToCss(!string.IsNullOrWhiteSpace(min) ? min : "1")},{ConvertToCss(!string.IsNullOrWhiteSpace(max) ? max : "*")})");
        }
        else
        {
            _convertedData.Add(ConvertToCss(data));
        }
    }

    private string ConvertToCss(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return "1fr";
        }

        if (data == "*")
        {
            return "1fr";
        }

        if (data.ToLower() == "auto")
        {
            return "auto";
        }

        if (_fixedSizePattern.IsMatch(data))
        {
            return data + "px";
        }

        if (_proportionPattern.IsMatch(data))
        {
            return data.Replace("*", "fr");
        }

        throw new GridLayoutException(data);
    }

    public IEnumerator<string> GetEnumerator()
        => _convertedData.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
