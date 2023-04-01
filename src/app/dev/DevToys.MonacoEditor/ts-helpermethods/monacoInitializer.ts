///<reference path="../monaco-editor/monaco.d.ts" />

const createMonacoEditor = (basePath: string, element: any) => {
    let editorContext = EditorContext.getEditorForElement(element);
    let debug = editorContext.Debug;
    debug.log("Create dynamic style element");

    var head = document.head || document.getElementsByTagName('head')[0];
    var style = document.createElement('style');
    style.id = 'dynamic';
    head.appendChild(style);

    debug.log("Starting Monaco Load");

    (<any>window).require.config({ paths: { 'vs': `${basePath}/monaco-editor/min/vs` } });
    (<any>window).require(['vs/editor/editor.main'], async function () {
        await initializeMonacoEditorAsync(editorContext);
    });
}

const initializeMonacoEditorAsync = async (editorContext: EditorContext) => {
    let debug = editorContext.Debug;
    let accessor = editorContext.Accessor;

    debug.log("Determining if a standard editor or diff view editor should be created.");
    let isDiffView = await getParentValue(editorContext, "IsDiffViewMode");
    isDiffView = (isDiffView === "true" || isDiffView == true);

    if (isDiffView) {
        await initializeDiffModeMonacoEditorAsync(editorContext);
    }
    else {
        await initializeStandardMonacoEditorAsync(editorContext);
    }

    //// Set theme
    debug.log("Getting accessor theme value");
    let theme = await getParentJsonValue(editorContext, "ActualTheme");
    switch (theme) {
        case "0":
            theme = "Default";
            break;
        case "1":
            theme = "Light";
            break;
        case "2":
            theme = "Dark";
            break;
        default:
            debug.log("Unknown theme");
    }

    debug.log("Current theme value - " + theme);
    if (theme == "Default")
    {
        debug.log("Loading default theme");
        theme = await getThemeCurrentThemeName(editorContext);
    }

    debug.log("Changing theme");
    setTheme(editorContext, await getAccentColorHtmlHex(editorContext));
    changeTheme(editorContext, theme, await getThemeIsHighContrast(editorContext), false);

    // Update Monaco Size when we receive a window resize event
    debug.log("Listen for resize events on the window and resize the editor");
    window.addEventListener(
        "resize",
        () => {
            editorContext.editor.layout();
        });

    // Disable WebView Scrollbar so Monaco Scrollbar can do heavy lifting
    document.body.style.overflow = 'hidden';

    // Callback to Parent that we're loaded
    debug.log("Loaded Monaco");
    accessor.callAction("Loaded");

    debug.log("Ending Monaco Load");
}

const initializeStandardMonacoEditorAsync = async (editorContext: EditorContext) => {
    let debug = editorContext.Debug;
    let accessor = editorContext.Accessor;
    debug.log("Grabbing Monaco Options");

    // link:CodeEditor.Options
    let opt = await getOptions(editorContext);

    debug.log("Getting Parent Text value");

    // link:CodeEditor.Text
    opt["value"] = await getParentValue(editorContext, "Text");

    debug.log("Creating Editor");
    const editor = monaco.editor.create(editorContext.htmlElement, opt);
    EditorContext.registerEditorForElement(editorContext.htmlElement, editor);
    editorContext.standaloneEditor = editor;

    debug.log("Getting & Configuring Editor model");
    let model = editor.getModel();
    model.updateOptions({
        bracketColorizationOptions: { enabled: true }
        });
    editorContext.model = model;

    // Listen for Content Changes
    debug.log("Listening for changes in the editor model - " + (!model));
    model.onDidChangeContent(
        async (event) => {
            // link:CodeEditor.Text
            await accessor.setValue("Text", model.getValue());
        });

    // Listen for Selection Changes
    debug.log("Listening for changes in the editor selection");
    editor.onDidChangeCursorSelection(
        async (event) => {
            if (!editorContext.modifingSelection) {
                var primarySelection = editor.getSelection();
                var start = model.getOffsetAt(primarySelection.getStartPosition());
                var end = model.getOffsetAt(primarySelection.getEndPosition());

                var selectedSpan = new TextSpan();
                selectedSpan.StartPosition = start;
                selectedSpan.Length = end - start;

                // link:CodeEditor.SelectedSpan of type TextSpan
                await accessor.setValueWithType("SelectedSpan", stringifyForMarshalling(JSON.stringify(selectedSpan)), "TextSpan");
            }
        });

    // Listen for focus changes.
    debug.log("Listen for focus events");
    editorContext.standaloneEditor.onDidFocusEditorWidget(
        async (event) => {
            accessor.callAction("GotFocus");
        });
    editorContext.standaloneEditor.onDidBlurEditorWidget(
        async (event) => {
            accessor.callAction("LostFocus");
        });
}

const initializeDiffModeMonacoEditorAsync = async (editorContext: EditorContext) => {
    let debug = editorContext.Debug;
    let accessor = editorContext.Accessor;
    debug.log("Grabbing Monaco Options");

    // link:CodeEditor.DiffOptions
    let opt = await getDiffOptions(editorContext);

    debug.log("Creating Diff Editor");
    const editor = monaco.editor.createDiffEditor(editorContext.htmlElement, opt);
    EditorContext.registerEditorForElement(editorContext.htmlElement, editor);
    editorContext.diffEditor = editor;

    debug.log("Getting & Configuring Diff Editor model");
    let originalModel = monaco.editor.createModel("", "text/plain");
    let modifiedModel = monaco.editor.createModel("", "text/plain");
    editor.setModel({
        original: originalModel,
        modified: modifiedModel
    });

    // Listen for focus changes.
    debug.log("Listen for focus events");
    editorContext.diffEditor.getOriginalEditor().onDidFocusEditorWidget(
        async (event) => {
            accessor.callAction("GotFocus");
        });
    editorContext.diffEditor.getModifiedEditor().onDidFocusEditorWidget(
        async (event) => {
            accessor.callAction("GotFocus");
        });
    editorContext.diffEditor.getOriginalEditor().onDidBlurEditorWidget(
        async (event) => {
            accessor.callAction("LostFocus");
        });
    editorContext.diffEditor.getModifiedEditor().onDidBlurEditorWidget(
        async (event) => {
            accessor.callAction("LostFocus");
        });
}