using DevToys.Tools.Tools.Text.SmartCalculator.Api.Core;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Core;

[Export(typeof(IParserAndInterpretersRepository))]
internal sealed class ParserRepository : IParserAndInterpretersRepository
{
    private readonly IEnumerable<Lazy<IDataParser, CultureCodeMetadata>> _dataParsers;
    private readonly IEnumerable<Lazy<IStatementParserAndInterpreter, ParserAndInterpreterMetadata>> _statementParsersAndInterpreters;
    private readonly IEnumerable<Lazy<IExpressionParserAndInterpreter, ParserAndInterpreterMetadata>> _expressionParsersAndInterpreters;
    private readonly Dictionary<SearchQuery, IEnumerable<IDataParser>> _applicableDataParsers = new();
    private readonly Dictionary<SearchQuery, IEnumerable<IStatementParserAndInterpreter>> _applicableStatementParsersAndInterpreters = new();
    private readonly Dictionary<SearchQuery, IEnumerable<IExpressionParserAndInterpreter>> _applicableExpressionParsersAndInterpreters = new();

    [ImportingConstructor]
    public ParserRepository(
        [ImportMany] IEnumerable<Lazy<IDataParser, CultureCodeMetadata>> dataParsers,
        [ImportMany] IEnumerable<Lazy<IStatementParserAndInterpreter, ParserAndInterpreterMetadata>> statementParsersAndInterpreters,
        [ImportMany] IEnumerable<Lazy<IExpressionParserAndInterpreter, ParserAndInterpreterMetadata>> expressionParsersAndInterpreters)
    {
        _dataParsers = dataParsers;

        _statementParsersAndInterpreters = ExtensionOrderer.Order(statementParsersAndInterpreters);
        _expressionParsersAndInterpreters = ExtensionOrderer.Order(expressionParsersAndInterpreters);
    }

    public IEnumerable<IDataParser> GetApplicableDataParsers(string culture)
    {
        lock (_applicableDataParsers)
        {
            var key = new SearchQuery(culture);
            if (_applicableDataParsers.TryGetValue(key, out IEnumerable<IDataParser>? parsers) && parsers is not null)
                return parsers;

            parsers = _dataParsers.Where(
                p => p.Metadata.CultureCodes.Any(
                    c => CultureHelper.IsCultureApplicable(c, culture)))
                .Select(p => p.Value);

            _applicableDataParsers[key] = parsers;
            return parsers;
        }
    }

    public IEnumerable<IStatementParserAndInterpreter> GetApplicableStatementParsersAndInterpreters(string culture)
    {
        lock (_applicableStatementParsersAndInterpreters)
        {
            var key = new SearchQuery(culture);
            if (_applicableStatementParsersAndInterpreters.TryGetValue(key, out IEnumerable<IStatementParserAndInterpreter>? parsersAndInterpreters) && parsersAndInterpreters is not null)
                return parsersAndInterpreters;

            parsersAndInterpreters = _statementParsersAndInterpreters.Where(
                p => p.Metadata.CultureCodes.Any(
                    c => CultureHelper.IsCultureApplicable(c, culture)))
                .Select(p => p.Value);

            _applicableStatementParsersAndInterpreters[key] = parsersAndInterpreters;
            return parsersAndInterpreters;
        }
    }

    public IEnumerable<IExpressionParserAndInterpreter> GetApplicableExpressionParsersAndInterpreters(string culture)
    {
        lock (_applicableExpressionParsersAndInterpreters)
        {
            var key = new SearchQuery(culture);
            if (_applicableExpressionParsersAndInterpreters.TryGetValue(key, out IEnumerable<IExpressionParserAndInterpreter>? parsersAndInterpreters) && parsersAndInterpreters is not null)
                return parsersAndInterpreters;

            parsersAndInterpreters = _expressionParsersAndInterpreters.Where(
                p => p.Metadata.CultureCodes.Any(
                    c => CultureHelper.IsCultureApplicable(c, culture)))
                .Select(p => p.Value);

            _applicableExpressionParsersAndInterpreters[key] = parsersAndInterpreters;
            return parsersAndInterpreters;
        }
    }

    public IExpressionParserAndInterpreter[] GetExpressionParserAndInterpreters(string culture, params string[] expressionParserAndInterpreterNames)
    {
        IExpressionParserAndInterpreter[] parserAndInterpreters
            = _expressionParsersAndInterpreters
                .Where(
                    p => expressionParserAndInterpreterNames.Any(n => string.Equals(p.Metadata.InternalComponentName, n, StringComparison.Ordinal))
                        && p.Metadata.CultureCodes.Any(c => CultureHelper.IsCultureApplicable(c, culture)))
                .Select(p => p.Value)
                .ToArray();

        if (parserAndInterpreters.Length != expressionParserAndInterpreterNames.Length)
            ThrowHelper.ThrowArgumentException("One of the given parser name can't be found.");

        return parserAndInterpreters;
    }

    private struct SearchQuery : IEquatable<SearchQuery>
    {
        private readonly string _culture;

        internal SearchQuery(string culture)
        {
            _culture = culture;
        }

        public override bool Equals(object? obj)
        {
            return obj is SearchQuery query && Equals(query);
        }

        public bool Equals(SearchQuery other)
        {
            return _culture == other._culture;
        }

        public override int GetHashCode()
        {
            return -498521196 + EqualityComparer<string>.Default.GetHashCode(_culture);
        }
    }
}
