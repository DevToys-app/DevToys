using CommunityToolkit.Common.Deferred;
using DevToys.Tools.Tools.Text.SmartCalculator.Api;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Core.Threading;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Core;

internal sealed class ParserAndInterpreter : IDisposable
{
    private readonly string _culture;
    private readonly ILogger _logger;
    private readonly Lexer _lexer;
    private readonly IParserAndInterpretersRepository _parserRepository;
    private readonly IArithmeticAndRelationOperationService _arithmeticAndRelationOperationService;
    private readonly VariableService _variableService = new();
    private readonly TextDocument _textDocument;

    private CancellationTokenSource _cancellationTokenSource = new();
    private Task _currentParsingAndInterpretationTask = Task.CompletedTask;
    private IReadOnlyList<ParserAndInterpreterResultLine>? _lineResults = null;
    private IReadOnlyList<TokenizedTextLine>? _oldTokenizedTextLines;

    internal event EventHandler<ParserAndInterpreterResultUpdatedEventArgs>? ParserAndInterpreterResultUpdated;

    internal ParserAndInterpreter(
        string culture,
        ILogger logger,
        ILexer lexer,
        IParserAndInterpretersRepository parserRepository,
        IArithmeticAndRelationOperationService arithmeticAndRelationOperationService,
        TextDocument textDocument)
    {
        Guard.IsNotNullOrWhiteSpace(culture);
        Guard.IsNotNull(logger);
        Guard.IsNotNull(lexer);
        Guard.IsNotNull(parserRepository);
        Guard.IsNotNull(arithmeticAndRelationOperationService);
        Guard.IsNotNull(textDocument);

        _culture = Culture.MapToNearestLanguage(culture);

        _logger = logger;
        _lexer = (Lexer)lexer;
        _parserRepository = parserRepository;
        _arithmeticAndRelationOperationService = arithmeticAndRelationOperationService;
        _textDocument = textDocument;
        _textDocument.TextChanged += TextDocument_TextChanged;
    }

    public void Dispose()
    {
        _textDocument.TextChanged -= TextDocument_TextChanged;
        CancelCurrentInterpretationWork();
    }

    internal async Task<IReadOnlyList<ParserAndInterpreterResultLine>?> WaitAsync()
    {
        try
        {
            await _currentParsingAndInterpretationTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Ignore.
        }

        return _lineResults;
    }

    private void TextDocument_TextChanged(object? sender, EventArgs e)
    {
        CancelCurrentInterpretationWork();
        CancellationToken cancellationToken = _cancellationTokenSource.Token;

        string newText = _textDocument.Text;
        _currentParsingAndInterpretationTask = ParseAndIntepretAsync(newText, _oldTokenizedTextLines, _lineResults, cancellationToken);
    }

    private void CancelCurrentInterpretationWork()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new();
    }

    private async Task ParseAndIntepretAsync(string text, IReadOnlyList<TokenizedTextLine>? oldTokenizedTextLines, IReadOnlyList<ParserAndInterpreterResultLine>? oldLineResults, CancellationToken cancellationToken)
    {
        // Make sure to go off the UI thread.
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var lineResults = new List<ParserAndInterpreterResultLine>();

        // Tokenize the whole document.
        IReadOnlyList<TokenizedTextLine> newTokenizedTextLines = _lexer.Tokenize(_culture, text);

        // Determine on which line a change happened.
        int lineFromWhichSomethingHasChanged;
        if (oldLineResults is null || oldLineResults.Count == 0)
            lineFromWhichSomethingHasChanged = 0;
        else
        {
            lineFromWhichSomethingHasChanged = DetermineLineFromWhichSomethingHasChanged(oldTokenizedTextLines, newTokenizedTextLines);
            for (int i = 0; i < lineFromWhichSomethingHasChanged; i++)
            {
                lineResults.Add(oldLineResults[i]);
            }
        }

        // For each line to parse and interpret.
        for (int i = lineFromWhichSomethingHasChanged; i < newTokenizedTextLines.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ParserAndInterpreterResultLine lineResult = await ParseAndInterpretLineAsync(newTokenizedTextLines[i], cancellationToken);
            lineResults.Add(lineResult);
        }

        Interlocked.Exchange(ref _oldTokenizedTextLines, newTokenizedTextLines);
        Interlocked.Exchange(ref _lineResults, lineResults);

        if (ParserAndInterpreterResultUpdated is not null && !cancellationToken.IsCancellationRequested)
            await ParserAndInterpreterResultUpdated.InvokeAsync(this, new ParserAndInterpreterResultUpdatedEventArgs(lineResults), cancellationToken);
    }

    private async Task<ParserAndInterpreterResultLine> ParseAndInterpretLineAsync(TokenizedTextLine lineToParseAndInterpret, CancellationToken cancellationToken)
    {
        // Start recording all the variable changes while interpreting the line. The changes won't be saved if the operation gets canceled.
        using (_variableService.BeginRecordVariableSnapshot(lineToParseAndInterpret.LineNumber, cancellationToken))
        {
            // Re-tokenize the line by taking consideration of the data this time.
            lineToParseAndInterpret = await TokenizeTextLineWithDataDetectionAsync(lineToParseAndInterpret, cancellationToken);
            if (lineToParseAndInterpret.Tokens is null)
                return new ParserAndInterpreterResultLine(lineToParseAndInterpret);

            cancellationToken.ThrowIfCancellationRequested();

            // Try to parse and interpret the line.
            var statementResults = new List<StatementParserAndInterpreterResult>();
            LinkedToken? nextTokenToParse = lineToParseAndInterpret.Tokens;
            while (nextTokenToParse is not null && !cancellationToken.IsCancellationRequested)
            {
                // Parse and interpret the next statement in the line.
                StatementParserAndInterpreterResult? result = await ParseAndInterpretNextStatementAsync(nextTokenToParse, cancellationToken);
                if (result is not null)
                {
                    if (result.Error is not null)
                    {
                        statementResults.Add(result);
                        break;
                    }

                    Guard.IsNotNull(result.ParsedStatement);
                    nextTokenToParse = result.ParsedStatement.LastToken.Next;
                    statementResults.Add(result);
                }
                else
                {
                    // Ignore the current token. It might be a word that we would simply skip.
                    nextTokenToParse = nextTokenToParse.Next;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (statementResults.Count > 0)
                return new ParserAndInterpreterResultLine(lineToParseAndInterpret, statementResults, statementResults[0].ResultedData);

            return new ParserAndInterpreterResultLine(lineToParseAndInterpret, statementResults);
        }
    }

    private async Task<StatementParserAndInterpreterResult?> ParseAndInterpretNextStatementAsync(LinkedToken currentToken, CancellationToken cancellationToken)
    {
        var result = new StatementParserAndInterpreterResult();

        // Get applicable statement parsers and interpreters.
        IEnumerable<IStatementParserAndInterpreter> statementParsersAndInterpreters
            = _parserRepository.GetApplicableStatementParsersAndInterpreters(_culture);

        foreach (IStatementParserAndInterpreter statementParserAndInterpreter in statementParsersAndInterpreters)
        {
            try
            {
                // Try to parse and interpret a statement.
                result = new StatementParserAndInterpreterResult();
                if (await statementParserAndInterpreter.TryParseAndInterpretStatementAsync(_culture, currentToken, _variableService, result, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (result.ParsedStatement is null)
                    {
                        ThrowHelper.ThrowInvalidDataException(
                            $"The method '{nameof(statementParserAndInterpreter.TryParseAndInterpretStatementAsync)}' returned true " +
                            $"but '{nameof(StatementParserAndInterpreterResult)}.{nameof(StatementParserAndInterpreterResult.ParsedStatement)}' is null. " +
                            $"It should not be null.");
                    }

                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore.
            }
            catch (DataOperationException doe)
            {
                result.Error = doe;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "ParseAndInterpretStatement.Fault",
                    ex,
                    ("StatementParserAndInterpreterName", statementParserAndInterpreter.GetType().FullName));
            }

            if (result.Error is not null)
                return result;

            if (cancellationToken.IsCancellationRequested)
                return null;
        }

        return null;
    }

    private async Task<TokenizedTextLine> TokenizeTextLineWithDataDetectionAsync(TokenizedTextLine tokenizedLine, CancellationToken cancellationToken)
    {
        IReadOnlyList<IData> parsedData
            = await ParseDataAsync(
                tokenizedLine,
                cancellationToken)
            .ConfigureAwait(false);

        return _lexer.TokenizeLine(
            _culture,
            tokenizedLine.LineNumber,
            tokenizedLine.Start,
            tokenizedLine.LineTextIncludingLineBreak,
            _variableService.GetVariableNames(),
            parsedData);
    }

    private async Task<IReadOnlyList<IData>> ParseDataAsync(TokenizedTextLine tokenizedLine, CancellationToken cancellationToken)
    {
        var rawDataBag = new List<IData>();
        var nonOverlappingData = new List<IData>();
        var tasks = new List<Task>();

        foreach (IDataParser dataParser in _parserRepository.GetApplicableDataParsers(_culture))
        {
            tasks.Add(
                Task.Run(
                    () =>
                    {
                        try
                        {
                            IReadOnlyList<IData>? results = dataParser.Parse(_culture, tokenizedLine, cancellationToken);
                            if (results is not null)
                            {
                                lock (rawDataBag)
                                {
                                    rawDataBag.AddRange(results);
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // Ignore.
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                "Parser.ParseData.Fault",
                                ex,
                                ("DataParserName", dataParser.GetType().FullName));
                        }
                    },
                    cancellationToken));
        }

        await Task.WhenAny(Task.WhenAll(tasks), cancellationToken.AsTask()).ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested)
            return nonOverlappingData;

        // For each data we parsed, find whether the data is overlapped by another one. If not, then we keep it.
        for (int i = 0; i < rawDataBag.Count; i++)
        {
            IData currentData = rawDataBag[i];
            if (currentData is not null)
            {
                IData? dataToAdd = SolvePotentialDataOverlapping(rawDataBag, currentData);
                if (dataToAdd is not null && !nonOverlappingData.Contains(dataToAdd))
                    nonOverlappingData.Add(dataToAdd);
            }
        }

        // Sort the non-overlapping items.
        nonOverlappingData.Sort();

        return nonOverlappingData;
    }

    private static IData? SolvePotentialDataOverlapping(IReadOnlyList<IData> allData, IData currentData)
    {
        for (int i = 0; i < allData.Count; i++)
        {
            IData data = allData[i];
            if (data is not null
                && currentData != data
                && data.StartInLine <= currentData.StartInLine
                && data.EndInLine >= currentData.EndInLine)
            {
                // Two data are overlapping.

                if (data.StartInLine == currentData.StartInLine
                    && data.EndInLine == currentData.EndInLine)
                {
                    // The two data have the exact same location in the document.
                    // It may indicate a conflict of data type. For example "1m" can be detected as "1 minute" and "1 meter".
                    // To solve the conflict:
                    // 1. If there are other data and that they have the same type than one of the 2 data we're conflicting with
                    //    then let's use that type because they may be compatible.
                    // 2. Otheriwse, let's decide of what data kind to use based on priority order.
                    IData? overlapResolution = SolveOverlappingData(currentData, data, allData);
                    overlapResolution ??= data.ConflictResolutionPriority < currentData.ConflictResolutionPriority ? data : currentData;
                    return overlapResolution;
                }

                // Overlap situation.
                return null;
            }
        }

        return currentData;
    }

    private static int DetermineLineFromWhichSomethingHasChanged(
        IReadOnlyList<TokenizedTextLine>? oldTokenizedTextLines,
        IReadOnlyList<TokenizedTextLine> newTokenizedTextLines)
    {
        Guard.IsNotNull(newTokenizedTextLines);

        if (oldTokenizedTextLines is not null)
        {
            int i = 0;
            while (i < oldTokenizedTextLines.Count && i < newTokenizedTextLines.Count)
            {
                TokenizedTextLine oldTokenizedTextLine = oldTokenizedTextLines[i];
                TokenizedTextLine tokenizedTextLine = newTokenizedTextLines[i];

                if (!string.Equals(
                    oldTokenizedTextLine.LineTextIncludingLineBreak,
                    tokenizedTextLine.LineTextIncludingLineBreak,
                    StringComparison.Ordinal))
                {
                    return i;
                }

                i++;
            }
        }

        return Math.Max(Math.Min(oldTokenizedTextLines?.Count ?? 0, newTokenizedTextLines.Count) - 1, 0);
    }

    private static IData? SolveOverlappingData(IData left, IData right, IReadOnlyList<IData> allData)
    {
        if (left.IsOfType(right.Type) && left.IsOfSubtype(right.Subtype ?? string.Empty))
            return left;

        if (allData.Count > 2)
        {
            for (int i = 0; i < allData.Count; i++)
            {
                IData data = allData[i];
                if (data != left
                    && data != right)
                {
                    if (left.IsOfSubtype(data.Subtype ?? string.Empty))
                        return left;
                    if (right.IsOfSubtype(data.Subtype ?? string.Empty))
                        return right;
                }
            }
        }

        return null;
    }
}
