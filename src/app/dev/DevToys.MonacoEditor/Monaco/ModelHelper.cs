using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// Helper to access IModel interface methods off of CodeEditor object.
/// https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.imodel.html
/// https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.itextmodel.html
/// </summary>
public sealed class ModelHelper : IModel
{
    private readonly WeakReference<CodeEditor> _editor;

    public ModelHelper(CodeEditor editor)
    {
        _editor = new WeakReference<CodeEditor>(editor);
    }

    public string Id => throw new NotImplementedException();

    public Uri Uri => throw new NotImplementedException();

    public Task DetectIndentationAsync(bool defaultInsertSpaces, bool defaultTabSize)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.InvokeScriptAsync("editorContext.model.detectIndentationAsync", new object[] { defaultInsertSpaces, defaultTabSize });
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<FindMatch>?> FindMatchesAsync(string searchString, bool searchOnlyEditableRange, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches)
    {
        // Default limit results: https://github.com/microsoft/vscode/blob/b2d0292a20c4a012005c94975019a5b572ce6a63/src/vs/editor/common/model/textModel.ts#L117
        return FindMatchesAsync(searchString, searchOnlyEditableRange, isRegex, matchCase, wordSeparators, captureMatches, 999);
    }

    public Task<IEnumerable<FindMatch>?> FindMatchesAsync(string searchString, IRange searchScope, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches)
    {
        // Default limit results: https://github.com/microsoft/vscode/blob/b2d0292a20c4a012005c94975019a5b572ce6a63/src/vs/editor/common/model/textModel.ts#L117
        return FindMatchesAsync(searchString, searchScope, isRegex, matchCase, wordSeparators, captureMatches, 999);
    }

    public Task<IEnumerable<FindMatch>?> FindMatchesAsync(string searchString, bool searchOnlyEditableRange, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches, double limitResultCount)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.InvokeScriptAsync<IEnumerable<FindMatch>>("editorContext.model.findMatches", new object[] { searchString, searchOnlyEditableRange, isRegex, matchCase, wordSeparators, captureMatches, limitResultCount });
        }

        return Task.FromResult<IEnumerable<FindMatch>?>(null);
    }

    public Task<IEnumerable<FindMatch>?> FindMatchesAsync(string searchString, IRange searchScope, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches, double limitResultCount)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.InvokeScriptAsync<IEnumerable<FindMatch>>("editorContext.model.findMatches", new object[] { searchString, searchScope, isRegex, matchCase, wordSeparators, captureMatches, limitResultCount });
        }

        return Task.FromResult<IEnumerable<FindMatch>?>(null);
    }

    public Task<FindMatch?> FindNextMatchAsync(string searchString, IPosition searchStart, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.InvokeScriptAsync<FindMatch>("editorContext.model.findNextMatch", new object[] { searchString, searchString, isRegex, matchCase, wordSeparators, captureMatches });
        }

        return Task.FromResult<FindMatch?>(null);
    }

    public Task<FindMatch?> FindPreviousMatchAsync(string searchString, IPosition searchStart, bool isRegex, bool matchCase, string wordSeparators, bool captureMatches)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.InvokeScriptAsync<FindMatch>("editorContext.model.findPreviousMatch", new object[] { searchString, searchString, isRegex, matchCase, wordSeparators, captureMatches });
        }

        return Task.FromResult<FindMatch?>(null);
    }

    public Task<uint> GetAlternativeVersionIdAsync()
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<uint>("editorContext.model.getAlternativeVersionId();");
        }

        return Task.FromResult<uint>(0);
    }

    public Task<string?> GetEOLAsync()
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<string>("editorContext.model.getEOL();");
        }

        return Task.FromResult<string?>(null);
    }

    public Task<Range?> GetFullModelRangeAsync()
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<Range>("editorContext.model.getFullModelRange();");
        }

        return Task.FromResult<Range?>(null);
    }

    public Task<string?> GetLineContentAsync(uint lineNumber)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<string>("editorContext.model.getLineContent(" + lineNumber + ");");
        }

        return Task.FromResult<string?>(null);
    }

    public Task<uint> GetLineCountAsync()
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<uint>("editorContext.model.getLineCount();");
        }

        return Task.FromResult<uint>(0);
    }

    public Task<uint> GetLineFirstNonWhitespaceColumnAsync(uint lineNumber)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<uint>("editorContext.model.getLineFirstNonWhitespaceColumn(" + lineNumber + ");");
        }

        return Task.FromResult<uint>(0);
    }

    public Task<uint> GetLineLastNonWhitespaceColumnAsync(uint lineNumber)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<uint>("editorContext.model.getLineLastNonWhitespaceColumn(" + lineNumber + ");");
        }

        return Task.FromResult<uint>(0);
    }

    public Task<uint> GetLineLengthAsync(uint lineNumber)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<uint>("editorContext.model.getLineLength(" + lineNumber + ");");
        }

        return Task.FromResult<uint>(0);
    }

    public Task<uint> GetLineMaxColumnAsync(uint lineNumber)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<uint>("editorContext.model.getLineMaxColumn(" + lineNumber + ");");
        }

        return Task.FromResult<uint>(0);
    }

    public Task<uint> GetLineMinColumnAsync(uint lineNumber)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<uint>("editorContext.model.getLineMinColumn(" + lineNumber + ");");
        }

        return Task.FromResult<uint>(0);
    }

    public Task<IEnumerable<string>?> GetLinesContentAsync()
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<IEnumerable<string>>("editorContext.model.getLinesContent();");
        }

        return Task.FromResult<IEnumerable<string>?>(null);
    }

    public Task<string?> GetModelIdAsync()
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<string>("editorContext.model.getModelId();");
        }

        return Task.FromResult<string?>(null);
    }

    public Task<uint> GetOffsetAtAsync(IPosition position)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<uint>("editorContext.model.getOffsetAt(" + JsonConvert.SerializeObject(position) + ");");
        }

        return Task.FromResult<uint>(0);
    }

    public Task<string?> GetOneIndentAsync()
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<string>("editorContext.model.getOneIndent();");
        }

        return Task.FromResult<string?>(null);
    }

    public Task<Position?> GetPositionAtAsync(uint offset)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<Position>("editorContext.model.getPositionAt(" + offset + ");");
        }

        return Task.FromResult<Position?>(null);
    }

    public Task<string?> GetValueAsync()
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<string>("editorContext.model.getValue();");
        }

        return Task.FromResult<string?>(null);
    }

    public Task<string?> GetValueAsync(EndOfLinePreference eol)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetValueAsync(EndOfLinePreference eol, bool preserveBOM)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetValueInRangeAsync(IRange range)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<string>("editorContext.model.getValueInRange(" + JsonConvert.SerializeObject(range) + ");");
        }

        return Task.FromResult<string?>(null);
    }

    public Task<string?> GetValueInRangeAsync(IRange range, EndOfLinePreference eol)
    {
        throw new NotImplementedException();
    }

    public Task<uint> GetValueLengthAsync()
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<uint>("editorContext.model.getValueLength();");
        }

        return Task.FromResult<uint>(0);
    }

    public Task<uint> GetValueLengthAsync(EndOfLinePreference eol)
    {
        throw new NotImplementedException();
    }

    public Task<uint> GetValueLengthAsync(EndOfLinePreference eol, bool preserveBOM)
    {
        throw new NotImplementedException();
    }

    public Task<uint> GetValueLengthInRangeAsync(IRange range)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<uint>("editorContext.model.getValueLengthInRange(" + JsonConvert.SerializeObject(range) + ");");
        }

        return Task.FromResult<uint>(0);
    }

    public Task<uint> GetVersionIdAsync()
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<uint>("editorContext.model.getVersionId();");
        }

        return Task.FromResult<uint>(0);
    }

    // TODO: Need to investigate why with .NET Native the InterfaceToClassConverter isn't working anymore?
    public Task<WordAtPosition?> GetWordAtPositionAsync(IPosition position)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<WordAtPosition>("editorContext.model.getWordAtPosition(" + JsonConvert.SerializeObject(position) + ");");
        }

        return Task.FromResult<WordAtPosition?>(null);
    }

    public Task<WordAtPosition?> GetWordUntilPositionAsync(IPosition position)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<WordAtPosition>("editorContext.model.getWordUntilPosition(" + JsonConvert.SerializeObject(position) + ");");
        }

        return Task.FromResult<WordAtPosition?>(null);
    }

    public Task<Position?> ModifyPositionAsync(IPosition position, int number)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<Position>("editorContext.model.modifyPosition(" + JsonConvert.SerializeObject(position) + ", " + number + ");");
        }

        return Task.FromResult<Position?>(null);
    }

    public Task<string?> NormalizeIndentationAsync(string str)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<string>("editorContext.model.normalizeIndentations(JSON.parse(" + JsonConvert.ToString(str) + "));");
        }

        return Task.FromResult<string?>(null);
    }

    public Task PushStackElementAsync()
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync("editorContext.model.pushStackElement();");
        }

        return Task.CompletedTask;
    }

    public Task SetEOLAsync(EndOfLineSequence eol)
    {
        throw new NotImplementedException();
    }

    public Task SetValue(string newValue)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync("editorContext.model.setValue(JSON.parse(" + JsonConvert.ToString(newValue) + "));");
        }

        return Task.CompletedTask;
    }

    public Task<Position?> ValidatePositionAsync(IPosition position)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<Position>("editorContext.model.validatePosition(" + JsonConvert.SerializeObject(position) + ");");
        }

        return Task.FromResult<Position?>(null);
    }

    public Task<Range?> ValidateRangeAsync(IRange range)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<Range>("editorContext.model.validateRange(" + JsonConvert.SerializeObject(range) + ");");
        }

        return Task.FromResult<Range?>(null);
    }
}

