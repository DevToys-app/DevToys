namespace DevToys.Tools.Helpers.SqlFormatter;

internal readonly record struct SqlFormatterOptions(
    Models.Indentation Indentation,
    bool Uppercase,
    int LinesBetweenQueries = 1,
    bool UseLeadingComma = false,
    IReadOnlyDictionary<string, string>? PlaceholderParameters = null);
