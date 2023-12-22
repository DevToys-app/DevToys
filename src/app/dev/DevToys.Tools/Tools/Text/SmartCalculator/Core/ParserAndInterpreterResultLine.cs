using DevToys.Tools.Tools.Text.SmartCalculator.Api;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Core;

internal sealed class ParserAndInterpreterResultLine
{
    internal TokenizedTextLine TokenizedTextLine { get; }

    internal IReadOnlyList<StatementParserAndInterpreterResult> StatementsAndData { get; }

    internal IData? SummarizedResultData { get; }

    internal ParserAndInterpreterResultLine(
        TokenizedTextLine tokenizedTextLine,
        IReadOnlyList<StatementParserAndInterpreterResult>? statementsAndData = null,
        IData? summarizedResultData = null)
    {
        Guard.IsNotNull(tokenizedTextLine);
        TokenizedTextLine = tokenizedTextLine;
        StatementsAndData = statementsAndData ?? new List<StatementParserAndInterpreterResult>();

        DataOperationException? dataOperationException = StatementsAndData.LastOrDefault(s => s.Error is not null)?.Error;
        if (dataOperationException is not null)
            SummarizedResultData = new ErrorData(dataOperationException);
        else
        {
            SummarizedResultData = summarizedResultData;
        }
    }

    private class ErrorData : IData
    {
        private readonly DataOperationException _dataOperationException;

        public string? Subtype => throw new NotImplementedException();

        public string Type => PredefinedTokenAndDataTypeNames.Error;

        public int StartInLine => throw new NotImplementedException();

        public int EndInLine => throw new NotImplementedException();

        public int Length => throw new NotImplementedException();

        public string LineTextIncludingLineBreak => throw new NotImplementedException();

        public int ConflictResolutionPriority => throw new NotImplementedException();

        public ErrorData(DataOperationException dataOperationException)
        {
            _dataOperationException = dataOperationException;
        }

        public int CompareTo(IData? other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IData? other)
        {
            throw new NotImplementedException();
        }

        public string GetDisplayText(string culture)
        {
            return _dataOperationException.GetLocalizedMessage(culture);
        }

        public string GetText()
        {
            throw new NotImplementedException();
        }

        public string GetText(int startInLine, int endInLine)
        {
            throw new NotImplementedException();
        }

        public bool Is(string expectedType, string expectedTokenText, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            throw new NotImplementedException();
        }

        public bool Is(string expectedType, string[] expectedTokenText, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            throw new NotImplementedException();
        }

        public bool IsOfSubtype(string expectedSubtype)
        {
            throw new NotImplementedException();
        }

        public bool IsNotOfType(string type)
        {
            return !IsOfType(type);
        }

        public bool IsOfType(string expectedType)
        {
            return string.Equals(Type, expectedType, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsTokenTextEqualTo(string compareTo, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            throw new NotImplementedException();
        }

        public IData MergeDataLocations(IData otherData)
        {
            throw new NotImplementedException();
        }
    }
}
