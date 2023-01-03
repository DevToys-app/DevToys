///<reference path="../monaco-editor/monaco.d.ts" />

// link:CodeEditor.Properties.cs:updateSelectedContent
const updateSelectedContent = function (element: any, content) {
    var editorContext = EditorContext.getEditorForElement(element);

    let selection = editorContext.editor.getSelection();

    // Need to ignore updates from us notifying of a change
    if (content != editorContext.model.getValueInRange(selection)) {
        editorContext.modifingSelection = true;
        let range = new monaco.Range(selection.startLineNumber, selection.startColumn, selection.endLineNumber, selection.endColumn);
        let op = { identifier: { major: 1, minor: 1 }, range, text: content, forceMoveMarkers: true };

        // Make change to selection
        //TODO how to properly fix this code?
        //model.pushEditOperations([], [op]);
        editorContext.model.pushEditOperations([], [op], null);

        // Update selection to new text.
        const newEndLineNumber = selection.startLineNumber + content.split('\r').length - 1;  // TODO: Not sure if line end is situational/platform specific... investigate more.
        const newEndColumn = (selection.startLineNumber === selection.endLineNumber)
            ? selection.startColumn + content.length
            : content.length - content.lastIndexOf('\r');

        selection = selection.setEndPosition(newEndLineNumber, newEndColumn);

        // Update other selection bound for direction.

        //TODO how to properly fix this code?
        selection = selection.setEndPosition(selection.endLineNumber, selection.endColumn);
        //if (selection.getDirection() == monaco.SelectionDirection.LTR) {
        //    selection.positionColumn = selection.endColumn;
        //    selection.positionLineNumber = selection.endLineNumber;
        //} else {
        //    selection.selectionStartColumn = selection.endColumn;
        //    selection.selectionStartLineNumber = selection.endLineNumber;
        //}

        editorContext.modifingSelection = false;
        editorContext.editor.setSelection(selection);
    }
};