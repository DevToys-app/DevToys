#nullable enable

using DevToys.MonacoEditor.Monaco.Editor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace DevToys.MonacoEditor.Monaco
{
    /// <summary>
    /// Helper to access IModel interface methods off of ICodeEditor object.
    /// https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.imodel.html
    /// https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.itextmodel.html
    /// </summary>
    public sealed class ModelHelper : IModel
    {
        private readonly WeakReference<ICodeEditor> _editor;

        public ModelHelper(ICodeEditor editor)
        {
            _editor = new WeakReference<ICodeEditor>(editor);
        }

        public string Id => throw new NotImplementedException();

        public Uri Uri => throw new NotImplementedException();

        public IAsyncAction? DetectIndentationAsync(bool defaultInsertSpaces, bool defaultTabSize)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.InvokeScriptAsync("model.detectIndentationAsync", new object[] { defaultInsertSpaces, defaultTabSize }).AsAsyncAction();
            }

            return null;
        }

        public IAsyncOperation<uint>? GetAlternativeVersionIdAsync()
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<uint>("model.getAlternativeVersionId();").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<string?>? GetEOLAsync()
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<string>("model.getEOL();").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<Range?>? GetFullModelRangeAsync()
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<Range>("model.getFullModelRange();").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<string?>? GetLineContentAsync(uint lineNumber)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<string>("model.getLineContent(" + lineNumber + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<uint>? GetLineCountAsync()
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<uint>("model.getLineCount();").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<uint>? GetLineFirstNonWhitespaceColumnAsync(uint lineNumber)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<uint>("model.getLineFirstNonWhitespaceColumn(" + lineNumber + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<uint>? GetLineLastNonWhitespaceColumnAsync(uint lineNumber)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<uint>("model.getLineLastNonWhitespaceColumn(" + lineNumber + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<uint>? GetLineLengthAsync(uint lineNumber)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<uint>("model.getLineLength(" + lineNumber + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<uint>? GetLineMaxColumnAsync(uint lineNumber)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<uint>("model.getLineMaxColumn(" + lineNumber + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<uint>? GetLineMinColumnAsync(uint lineNumber)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<uint>("model.getLineMinColumn(" + lineNumber + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<IEnumerable<string>?>? GetLinesContentAsync()
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<IEnumerable<string>>("model.getLinesContent();").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<string?>? GetModelIdAsync()
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<string>("model.getModelId();").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<uint>? GetOffsetAtAsync(IPosition position)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<uint>("model.getOffsetAt(" + JsonConvert.SerializeObject(position) + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<string?>? GetOneIndentAsync()
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<string>("model.getOneIndent();").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<Position?>? GetPositionAtAsync(uint offset)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<Position>("model.getPositionAt(" + offset + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<string?>? GetValueAsync()
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<string>("model.getValue();").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<string?>? GetValueAsync(EndOfLinePreference eol)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<string?>? GetValueAsync(EndOfLinePreference eol, bool preserveBOM)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<string?>? GetValueInRangeAsync(IRange range)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<string>("model.getValueInRange(" + JsonConvert.SerializeObject(range) + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<string?>? GetValueInRangeAsync(IRange range, EndOfLinePreference eol)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<uint>? GetValueLengthAsync()
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<uint>("model.getValueLength();").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<uint>? GetValueLengthAsync(EndOfLinePreference eol)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<uint>? GetValueLengthAsync(EndOfLinePreference eol, bool preserveBOM)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<uint>? GetValueLengthInRangeAsync(IRange range)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<uint>("model.getValueLengthInRange(" + JsonConvert.SerializeObject(range) + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<uint>? GetVersionIdAsync()
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<uint>("model.getVersionId();").AsAsyncOperation();
            }

            return null;
        }

        // TODO: Need to investigate why with .NET Native the InterfaceToClassConverter isn't working anymore?
        public IAsyncOperation<WordAtPosition?>? GetWordAtPositionAsync(IPosition position)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<WordAtPosition>("model.getWordAtPosition(" + JsonConvert.SerializeObject(position) + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<WordAtPosition?>? GetWordUntilPositionAsync(IPosition position)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<WordAtPosition>("model.getWordUntilPosition(" + JsonConvert.SerializeObject(position) + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<Position?>? ModifyPositionAsync(IPosition position, int number)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<Position>("model.modifyPosition(" + JsonConvert.SerializeObject(position) + ", " + number + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<string?>? NormalizeIndentationAsync(string str)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<string>("model.normalizeIndentations(JSON.parse(" + JsonConvert.ToString(str) + "));").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncAction? PushStackElementAsync()
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync("model.pushStackElement();").AsAsyncAction();
            }

            return null;
        }

        public IAsyncAction? SetEOLAsync(EndOfLineSequence eol)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction? SetValue(string newValue)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync("model.setValue(JSON.parse(" + JsonConvert.ToString(newValue) + "));").AsAsyncAction();
            }

            return null;
        }

        public IAsyncOperation<Position?>? ValidatePositionAsync(IPosition position)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<Position?>("model.validatePosition(" + JsonConvert.SerializeObject(position) + ");").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncOperation<Range?>? ValidateRangeAsync(IRange range)
        {
            if (_editor.TryGetTarget(out ICodeEditor editor))
            {
                return editor.SendScriptAsync<Range?>("model.validateRange(" + JsonConvert.SerializeObject(range) + ");").AsAsyncOperation();
            }

            return null;
        }
    }
}
