#nullable enable

using System.Collections.Generic;

namespace DevToys.Helpers.SqlFormatter
{
    internal struct SqlFormatterOptions
    {
        public int IndentationSize { get; }

        public bool Uppercase { get; }

        public int LinesBetweenQueries { get; }

        public IReadOnlyDictionary<string, string>? PlaceholderParameters { get; }

        public SqlFormatterOptions(
            int indentationSize,
            bool uppercase,
            int linesBetweenQueries = 1,
            IReadOnlyDictionary<string, string>? placeholderParameters = null)
        {
            IndentationSize = indentationSize;
            Uppercase = uppercase;
            LinesBetweenQueries = linesBetweenQueries;
            PlaceholderParameters = placeholderParameters;
        }
    }
}
