namespace DevToys.Tools.Helpers.SqlFormatter;

internal readonly struct SqlFormatterOptions
{
    public Models.Indentation Indentation { get; }

    public bool Uppercase { get; }

    public int LinesBetweenQueries { get; }

    public IReadOnlyDictionary<string, string>? PlaceholderParameters { get; }

    public SqlFormatterOptions(
        Models.Indentation indentation,
        bool uppercase,
        int linesBetweenQueries = 1,
        IReadOnlyDictionary<string, string>? placeholderParameters = null)
    {
        Indentation = indentation;
        Uppercase = uppercase;
        LinesBetweenQueries = linesBetweenQueries;
        PlaceholderParameters = placeholderParameters;
    }
}
