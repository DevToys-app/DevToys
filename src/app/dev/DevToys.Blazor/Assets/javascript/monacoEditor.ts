// eslint-disable-next-line @typescript-eslint/triple-slash-reference
///<reference path="../../node_modules/monaco-editor/monaco.d.ts" />

// eslint-disable-next-line @typescript-eslint/no-explicit-any
declare const require: any;

class MonacoEditorHolder {
    public id: string;
    public editor: monaco.editor.IStandaloneCodeEditor | monaco.editor.IStandaloneDiffEditor;
    public dotNetObjRef: DotNet.DotNetObject;
    public isDiffEditor: boolean;
}

class MonacoEditor {
    // list of editor instances
    private static editors: MonacoEditorHolder[] = [];

    // constructor
    static {
        // this will force loading the monaco library. It happens on the app startup.
        require.config({ paths: { "vs": "_content/DevToys.Blazor/wwwroot/lib/monaco-editor/min/vs" } });
        require(["vs/editor/editor.main"]);
    }

    // create a new instance of Monaco Editor.
    public static create(
        id: string,
        options: monaco.editor.IStandaloneEditorConstructionOptions,
        override: monaco.editor.IEditorOverrideServices,
        dotNetObjRef: DotNet.DotNetObject): void {
        if (options == null) {
            options = {};
        }

        const oldEditor = MonacoEditor.getStandaloneCodeEditor(id, true);
        if (oldEditor != null) {
            options.value = oldEditor.getValue();
            MonacoEditor.editors.splice(MonacoEditor.editors.findIndex(item => item.id == id), 1);
            oldEditor.dispose();
        }

        if (typeof monaco === "undefined") {
            throw new Error("Error : Monaco Editor library isn't loaded.");
        }

        const newEditor = monaco.editor.create(document.getElementById(id), options, override);
        MonacoEditor.editors.push({
            id: id,
            editor: newEditor,
            dotNetObjRef: dotNetObjRef,
            isDiffEditor: false
        });
    }

    // create a new instance of Diff Monaco Editor.
    public static createDiffEditor(
        id: string,
        options: monaco.editor.IStandaloneDiffEditorConstructionOptions,
        override: monaco.editor.IEditorOverrideServices,
        dotNetObjRef: DotNet.DotNetObject,
        dotNetObjRefOriginal: DotNet.DotNetObject,
        dotNetObjRefModified: DotNet.DotNetObject): void {
        if (options == null) {
            options = {};
        }

        const oldEditor = MonacoEditor.getStandaloneDiffEditor(id, true);
        let oldModel: monaco.editor.IDiffEditorModel = null;
        if (oldEditor !== null) {
            oldModel = oldEditor.getModel();

            MonacoEditor.editors.splice(MonacoEditor.editors.findIndex(item => item.id === id + "_original"), 1);
            MonacoEditor.editors.splice(MonacoEditor.editors.findIndex(item => item.id === id + "_modified"), 1);
            MonacoEditor.editors.splice(MonacoEditor.editors.findIndex(item => item.id === id), 1);
            oldEditor.dispose();
        }

        if (typeof monaco === "undefined") {
            throw new Error("Error : Monaco Editor library isn't loaded.");
        }

        const editor = monaco.editor.createDiffEditor(document.getElementById(id), options, override);
        MonacoEditor.editors.push({
            id: id,
            editor: editor,
            dotNetObjRef: dotNetObjRef,
            isDiffEditor: true
        });
        MonacoEditor.editors.push({
            id: id + "_original",
            editor: editor.getOriginalEditor(),
            dotNetObjRef: dotNetObjRefOriginal,
            isDiffEditor: false
        });
        MonacoEditor.editors.push({
            id: id + "_modified",
            editor: editor.getModifiedEditor(),
            dotNetObjRef: dotNetObjRefModified,
            isDiffEditor: false
        });

        if (oldModel !== null)
            editor.setModel(oldModel);
    }

    public static createModel(value: string, language: string, uriStr: string) {
        // uri is the key; if no uri exists create one
        if (uriStr == null || uriStr == "") {
            uriStr = "generatedUriKey_" + this.uuidv4();
        }

        const uri = monaco.Uri.parse(uriStr);
        const model = monaco.editor.createModel(value, language, uri);
        if (model == null) {
            return null;
        }

        return {
            id: model.id,
            uri: model.uri.toString()
        };
    }

    // dipose an editor.
    public static dispose(id: string): void {
        this.getEditor(id)?.dispose();
    }

    // update options.
    public static updateOptions(id: string, options: monaco.editor.IEditorOptions & monaco.editor.IGlobalEditorOptions): void {
        this.getEditor(id)?.updateOptions(options);
    }

    // set value.
    public static setValue(id: string, value: string): void {
        this.getStandaloneCodeEditor(id)?.setValue(value);
    }

    // get value.
    public static getValue(id: string, preserveBOM?: boolean, lineEnding?: string): string {
        const editor = this.getStandaloneCodeEditor(id);
        let options = null;
        if (preserveBOM != null && lineEnding != null) {
            options = {
                preserveBOM: preserveBOM,
                lineEnding: lineEnding
            };
        }
        return editor.getValue(options);
    }

    public static async colorize(text: string, languageId: string, options: monaco.editor.IColorizerOptions): Promise<string> {
        const promise = monaco.editor.colorize(text, languageId, options);
        return await promise;
    }

    public static async colorizeElement(elementId: string, options: monaco.editor.IColorizerOptions): Promise<void> {
        const promise = monaco.editor.colorizeElement(document.getElementById(elementId), options);
        return await promise;
    }

    public static colorizeModelLine(uriStr: string, lineNumber: number, tabSize?: number): string {
        const model = MonacoEditor.TextModel.getModel(uriStr);
        if (model == null) {
            return null;
        }
        return monaco.editor.colorizeModelLine(model, lineNumber, tabSize);
    }

    public static defineTheme(themeName: string, themeData: monaco.editor.IStandaloneThemeData): void {
        monaco.editor.defineTheme(themeName, themeData);
    }

    public static getModel(uriStr: string) {
        const model = MonacoEditor.TextModel.getModel(uriStr);
        if (model == null) {
            return null;
        }

        return {
            id: model.id,
            uri: model.uri.toString()
        };
    }

    public static getModels() {
        return monaco.editor.getModels().map(function (value) {
            return {
                id: value.id,
                uri: value.uri.toString()
            };
        });
    }

    public static remeasureFonts(): void {
        monaco.editor.remeasureFonts();
    }

    public static setModelLanguage(uriStr: string, languageId: string): void {
        const model = MonacoEditor.TextModel.getModel(uriStr);
        if (model == null) {
            return null;
        }
        monaco.editor.setModelLanguage(model, languageId);
    }

    public static setTheme(theme: string): boolean {
        monaco.editor.setTheme(theme);
        return true;
    }


    public static addAction(id: string, actionDescriptor: monaco.editor.IActionDescriptor): void {
        const editorHolder = this.getEditorHolder(id);
        editorHolder.editor.addAction({
            id: actionDescriptor.id,
            label: actionDescriptor.label,
            keybindings: actionDescriptor.keybindings,
            precondition: actionDescriptor.precondition,
            keybindingContext: actionDescriptor.keybindingContext,
            contextMenuGroupId: actionDescriptor.contextMenuGroupId,
            contextMenuOrder: actionDescriptor.contextMenuOrder,
            run(editor, args) {
                editorHolder.dotNetObjRef.invokeMethodAsync("ActionCallback", actionDescriptor.id);
            }
        });
    }

    public static addCommand(id: string, keybinding: number, context?: string): void {
        const editorHolder = this.getEditorHolder(id);
        editorHolder.editor.addCommand(keybinding, function (args: any[]): void {
            editorHolder.dotNetObjRef.invokeMethodAsync("CommandCallback", keybinding);
        }, context);
    }

    public static focus(id: string): void {
        const editor = this.getEditor(id);
        editor.focus();
    }

    public static executeEdits(id: string, source: string, edits: monaco.editor.IIdentifiedSingleEditOperation[], endCursorState?: monaco.editor.ICursorStateComputer): boolean {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.executeEdits(source, edits, endCursorState);
    }

    public static getContainerDomNodeId(id: string): string {
        const editor = this.getStandaloneCodeEditor(id);
        const containerNode = editor.getContainerDomNode();
        if (containerNode == null) {
            return null;
        }
        return containerNode.id;
    }

    public static getContentHeight(id: string): number {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getContentHeight();
    }

    public static getContentWidth(id: string): number {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getContentWidth();
    }

    public static getDomNodeId(id: string): string {
        const editor = this.getStandaloneCodeEditor(id);
        const domeNode = editor.getDomNode();
        if (domeNode == null) {
            return null;
        }
        return domeNode.id;
    }

    public static getEditorType(id: string): string {
        const editor = this.getEditor(id);
        return editor.getEditorType();
    }

    public static getInstanceModel(id: string) {
        const editor = this.getEditor(id);
        const model = editor.getModel() as any;
        if (model == null) {
            return null;
        }

        return {
            id: model.id,
            uri: model.uri.toString()
        };
    }

    public static getInstanceDiffModel(id: string) {
        const editor = this.getEditor(id) as unknown as monaco.editor.IStandaloneDiffEditor;
        const model = editor.getModel();
        if (model == null) {
            return null;
        }

        return {
            original: {
                id: model.original.id,
                uri: model.original.uri.toString()
            },
            modified: {
                id: model.modified.id,
                uri: model.modified.uri.toString()
            }
        };
    }

    public static getLayoutInfo(id: string): monaco.editor.EditorLayoutInfo {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getLayoutInfo();
    }

    public static getOffsetForColumn(id: string, lineNumber: number, column: number): number {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getOffsetForColumn(lineNumber, column);
    }

    public static getOption(id: string, optionId: monaco.editor.EditorOption): string {
        const editor = this.getStandaloneCodeEditor(id);
        return JSON.stringify(editor.getOption(optionId));
    }

    public static getOptions(id: string) {
        const editor = this.getStandaloneCodeEditor(id);
        return (editor.getOptions() as any)._values.map(function (value) {
            return JSON.stringify(value);
        });
    }

    public static getPosition(id: string): monaco.Position {
        const editor = this.getEditor(id);
        return editor.getPosition();
    }

    public static getRawOptions(id: string): monaco.editor.IEditorOptions {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getRawOptions();
    }

    public static getScrollHeight(id: string): number {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getScrollHeight();
    }

    public static getScrollLeft(id: string): number {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getScrollLeft();
    }

    public static getScrollTop(id: string): number {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getScrollTop();
    }

    public static getScrollWidth(id: string): number {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getScrollWidth();
    }

    public static getScrolledVisiblePosition(id: string, position: monaco.IPosition) {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getScrolledVisiblePosition(position);
    }

    public static getSelection(id: string): monaco.Selection {
        const editor = this.getEditor(id);
        return editor.getSelection();
    }

    public static getSelections(id: string): monaco.Selection[] {
        const editor = this.getEditor(id);
        return editor.getSelections();
    }

    public static getTargetAtClientPoint(id: string, clientX: number, clientY: number): monaco.editor.IMouseTarget {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getTargetAtClientPoint(clientX, clientY);
    }

    public static getTopForLineNumber(id: string, lineNumber: number): number {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getTopForLineNumber(lineNumber);
    }

    public static getTopForPosition(id: string, lineNumber: number, column: number): number {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getTopForPosition(lineNumber, column);
    }

    public static getVisibleColumnFromPosition(id: string, position: monaco.IPosition): number {
        const editor = this.getEditor(id);
        return editor.getVisibleColumnFromPosition(position);
    }

    public static getVisibleRanges(id: string): monaco.Range[] {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.getVisibleRanges();
    }

    public static hasTextFocus(id: string): boolean {
        const editor = this.getEditor(id);
        return editor.hasTextFocus();
    }

    public static hasWidgetFocus(id: string): boolean {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.hasWidgetFocus();
    }

    public static layout(id: string, dimension?: monaco.editor.IDimension): void {
        const editor = this.getEditor(id);
        editor.layout(dimension);
    }

    public static pushUndoStop(id: string): boolean {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.pushUndoStop();
    }

    public static popUndoStop(id: string): boolean {
        const editor = this.getStandaloneCodeEditor(id);
        return editor.popUndoStop();
    }

    public static render(id: string, forceRedraw?: boolean): void {
        const editor = this.getStandaloneCodeEditor(id);
        editor.render(forceRedraw);
    }

    public static revealLine(id: string, lineNumber: number, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getEditor(id);
        editor.revealLine(lineNumber, scrollType);
    }

    public static revealLineInCenter(id: string, lineNumber: number, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getEditor(id);
        editor.revealLineInCenter(lineNumber, scrollType);
    }

    public static revealLineInCenterIfOutsideViewport(id: string, lineNumber: number, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getEditor(id);
        editor.revealLineInCenterIfOutsideViewport(lineNumber, scrollType);
    }

    public static revealLines(id: string, startLineNumber: number, endLineNumber: number, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getEditor(id);
        editor.revealLines(startLineNumber, endLineNumber, scrollType);
    }

    public static revealLinesInCenter(id: string, startLineNumber: number, endLineNumber: number, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getEditor(id);
        editor.revealLinesInCenter(startLineNumber, endLineNumber, scrollType);
    }

    public static revealLinesInCenterIfOutsideViewport(id: string, startLineNumber: number, endLineNumber: number, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getEditor(id);
        editor.revealLinesInCenterIfOutsideViewport(startLineNumber, endLineNumber, scrollType);
    }

    public static revealPosition(id: string, position: monaco.IPosition, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getEditor(id);
        editor.revealPosition(position, scrollType);
    }

    public static revealPositionInCenter(id: string, position: monaco.IPosition, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getEditor(id);
        editor.revealPositionInCenter(position, scrollType);
    }

    public static revealPositionInCenterIfOutsideViewport(id: string, position: monaco.IPosition, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getEditor(id);
        editor.revealPositionInCenterIfOutsideViewport(position, scrollType);
    }

    public static revealRange(id: string, range: monaco.IRange, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getEditor(id);
        editor.revealRange(range, scrollType);
    }

    public static revealRangeAtTop(id: string, range: monaco.IRange, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getEditor(id);
        editor.revealRangeAtTop(range, scrollType);
    }

    public static revealRangeInCenter(id: string, range: monaco.IRange, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getEditor(id);
        editor.revealRangeInCenter(range, scrollType);
    }

    public static revealRangeInCenterIfOutsideViewport(id: string, range: monaco.IRange, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getEditor(id);
        editor.revealRangeInCenterIfOutsideViewport(range, scrollType);
    }

    public static setEventListener(id: string, eventName: string): void {
        const editorHolder = this.getEditorHolder(id);
        const editor = editorHolder.editor as monaco.editor.IStandaloneCodeEditor;
        const dotNetObjRef = editorHolder.dotNetObjRef;

        const listener = function (e?: unknown) {
            let eventJson = JSON.stringify(e);
            if (eventName == "OnDidChangeModel") {
                const eStrong = e as monaco.editor.IModelChangedEvent;
                eventJson = JSON.stringify({
                    oldModelUri: eStrong.oldModelUrl == null ? null : eStrong.oldModelUrl.toString(),
                    newModelUri: eStrong.newModelUrl == null ? null : eStrong.newModelUrl.toString(),
                });
            }
            else if (eventName == "OnDidChangeConfiguration") {
                const eStrong = e as monaco.editor.ConfigurationChangedEvent;
                eventJson = JSON.stringify((eStrong as any)._values);
            }
            dotNetObjRef.invokeMethodAsync("EventCallbackAsync", eventName, eventJson);
        };

        switch (eventName) {
        case "OnDidCompositionEnd":
            editor.onDidCompositionEnd(listener);
            break;

        case "OnDidCompositionStart":
            editor.onDidCompositionStart(listener);
            break;

        case "OnContextMenu":
            editor.onContextMenu(listener);
            break;

        case "OnDidBlurEditorText":
            editor.onDidBlurEditorText(listener);
            break;

        case "OnDidBlurEditorWidget":
            editor.onDidBlurEditorWidget(listener);
            break;

        case "OnDidChangeConfiguration":
            editor.onDidChangeConfiguration(listener);
            break;

        case "OnDidChangeCursorPosition":
            editor.onDidChangeCursorPosition(listener);
            break;

        case "OnDidChangeCursorSelection":
            editor.onDidChangeCursorSelection(listener);
            break;

        case "OnDidChangeModel":
            editor.onDidChangeModel(listener);
            break;

        case "OnDidChangeModelContent":
            editor.onDidChangeModelContent(listener);
            break;

        case "OnDidChangeModelDecorations":
            editor.onDidChangeModelDecorations(listener);
            break;

        case "OnDidChangeModelLanguage":
            editor.onDidChangeModelLanguage(listener);
            break;

        case "OnDidChangeModelLanguageConfiguration":
            editor.onDidChangeModelLanguageConfiguration(listener);
            break;

        case "OnDidChangeModelOptions":
            editor.onDidChangeModelOptions(listener);
            break;

        case "OnDidContentSizeChange":
            editor.onDidContentSizeChange(listener);
            break;

        case "OnDidDispose":
            editor.onDidDispose(listener);
            break;

        case "OnDidFocusEditorText":
            editor.onDidFocusEditorText(listener);
            break;

        case "OnDidFocusEditorWidget":
            editor.onDidFocusEditorWidget(listener);
            break;

        case "OnDidLayoutChange":
            editor.onDidLayoutChange(listener);
            break;

        case "OnDidPaste":
            editor.onDidPaste(listener);
            break;

        case "OnDidScrollChange":
            editor.onDidScrollChange(listener);
            break;

        case "OnKeyDown":
            editor.onKeyDown(listener);
            break;

        case "OnKeyUp":
            editor.onKeyUp(listener);
            break;

        case "OnMouseDown":
            editor.onMouseDown(listener);
            break;

        case "OnMouseLeave":
            editor.onMouseLeave(listener);
            break;

        case "OnMouseMove":
            editor.onMouseMove(listener);
            break;

        case "OnMouseUp":
            editor.onMouseUp(listener);
            break;
        }
    }

    public static setInstanceModel(id: string, uriStr: string): void {
        const model = MonacoEditor.TextModel.getModel(uriStr);
        if (model == null) {
            return;
        }

        const editor = this.getEditor(id);
        editor.setModel(model);
    }

    public static setInstanceDiffModel(id: string, model: monaco.editor.IDiffEditorModel): void {
        const original_model = MonacoEditor.TextModel.getModel(model.original.uri.toString());
        const modified_model = MonacoEditor.TextModel.getModel(model.modified.uri.toString());
        if (original_model == null || modified_model == null) {
            return;
        }

        const editor = this.getEditor(id) as unknown as monaco.editor.IStandaloneDiffEditor;
        editor.setModel({
            original: original_model,
            modified: modified_model,
        });
    }

    public static setPosition(id: string, position?: monaco.IPosition): void {
        const editor = this.getEditor(id);
        editor.setPosition(position);
    }

    public static setScrollLeft(id: string, newScrollLeft: number, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getStandaloneCodeEditor(id);
        editor.setScrollLeft(newScrollLeft, scrollType);
    }

    public static setScrollPosition(id: string, newPosition: monaco.editor.INewScrollPosition, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getStandaloneCodeEditor(id);
        editor.setScrollPosition(newPosition, scrollType);
    }

    public static setScrollTop(id: string, newScrollTop: number, scrollType?: monaco.editor.ScrollType): void {
        const editor = this.getStandaloneCodeEditor(id);
        editor.setScrollTop(newScrollTop, scrollType);
    }

    public static setSelection(id: string, selection: monaco.IRange): void {
        const editor = this.getEditor(id);
        editor.setSelection(selection);
    }

    public static setSelections(id: string, selections: monaco.ISelection[]): void {
        const editor = this.getEditor(id);
        editor.setSelections(selections);
    }

    public static trigger(id: string, source: string, handlerId: string, payload: any): void {
        const editor = this.getEditor(id);
        editor.trigger(source, handlerId, payload);
    }

    public static uuidv4() {
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
            const r = Math.random() * 16 | 0, v = c == "x" ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    private static getEditor(id: string, unobstrusive = false): monaco.editor.IEditor | null {
        const editorHolder = MonacoEditor.getEditorHolder(id, unobstrusive);
        return editorHolder == null ? null : editorHolder.editor;
    }

    private static getStandaloneCodeEditor(id: string, unobstrusive = false): monaco.editor.IStandaloneCodeEditor | null {
        const editorHolder = MonacoEditor.getEditorHolder(id, unobstrusive);

        if (editorHolder === null) {
            return null;
        }

        if (editorHolder.isDiffEditor) {
            throw "Code editor was expected but a Diff editor has been found.";
        }

        return editorHolder.editor as monaco.editor.IStandaloneCodeEditor;
    }

    private static getStandaloneDiffEditor(id: string, unobstrusive = false): monaco.editor.IStandaloneDiffEditor | null {
        const editorHolder = MonacoEditor.getEditorHolder(id, unobstrusive);

        if (editorHolder === null) {
            return null;
        }

        if (!editorHolder.isDiffEditor) {
            throw "Diff editor was expected but a Code editor has been found.";
        }

        return editorHolder.editor as monaco.editor.IStandaloneDiffEditor;
    }

    private static getEditorHolder(id: string, unobstrusive = false): MonacoEditorHolder {
        const editorHolder: MonacoEditorHolder = MonacoEditor.editors.find(e => e.id === id);
        if (!editorHolder) {
            if (unobstrusive) {
                console.log("WARNING : Couldn't find the editor with id: " + id + " editors.length: " + MonacoEditor.editors.length);
                return null;
            }
            throw "Couldn't find the editor with id: " + id + " editors.length: " + MonacoEditor.editors.length;
        }
        else if (!editorHolder.editor) {
            if (unobstrusive) {
                console.log("WARNING : editor is null for editorHolder: " + editorHolder);
                return null;
            }
            throw "editor is null for editorHolder: " + editorHolder;
        }
        return editorHolder;
    }

    public static TextModel = class {
        public static getModel(uriStr: string): monaco.editor.ITextModel {
            const uri = monaco.Uri.parse(uriStr);
            if (uri == null) {
                return null;
            }
            return monaco.editor.getModel(uri);
        }

        public static getOptions(uriStr: string): monaco.editor.TextModelResolvedOptions {
            const model = this.getModel(uriStr);
            return model.getOptions();
        }

        public static getVersionId(uriStr: string): number {
            const model = this.getModel(uriStr);
            return model.getVersionId();
        }

        public static getAlternativeVersionId(uriStr: string): number {
            const model = this.getModel(uriStr);
            return model.getAlternativeVersionId();
        }

        public static setValue(uriStr: string, newValue: string): void {
            const model = this.getModel(uriStr);
            model.setValue(newValue);
        }

        public static getValue(uriStr: string, eol?: monaco.editor.EndOfLinePreference, preserveBOM?: boolean): string {
            const model = this.getModel(uriStr);
            return model.getValue(eol, preserveBOM);
        }

        public static getValueLength(uriStr: string, eol?: monaco.editor.EndOfLinePreference, preserveBOM?: boolean): number {
            const model = this.getModel(uriStr);
            return model.getValueLength(eol, preserveBOM);
        }

        public static getValueInRange(uriStr: string, range: monaco.IRange, eol?: monaco.editor.EndOfLinePreference): string {
            const model = this.getModel(uriStr);
            return model.getValueInRange(range, eol);
        }

        public static getValueLengthInRange(uriStr: string, range: monaco.IRange): number {
            const model = this.getModel(uriStr);
            return model.getValueLengthInRange(range);
        }

        public static getCharacterCountInRange(uriStr: string, range: monaco.IRange): number {
            const model = this.getModel(uriStr);
            return model.getCharacterCountInRange(range);
        }

        public static getLineCount(uriStr: string): number {
            const model = this.getModel(uriStr);
            return model.getLineCount();
        }

        public static getLineContent(uriStr: string, lineNumber: number): string {
            const model = this.getModel(uriStr);
            return model.getLineContent(lineNumber);
        }

        public static getLineLength(uriStr: string, lineNumber: number): number {
            const model = this.getModel(uriStr);
            return model.getLineLength(lineNumber);
        }

        public static getLinesContent(uriStr: string): string[] {
            const model = this.getModel(uriStr);
            return model.getLinesContent();
        }

        public static getEOL(uriStr: string): string {
            const model = this.getModel(uriStr);
            return model.getEOL();
        }

        public static getEndOfLineSequence(uriStr: string): monaco.editor.EndOfLineSequence {
            const model = this.getModel(uriStr);
            return model.getEndOfLineSequence();
        }

        public static getLineMinColumn(uriStr: string, lineNumber: number): number {
            const model = this.getModel(uriStr);
            return model.getLineMinColumn(lineNumber);
        }

        public static getLineMaxColumn(uriStr: string, lineNumber: number): number {
            const model = this.getModel(uriStr);
            return model.getLineMaxColumn(lineNumber);
        }

        public static getLineFirstNonWhitespaceColumn(uriStr: string, lineNumber: number): number {
            const model = this.getModel(uriStr);
            return model.getLineFirstNonWhitespaceColumn(lineNumber);
        }

        public static getLineLastNonWhitespaceColumn(uriStr: string, lineNumber: number): number {
            const model = this.getModel(uriStr);
            return model.getLineLastNonWhitespaceColumn(lineNumber);
        }

        public static validatePosition(uriStr: string, position: monaco.IPosition): monaco.Position {
            const model = this.getModel(uriStr);
            return model.validatePosition(position);
        }

        public static modifyPosition(uriStr: string, position: monaco.IPosition, offset: number): monaco.Position {
            const model = this.getModel(uriStr);
            return model.modifyPosition(position, offset);
        }

        public static validateRange(uriStr: string, range: monaco.IRange): monaco.Range {
            const model = this.getModel(uriStr);
            return model.validateRange(range);
        }

        public static getOffsetAt(uriStr: string, position: monaco.IPosition): number {
            const model = this.getModel(uriStr);
            return model.getOffsetAt(position);
        }

        public static getPositionAt(uriStr: string, offset: number): monaco.Position {
            const model = this.getModel(uriStr);
            return model.getPositionAt(offset);
        }

        public static getFullModelRange(uriStr: string): monaco.Range {
            const model = this.getModel(uriStr);
            return model.getFullModelRange();
        }

        public static isDisposed(uriStr: string): boolean {
            const model = this.getModel(uriStr);
            return model.isDisposed();
        }

        public static findMatches(
            uriStr: string,
            searchString: string,
            searchScope_or_searchOnlyEditableRange: boolean,
            isRegex: boolean,
            matchCase: boolean,
            wordSeparators: string,
            captureMatches: boolean,
            limitResultCount?: number): monaco.editor.FindMatch[] {

            const model = this.getModel(uriStr);
            return model.findMatches(
                searchString,
                searchScope_or_searchOnlyEditableRange,
                isRegex,
                matchCase,
                wordSeparators,
                captureMatches,
                limitResultCount);
        }

        public static findNextMatch(
            uriStr: string,
            searchString: string,
            searchStart: monaco.IPosition,
            isRegex: boolean,
            matchCase: boolean,
            wordSeparators: string,
            captureMatches: boolean): monaco.editor.FindMatch {

            const model = this.getModel(uriStr);
            return model.findNextMatch(
                searchString,
                searchStart,
                isRegex,
                matchCase,
                wordSeparators,
                captureMatches);
        }

        public static findPreviousMatch(
            uriStr: string,
            searchString: string,
            searchStart: monaco.IPosition,
            isRegex: boolean,
            matchCase: boolean,
            wordSeparators: string,
            captureMatches: boolean): monaco.editor.FindMatch {

            const model = this.getModel(uriStr);
            return model.findPreviousMatch(
                searchString,
                searchStart,
                isRegex,
                matchCase,
                wordSeparators,
                captureMatches);
        }

        public static getLanguageId(uriStr: string): string {
            const model = this.getModel(uriStr);
            return model.getLanguageId();
        }

        public static getWordAtPosition(uriStr: string, position: monaco.IPosition): monaco.editor.IWordAtPosition {
            const model = this.getModel(uriStr);
            return model.getWordAtPosition(position);
        }

        public static getWordUntilPosition(uriStr: string, position: monaco.IPosition): monaco.editor.IWordAtPosition {
            const model = this.getModel(uriStr);
            return model.getWordUntilPosition(position);
        }

        public static deltaDecorations(uriStr: string, oldDecorations: string[], newDecorations: monaco.editor.IModelDecoration[], ownerId?: number): string[] {
            const model = this.getModel(uriStr);
            return model.deltaDecorations(oldDecorations, newDecorations, ownerId);
        }

        public static getDecorationOptions(uriStr: string, id: string): monaco.editor.IModelDecorationOptions {
            const model = this.getModel(uriStr);
            return model.getDecorationOptions(id);
        }

        public static getDecorationRange(uriStr: string, id: string): monaco.Range {
            const model = this.getModel(uriStr);
            return model.getDecorationRange(id);
        }

        public static getLineDecorations(uriStr: string, lineNumber: number, ownerId?: number, filterOutValidation?: boolean): monaco.editor.IModelDecoration[] {
            const model = this.getModel(uriStr);
            return model.getLineDecorations(lineNumber, ownerId, filterOutValidation);
        }

        public static getLinesDecorations(uriStr: string, startLineNumber: number, endLineNumber: number, ownerId?: number, filterOutValidation?: boolean): monaco.editor.IModelDecoration[] {
            const model = this.getModel(uriStr);
            return model.getLinesDecorations(startLineNumber, endLineNumber, ownerId, filterOutValidation);
        }

        public static getDecorationsInRange(uriStr: string, range: monaco.IRange, ownerId?: number, filterOutValidation?: boolean): monaco.editor.IModelDecoration[] {
            const model = this.getModel(uriStr);
            return model.getDecorationsInRange(range, ownerId, filterOutValidation);
        }

        public static getAllDecorations(uriStr: string, ownerId?: number, filterOutValidation?: boolean): monaco.editor.IModelDecoration[] {
            const model = this.getModel(uriStr);
            return model.getAllDecorations(ownerId, filterOutValidation);
        }

        public static getInjectedTextDecorations(uriStr: string, ownerId?: number): monaco.editor.IModelDecoration[] {
            const model = this.getModel(uriStr);
            return model.getInjectedTextDecorations(ownerId);
        }

        public static getOverviewRulerDecorations(uriStr: string, ownerId?: number, filterOutValidation?: boolean): monaco.editor.IModelDecoration[] {
            const model = this.getModel(uriStr);
            return model.getOverviewRulerDecorations(ownerId, filterOutValidation);
        }

        public static normalizeIndentation(uriStr: string, str: string): string {
            const model = this.getModel(uriStr);
            return model.normalizeIndentation(str);
        }

        public static updateOptions(uriStr: string, newOpts: monaco.editor.ITextModelUpdateOptions): void {
            const model = this.getModel(uriStr);
            return model.updateOptions(newOpts);
        }

        public static detectIndentation(uriStr: string, defaultInsertSpaces: boolean, defaultTabSize: number): void {
            const model = this.getModel(uriStr);
            model.detectIndentation(defaultInsertSpaces, defaultTabSize);
        }

        public static pushStackElement(uriStr: string): void {
            const model = this.getModel(uriStr);
            model.pushStackElement();
        }

        public static popStackElement(uriStr: string): void {
            const model = this.getModel(uriStr);
            model.popStackElement();
        }

        public static pushEOL(uriStr: string, eol: monaco.editor.EndOfLineSequence): void {
            const model = this.getModel(uriStr);
            model.pushEOL(eol);
        }

        public static applyEdits(uriStr: string, operations: monaco.editor.IIdentifiedSingleEditOperation[], computeUndoEdits: false): void {
            const model = this.getModel(uriStr);
            model.applyEdits(operations, computeUndoEdits);
        }

        public static setEOL(uriStr: string, eol: monaco.editor.EndOfLineSequence): void {
            const model = this.getModel(uriStr);
            model.setEOL(eol);
        }

        public static dispose(uriStr: string): void {
            const model = this.getModel(uriStr);
            model.dispose();
        }
    };
}

export default MonacoEditor;