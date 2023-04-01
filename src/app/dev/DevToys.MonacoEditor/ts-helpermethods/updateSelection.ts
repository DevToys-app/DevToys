///<reference path="../monaco-editor/monaco.d.ts" />

// link:CodeEditor.Properties.cs:updateSelectedSpan
const updateSelectedSpan = function (element: any, span: any) {
    var editorContext = EditorContext.getEditorForElement(element).htmlElement;

    var startPosition = editorContext.model.getPositionAt(span.startPosition);
    var endPosition = editorContext.model.getPositionAt(span.startPosition + span.length);
    var newSelection = monaco.Selection.fromPositions(startPosition, endPosition);

    editorContext.editor.setSelection(newSelection);
};