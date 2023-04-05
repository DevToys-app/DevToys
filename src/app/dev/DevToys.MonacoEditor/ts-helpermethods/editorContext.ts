///<reference path="../monaco-editor/monaco.d.ts" />

class EditorContext {
    static _editors: Map<any, EditorContext> = new Map<any, EditorContext>();

    public static registerEditorForElement(element: any, editor: monaco.editor.IEditor): EditorContext {
        var value = EditorContext.getEditorForElement(element);
        value.editor = editor;
        return value;
    }

    public static getEditorForElement(element: any): EditorContext {
        var context = EditorContext._editors.get(element);

        if (!context) {
            context = new EditorContext();
            context.htmlElement = element;
            EditorContext._editors.set(element, context);
        }

        return context;
    }

    constructor() {
        this.modifingSelection = false;
        this.contexts = {};
        this.decorations = [];
    }

    public Accessor: ParentAccessor;
    public Keyboard: KeyboardListener;
    public Theme: ThemeAccessor;
    public Debug: DebugLogger;

    public editor: monaco.editor.IEditor;
    public standaloneEditor: monaco.editor.IStandaloneCodeEditor;
    public diffEditor: monaco.editor.IStandaloneDiffEditor;
    public model: monaco.editor.ITextModel;
    public contexts: { [index: string]: monaco.editor.IContextKey<any> };
    public decorations: string[];
    public modifingSelection: boolean; // Supress updates to selection when making edits.
    public htmlElement: any;
}

class TextSpan {
    public StartPosition: number;
    public Length: number;
}