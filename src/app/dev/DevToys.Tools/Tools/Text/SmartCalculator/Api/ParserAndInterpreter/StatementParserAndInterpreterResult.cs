using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

/// <summary>
/// Represents the result of a parsed and interpreted statement.
/// </summary>
[DebuggerDisplay($"ParsedStatement = {{{nameof(ParsedStatement)}}}, Error = {{{nameof(Error)}}}, ResultedData = [{{{nameof(ResultedData)}}}]")]
public record StatementParserAndInterpreterResult
{
    /// <summary>
    /// Gets or sets the resulting data out of the interpretation.
    /// </summary>
    public IData? ResultedData { get; set; } = null;

    /// <summary>
    /// Gets or sets the parsed statement.
    /// </summary>
    public Statement? ParsedStatement { get; set; } = null;

    /// <summary>
    /// Gets or sets an exception that may have been triggered.
    /// </summary>
    /// <remarks>
    /// When this property is set to a non-null value, the interpretation will stop, meaning that other parsers and
    /// interpreter won't have a chance to parse. Set this property only to indicate that the parsing should stop.
    /// </remarks>
    public DataOperationException? Error { get; set; } = null;
}
