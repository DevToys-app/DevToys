using Newtonsoft.Json;
using Windows.Foundation.Metadata;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.imodel.html
/// </summary>
public interface IModel
{
    // TODO: Events

    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    string Id { get; }
    [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
    Uri Uri { get; }

    //IIdentifiedSingleEditOperation[] ApplyEditsAsync(IIdentifiedSingleEditOperation[] operations)
    //DeltaDecorationsAsync
    Task DetectIndentationAsync(bool defaultInsertSpaces, bool defaultTabSize);

    /// <summary>
    /// Search the model.
    /// </summary>
    /// <returns>
    /// The ranges where the matches are. It is empty if not matches have been found.
    /// 
    /// </returns>
    [DefaultOverload]
    Task<IEnumerable<FindMatch>?> FindMatchesAsync(string searchString, bool searchOnlyEditableRange, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches);

    [DefaultOverload]
    Task<IEnumerable<FindMatch>?> FindMatchesAsync(string searchString, bool searchOnlyEditableRange, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches, double limitResultCount);

    /// <summary>
    /// Search the model.
    /// </summary>
    /// <returns>
    /// The ranges where the matches are. It is empty if no matches have been found.
    /// 
    /// </returns>
    Task<IEnumerable<FindMatch>?> FindMatchesAsync(string searchString, IRange searchScope, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches);

    Task<IEnumerable<FindMatch>?> FindMatchesAsync(string searchString, IRange searchScope, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches, double limitResultCount);

    /// <summary>
    /// Search the model for the next match. Loops to the beginning of the model if needed.
    /// </summary>
    /// <returns>
    /// The range where the next match is. It is null if no next match has been found.
    /// 
    /// </returns>
    Task<FindMatch?> FindNextMatchAsync(string searchString, IPosition searchStart, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches);

    /// <summary>
    /// Search the model for the previous match. Loops to the end of the model if needed.
    /// </summary>
    /// <returns>
    /// The range where the previous match is. It is null if no previous match has been found.
    /// 
    /// </returns>
    Task<FindMatch?> FindPreviousMatchAsync(string searchString, IPosition searchStart, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches);

    //GetAllDecorationsAsync
    Task<uint> GetAlternativeVersionIdAsync();
    //GetDecorationOptionsAsync
    //GetDecorationRangeAsync
    //GetDecorationsInRangeAsync
    Task<string?> GetEOLAsync();
    Task<Range?> GetFullModelRangeAsync();
    Task<string?> GetLineContentAsync(uint lineNumber);
    Task<uint> GetLineCountAsync();
    //GetLineDecorationsAsync
    Task<uint> GetLineFirstNonWhitespaceColumnAsync(uint lineNumber);
    Task<uint> GetLineLastNonWhitespaceColumnAsync(uint lineNumber);
    Task<uint> GetLineLengthAsync(uint lineNumber);
    Task<uint> GetLineMaxColumnAsync(uint lineNumber);
    Task<uint> GetLineMinColumnAsync(uint lineNumber);
    Task<IEnumerable<string>?> GetLinesContentAsync();
    //GetLinesDecorationsAsync
    Task<string?> GetModelIdAsync();
    Task<uint> GetOffsetAtAsync(IPosition position);
    Task<string?> GetOneIndentAsync();
    //GetOptionsAsync
    Task<Position?> GetPositionAtAsync(uint offset);
    Task<string?> GetValueAsync();
    // TextDefined is default eol
    Task<string?> GetValueAsync(EndOfLinePreference eol);
    Task<string?> GetValueAsync(EndOfLinePreference eol, bool preserveBOM);
    Task<string?> GetValueInRangeAsync(IRange range);
    Task<string?> GetValueInRangeAsync(IRange range, EndOfLinePreference eol);
    Task<uint> GetValueLengthAsync();
    Task<uint> GetValueLengthAsync(EndOfLinePreference eol);
    Task<uint> GetValueLengthAsync(EndOfLinePreference eol, bool preserveBOM);
    Task<uint> GetValueLengthInRangeAsync(IRange range);
    Task<uint> GetVersionIdAsync();
    Task<WordAtPosition?> GetWordAtPositionAsync(IPosition position);
    Task<WordAtPosition?> GetWordUntilPositionAsync(IPosition position);
    Task<Position?> ModifyPositionAsync(IPosition position, int number);
    Task<string?> NormalizeIndentationAsync(string str);
    //PushEditOperationsAsync
    Task PushStackElementAsync();
    Task SetEOLAsync(EndOfLineSequence eol);
    Task SetValue(string newValue);
    //Task UpdateOptions(ITextModelUpdateOptions newOpts);
    Task<Position?> ValidatePositionAsync(IPosition position);
    Task<Range?> ValidateRangeAsync(IRange range);
}

