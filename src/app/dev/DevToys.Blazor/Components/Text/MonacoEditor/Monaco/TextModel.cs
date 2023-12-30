using DevToys.Blazor.Components.Monaco.Editor;

namespace DevToys.Blazor.Components.Monaco;

/// <summary>
/// A model.
/// </summary>
public class TextModel
{
    /// <summary>
    /// Gets the resource associated with this editor model.
    /// </summary>
    public string? Uri { get; set; }

    /// <summary>
    /// A unique identifier associated with this model.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Get the resolved options for this model.
    /// </summary>
    public ValueTask<TextModelResolvedOptions> GetOptionsAsync(IJSRuntime runtime)
        => runtime.InvokeAsync<TextModelResolvedOptions>("devtoys.MonacoEditor.TextModel.getOptions", Uri);

    /// <summary>
    /// Get the current version id of the model.
    /// Anytime a change happens to the model (even undo/redo),
    /// the version id is incremented.
    /// </summary>
    public ValueTask<int> GetVersionIdAsync(IJSRuntime runtime)
        => runtime.InvokeAsync<int>("devtoys.MonacoEditor.TextModel.getVersionId", Uri);

    /// <summary>
    /// Get the alternative version id of the model.
    /// This alternative version id is not always incremented,
    /// it will return the same values in the case of undo-redo.
    /// </summary>
    public ValueTask<int> GetAlternativeVersionIdAsync(IJSRuntime runtime)
        => runtime.InvokeAsync<int>("devtoys.MonacoEditor.TextModel.getAlternativeVersionId", Uri);

    /// <summary>
    /// Replace the entire text buffer value contained in this model.
    /// </summary>
    public ValueTask<bool> SetValueAsync(IJSRuntime runtime, string newValue)
        => runtime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.TextModel.setValue", Uri, newValue);

    /// <summary>
    /// Get the text stored in this model.
    /// @param eol The end of line character preference. Defaults to `EndOfLinePreference.TextDefined`.
    /// @param preserverBOM Preserve a BOM character if it was detected when the model was constructed.
    /// @return The text.
    /// </summary>
    public ValueTask<string> GetValueAsync(IJSRuntime runtime, EndOfLinePreference? eol, bool? preserveBOM)
        => runtime.InvokeAsync<string>("devtoys.MonacoEditor.TextModel.getValue", Uri, eol, preserveBOM);

    /// <summary>
    /// Get the length of the text stored in this model.
    /// </summary>
    public ValueTask<int> GetValueLengthAsync(IJSRuntime runtime, EndOfLinePreference? eol, bool? preserveBOM)
        => runtime.InvokeAsync<int>("devtoys.MonacoEditor.TextModel.getValueLength", Uri, eol, preserveBOM);

    /// <summary>
    /// Get the text in a certain range.
    /// @param range The range describing what text to get.
    /// @param eol The end of line character preference. This will only be used for multiline ranges. Defaults to `EndOfLinePreference.TextDefined`.
    /// @return The text.
    /// </summary>
    public ValueTask<string> GetValueInRangeAsync(IJSRuntime runtime, Range range, EndOfLinePreference? eol)
        => runtime.InvokeAsync<string>("devtoys.MonacoEditor.TextModel.getValueInRange", Uri, range, eol);

    /// <summary>
    /// Get the length of text in a certain range.
    /// @param range The range describing what text length to get.
    /// @return The text length.
    /// </summary>
    public ValueTask<int> GetValueLengthInRangeAsync(IJSRuntime runtime, Range range)
        => runtime.InvokeAsync<int>("devtoys.MonacoEditor.TextModel.getValueLengthInRange", Uri, range);

    /// <summary>
    /// Get the character count of text in a certain range.
    /// @param range The range describing what text length to get.
    /// </summary>
    public ValueTask<int> GetCharacterCountInRangeAsync(IJSRuntime runtime, Range range)
        => runtime.InvokeAsync<int>("devtoys.MonacoEditor.TextModel.getCharacterCountInRange", Uri, range);

    /// <summary>
    /// Get the number of lines in the model.
    /// </summary>
    public ValueTask<int> GetLineCountAsync(IJSRuntime runtime)
        => runtime.InvokeAsync<int>("devtoys.MonacoEditor.TextModel.getLineCount", Uri);

    /// <summary>
    /// Get the text for a certain line.
    /// </summary>
    public ValueTask<string> GetLineContentAsync(IJSRuntime runtime, int lineNumber)
        => runtime.InvokeAsync<string>("devtoys.MonacoEditor.TextModel.getLineContent", Uri, lineNumber);

    /// <summary>
    /// Get the text length for a certain line.
    /// </summary>
    public ValueTask<int> GetLineLengthAsync(IJSRuntime runtime, int lineNumber)
        => runtime.InvokeAsync<int>("devtoys.MonacoEditor.TextModel.getLineLength", Uri, lineNumber);

    /// <summary>
    /// Get the text for all lines.
    /// </summary>
    public ValueTask<List<string>> GetLinesContentAsync(IJSRuntime runtime)
        => runtime.InvokeAsync<List<string>>("devtoys.MonacoEditor.TextModel.getLinesContent", Uri);

    /// <summary>
    /// Get the end of line sequence predominantly used in the text buffer.
    /// @return EOL char sequence (e.g.: '\n' or '\r\n').
    /// </summary>
    public ValueTask<string> GetEOLAsync(IJSRuntime runtime)
        => runtime.InvokeAsync<string>("devtoys.MonacoEditor.TextModel.getEOL", Uri);

    /// <summary>
    /// Get the end of line sequence predominantly used in the text buffer.
    /// </summary>
    public ValueTask<EndOfLineSequence> GetEndOfLineSequenceAsync(IJSRuntime runtime)
        => runtime.InvokeAsync<EndOfLineSequence>("devtoys.MonacoEditor.TextModel.getEndOfLineSequence", Uri);

    /// <summary>
    /// Get the minimum legal column for line at `lineNumber`
    /// </summary>
    public ValueTask<int> GetLineMinColumnAsync(IJSRuntime runtime, int lineNumber)
        => runtime.InvokeAsync<int>("devtoys.MonacoEditor.TextModel.getLineMinColumn", Uri, lineNumber);

    /// <summary>
    /// Get the maximum legal column for line at `lineNumber`
    /// </summary>
    public ValueTask<int> GetLineMaxColumnAsync(IJSRuntime runtime, int lineNumber)
        => runtime.InvokeAsync<int>("devtoys.MonacoEditor.TextModel.getLineMaxColumn", Uri, lineNumber);

    /// <summary>
    /// Returns the column before the first non whitespace character for line at `lineNumber`.
    /// Returns 0 if line is empty or contains only whitespace.
    /// </summary>
    public ValueTask<int> GetLineFirstNonWhitespaceColumnAsync(IJSRuntime runtime, int lineNumber)
        => runtime.InvokeAsync<int>("devtoys.MonacoEditor.TextModel.getLineFirstNonWhitespaceColumn", Uri, lineNumber);

    /// <summary>
    /// Returns the column after the last non whitespace character for line at `lineNumber`.
    /// Returns 0 if line is empty or contains only whitespace.
    /// </summary>
    public ValueTask<int> GetLineLastNonWhitespaceColumnAsync(IJSRuntime runtime, int lineNumber)
        => runtime.InvokeAsync<int>("devtoys.MonacoEditor.TextModel.getLineLastNonWhitespaceColumn", Uri, lineNumber);

    /// <summary>
    /// Create a valid position.
    /// </summary>
    public ValueTask<Position> ValidatePositionAsync(IJSRuntime runtime, Position position)
        => runtime.InvokeAsync<Position>("devtoys.MonacoEditor.TextModel.validatePosition", Uri, position);

    /// <summary>
    /// Advances the given position by the given offset (negative offsets are also accepted)
    /// and returns it as a new valid position.
    ///
    /// If the offset and position are such that their combination goes beyond the beginning or
    /// end of the model, throws an exception.
    ///
    /// If the offset is such that the new position would be in the middle of a multi-byte
    /// line terminator, throws an exception.
    /// </summary>
    public ValueTask<Position> ModifyPositionAsync(IJSRuntime runtime, Position position, int offset)
        => runtime.InvokeAsync<Position>("devtoys.MonacoEditor.TextModel.modifyPosition", Uri, position, offset);

    /// <summary>
    /// Create a valid range.
    /// </summary>
    public ValueTask<Range> ValidateRangeAsync(IJSRuntime runtime, Range range)
        => runtime.InvokeAsync<Range>("devtoys.MonacoEditor.TextModel.validateRange", Uri, range);

    /// <summary>
    /// Converts the position to a zero-based offset.
    ///
    /// The position will be [adjusted](#TextDocument.validatePosition).
    ///
    /// @param position A position.
    /// @return A valid zero-based offset.
    /// </summary>
    public ValueTask<int> GetOffsetAtAsync(IJSRuntime runtime, Position position)
        => runtime.InvokeAsync<int>("devtoys.MonacoEditor.TextModel.getOffsetAt", Uri, position);

    /// <summary>
    /// Converts a zero-based offset to a position.
    ///
    /// @param offset A zero-based offset.
    /// @return A valid [position](#Position).
    /// </summary>
    public ValueTask<Position> GetPositionAtAsync(IJSRuntime runtime, int offset)
        => runtime.InvokeAsync<Position>("devtoys.MonacoEditor.TextModel.getPositionAt", Uri, offset);

    /// <summary>
    /// Get a range covering the entire model.
    /// </summary>
    public ValueTask<Range> GetFullModelRangeAsync(IJSRuntime runtime)
        => runtime.InvokeAsync<Range>("devtoys.MonacoEditor.TextModel.getFullModelRange", Uri);

    /// <summary>
    /// Returns if the model was disposed or not.
    /// </summary>
    public ValueTask<bool> IsDisposedAsync(IJSRuntime runtime)
        => runtime.InvokeAsync<bool>("devtoys.MonacoEditor.TextModel.isDisposed", Uri);

    /// <summary>
    /// Search the model.
    /// @param searchString The string used to search. If it is a regular expression, set `isRegex` to true.
    /// @param searchOnlyEditableRange Limit the searching to only search inside the editable range of the model.
    /// @param isRegex Used to indicate that `searchString` is a regular expression.
    /// @param matchCase Force the matching to match lower/upper case exactly.
    /// @param wordSeparators Force the matching to match entire words only. Pass null otherwise.
    /// @param captureMatches The result will contain the captured groups.
    /// @param limitResultCount Limit the number of results
    /// @return The ranges where the matches are. It is empty if not matches have been found.
    /// </summary>
    public ValueTask<List<FindMatch>> FindMatchesAsync(IJSRuntime runtime, string searchString, bool searchOnlyEditableRange, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches, int? limitResultCount)
        => runtime.InvokeAsync<List<FindMatch>>("devtoys.MonacoEditor.TextModel.findMatches", Uri, searchString, searchOnlyEditableRange, isRegex, matchCase, wordSeparators, captureMatches, limitResultCount);

    /// <summary>
    /// Search the model.
    /// @param searchString The string used to search. If it is a regular expression, set `isRegex` to true.
    /// @param searchScope Limit the searching to only search inside these ranges.
    /// @param isRegex Used to indicate that `searchString` is a regular expression.
    /// @param matchCase Force the matching to match lower/upper case exactly.
    /// @param wordSeparators Force the matching to match entire words only. Pass null otherwise.
    /// @param captureMatches The result will contain the captured groups.
    /// @param limitResultCount Limit the number of results
    /// @return The ranges where the matches are. It is empty if no matches have been found.
    /// </summary>
    public ValueTask<List<FindMatch>> FindMatchesAsync(IJSRuntime runtime, string searchString, Range searchScope, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches, int? limitResultCount)
        => runtime.InvokeAsync<List<FindMatch>>("devtoys.MonacoEditor.TextModel.findMatches", Uri, searchString, searchScope, isRegex, matchCase, wordSeparators, captureMatches, limitResultCount);

    /// <summary>
    /// Search the model for the next match. Loops to the beginning of the model if needed.
    /// @param searchString The string used to search. If it is a regular expression, set `isRegex` to true.
    /// @param searchStart Start the searching at the specified position.
    /// @param isRegex Used to indicate that `searchString` is a regular expression.
    /// @param matchCase Force the matching to match lower/upper case exactly.
    /// @param wordSeparators Force the matching to match entire words only. Pass null otherwise.
    /// @param captureMatches The result will contain the captured groups.
    /// @return The range where the next match is. It is null if no next match has been found.
    /// </summary>
    public ValueTask<FindMatch> FindNextMatchAsync(IJSRuntime runtime, string searchString, Position searchStart, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches)
        => runtime.InvokeAsync<FindMatch>("devtoys.MonacoEditor.TextModel.findNextMatch", Uri, searchString, searchStart, isRegex, matchCase, wordSeparators, captureMatches);

    /// <summary>
    /// Search the model for the previous match. Loops to the end of the model if needed.
    /// @param searchString The string used to search. If it is a regular expression, set `isRegex` to true.
    /// @param searchStart Start the searching at the specified position.
    /// @param isRegex Used to indicate that `searchString` is a regular expression.
    /// @param matchCase Force the matching to match lower/upper case exactly.
    /// @param wordSeparators Force the matching to match entire words only. Pass null otherwise.
    /// @param captureMatches The result will contain the captured groups.
    /// @return The range where the previous match is. It is null if no previous match has been found.
    /// </summary>
    public ValueTask<FindMatch> FindPreviousMatchAsync(IJSRuntime runtime, string searchString, Position searchStart, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches)
        => runtime.InvokeAsync<FindMatch>("devtoys.MonacoEditor.TextModel.findPreviousMatch", Uri, searchString, searchStart, isRegex, matchCase, wordSeparators, captureMatches);

    /// <summary>
    /// Get the language associated with this model.
    /// </summary>
    public ValueTask<string> GetLanguageIdAsync(IJSRuntime runtime)
        => runtime.InvokeAsync<string>("devtoys.MonacoEditor.TextModel.getLanguageId", Uri);

    /// <summary>
    /// Get the word under or besides `position`.
    /// @param position The position to look for a word.
    /// @return The word under or besides `position`. Might be null.
    /// </summary>
    public ValueTask<WordAtPosition> GetWordAtPositionAsync(IJSRuntime runtime, Position position)
        => runtime.InvokeAsync<WordAtPosition>("devtoys.MonacoEditor.TextModel.getWordAtPosition", Uri, position);

    /// <summary>
    /// Get the word under or besides `position` trimmed to `position`.column
    /// @param position The position to look for a word.
    /// @return The word under or besides `position`. Will never be null.
    /// </summary>
    public ValueTask<WordAtPosition> GetWordUntilPositionAsync(IJSRuntime runtime, Position position)
        => runtime.InvokeAsync<WordAtPosition>("devtoys.MonacoEditor.TextModel.getWordUntilPosition", Uri, position);

    /// <summary>
    /// Perform a minimum amount of operations, in order to transform the decorations
    /// identified by `oldDecorations` to the decorations described by `newDecorations`
    /// and returns the new identifiers associated with the resulting decorations.
    ///
    /// @param oldDecorations Array containing previous decorations identifiers.
    /// @param newDecorations Array describing what decorations should result after the call.
    /// @param ownerId Identifies the editor id in which these decorations should appear. If no `ownerId` is provided, the decorations will appear in all editors that attach this model.
    /// @return An array containing the new decorations identifiers.
    /// </summary>
    public ValueTask<string[]> DeltaDecorationsAsync(IJSRuntime runtime, string[] oldDecorations, ModelDeltaDecoration[] newDecorations, int? ownerId)
        => runtime.InvokeAsync<string[]>("devtoys.MonacoEditor.TextModel.deltaDecorations", Uri, oldDecorations, newDecorations.PrepareJsInterop(), ownerId);

    /// <summary>
    /// Get the options associated with a decoration.
    /// @param id The decoration id.
    /// @return The decoration options or null if the decoration was not found.
    /// </summary>
    public ValueTask<ModelDecorationOptions> GetDecorationOptionsAsync(IJSRuntime runtime, string id)
        => runtime.InvokeAsync<ModelDecorationOptions>("devtoys.MonacoEditor.TextModel.getDecorationOptions", Uri, id);

    /// <summary>
    /// Get the range associated with a decoration.
    /// @param id The decoration id.
    /// @return The decoration range or null if the decoration was not found.
    /// </summary>
    public ValueTask<Range> GetDecorationRangeAsync(IJSRuntime runtime, string id)
        => runtime.InvokeAsync<Range>("devtoys.MonacoEditor.TextModel.getDecorationRange", Uri, id);

    /// <summary>
    /// Gets all the decorations for the line `lineNumber` as an array.
    /// @param lineNumber The line number
    /// @param ownerId If set, it will ignore decorations belonging to other owners.
    /// @param filterOutValidation If set, it will ignore decorations specific to validation (i.e. warnings, errors).
    /// @return An array with the decorations
    /// </summary>
    public ValueTask<ModelDecoration> GetLineDecorationsAsync(IJSRuntime runtime, int lineNumber, int? ownerId, bool? filterOutValidation)
        => runtime.InvokeAsync<ModelDecoration>("devtoys.MonacoEditor.TextModel.getLineDecorations", Uri, lineNumber, ownerId, filterOutValidation);

    /// <summary>
    /// Gets all the decorations for the lines between `startLineNumber` and `endLineNumber` as an array.
    /// @param startLineNumber The start line number
    /// @param endLineNumber The end line number
    /// @param ownerId If set, it will ignore decorations belonging to other owners.
    /// @param filterOutValidation If set, it will ignore decorations specific to validation (i.e. warnings, errors).
    /// @return An array with the decorations
    /// </summary>
    public ValueTask<ModelDecoration> GetLinesDecorationsAsync(IJSRuntime runtime, int startLineNumber, int endLineNumber, int? ownerId, bool? filterOutValidation)
        => runtime.InvokeAsync<ModelDecoration>("devtoys.MonacoEditor.TextModel.getLinesDecorations", Uri, startLineNumber, endLineNumber, ownerId, filterOutValidation);

    /// <summary>
    /// Gets all the decorations in a range as an array. Only `startLineNumber` and `endLineNumber` from `range` are used for filtering.
    /// So for now it returns all the decorations on the same line as `range`.
    /// @param range The range to search in
    /// @param ownerId If set, it will ignore decorations belonging to other owners.
    /// @param filterOutValidation If set, it will ignore decorations specific to validation (i.e. warnings, errors).
    /// @return An array with the decorations
    /// </summary>
    public ValueTask<ModelDecoration[]> GetDecorationsInRangeAsync(IJSRuntime runtime, Range range, int? ownerId, bool? filterOutValidation)
        => runtime.InvokeAsync<ModelDecoration[]>("devtoys.MonacoEditor.TextModel.getDecorationsInRange", Uri, range, ownerId, filterOutValidation);

    /// <summary>
    /// Gets all the decorations as an array.
    /// @param ownerId If set, it will ignore decorations belonging to other owners.
    /// @param filterOutValidation If set, it will ignore decorations specific to validation (i.e. warnings, errors).
    /// </summary>
    public ValueTask<List<ModelDecoration>> GetAllDecorationsAsync(IJSRuntime runtime, int? ownerId, bool? filterOutValidation)
        => runtime.InvokeAsync<List<ModelDecoration>>("devtoys.MonacoEditor.TextModel.getAllDecorations", Uri, ownerId, filterOutValidation);

    /// <summary>
    /// Gets all the decorations that should be rendered in the overview ruler as an array.
    /// @param ownerId If set, it will ignore decorations belonging to other owners.
    /// @param filterOutValidation If set, it will ignore decorations specific to validation (i.e. warnings, errors).
    /// </summary>
    public ValueTask<ModelDecoration> GetOverviewRulerDecorationsAsync(IJSRuntime runtime, int? ownerId, bool? filterOutValidation)
        => runtime.InvokeAsync<ModelDecoration>("devtoys.MonacoEditor.TextModel.getOverviewRulerDecorations", Uri, ownerId, filterOutValidation);

    /// <summary>
    /// Gets all the decorations that contain injected text.
    /// @param ownerId If set, it will ignore decorations belonging to other owners.
    /// </summary>
    public ValueTask<ModelDecoration[]> GetInjectedTextDecorationsAsync(IJSRuntime runtime, int? ownerId)
        => runtime.InvokeAsync<ModelDecoration[]>("devtoys.MonacoEditor.TextModel.getInjectedTextDecorations", Uri, ownerId);

    /// <summary>
    /// Normalize a string containing whitespace according to indentation rules (converts to spaces or to tabs).
    /// </summary>
    public ValueTask<string> NormalizeIndentationAsync(IJSRuntime runtime, string str)
        => runtime.InvokeAsync<string>("devtoys.MonacoEditor.TextModel.normalizeIndentation", Uri, str);

    /// <summary>
    /// Change the options of this model.
    /// </summary>
    public ValueTask<bool> UpdateOptionsAsync(IJSRuntime runtime, TextModelUpdateOptions newOptions)
        => runtime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.TextModel.updateOptions", Uri, newOptions);

    /// <summary>
    /// Detect the indentation options for this model from its content.
    /// </summary>
    public ValueTask<bool> DetectIndentationAsync(IJSRuntime runtime, bool defaultInsertSpaces, int defaultTabSize)
        => runtime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.TextModel.detectIndentation", Uri, defaultInsertSpaces, defaultTabSize);

    /// <summary>
    /// Close the current undo-redo element.
    /// This offers a way to create an undo/redo stop point.
    /// </summary>
    public ValueTask<bool> PushStackElementAsync(IJSRuntime runtime)
        => runtime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.TextModel.pushStackElement", Uri);

    /// <summary>
    /// Open the current undo-redo element.
    /// This offers a way to remove the current undo/redo stop point.
    /// </summary>
    public ValueTask<bool> PopStackElementAsync(IJSRuntime runtime)
        => runtime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.TextModel.popStackElement", Uri);

    /// <summary>
    /// Change the end of line sequence. This is the preferred way of
    /// changing the eol sequence. This will land on the undo stack.
    /// </summary>
    public ValueTask<bool> PushEOLAsync(IJSRuntime runtime, EndOfLineSequence eol)
        => runtime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.TextModel.pushEOL", Uri, eol);

    /// <summary>
    /// Edit the model without adding the edits to the undo stack.
    /// This can have dire consequences on the undo stack! See @pushEditOperations for the preferred way.
    /// @param operations The edit operations.
    /// @return If desired, the inverse edit operations, that, when applied, will bring the model back to the previous state.
    /// </summary>
    public ValueTask<List<ValidEditOperation>> ApplyEditsAsync(IJSRuntime runtime, List<IdentifiedSingleEditOperation> operations, bool computeUndoEdits = false)
        => runtime.InvokeAsync<List<ValidEditOperation>>("devtoys.MonacoEditor.TextModel.applyEdits", Uri, operations, computeUndoEdits);

    /// <summary>
    /// Change the end of line sequence without recording in the undo stack.
    /// This can have dire consequences on the undo stack! See @pushEOL for the preferred way.
    /// </summary>
    public ValueTask<bool> SetEOLAsync(IJSRuntime runtime, EndOfLineSequence eol)
        => runtime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.TextModel.setEOL", Uri, eol);

    /// <summary>
    /// Destroy this model.
    /// </summary>
    public ValueTask<bool> DisposeModelAsync(IJSRuntime runtime)
        => runtime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditor.TextModel.dispose", Uri);
}
