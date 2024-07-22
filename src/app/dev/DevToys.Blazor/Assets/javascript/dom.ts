import MonacoEditor from "./monacoEditor";

class DOM {
    public static setFocus(element: HTMLElement): boolean {
        element.focus();

        if (document.activeElement != element) {
            // We failed to give focus to the given element. Let's find the first child element that can
            // accept the focus and let's try to give it.
            const focusableChildrenElements = DOM.getFocusableElements(element);

            if (focusableChildrenElements.length > 0) {
                (focusableChildrenElements[0] as HTMLElement).focus();
                return document.activeElement == element;
            }

            return false;
        }
        else {
            return true;
        }
    }

    public static getFocusableElements(element: HTMLElement): NodeListOf<Element> {
        return element.querySelectorAll(
            "a[href]:not([tabindex='-1'])," +
            "area[href]:not([tabindex='-1'])," +
            "button:not([disabled]):not([tabindex='-1'])," +
            "input:not([disabled]):not([tabindex='-1']):not([type='hidden'])," +
            "select:not([disabled]):not([tabindex='-1'])," +
            "textarea:not([disabled]):not([tabindex='-1'])," +
            "iframe:not([tabindex='-1'])," +
            "details:not([tabindex='-1'])," +
            "[tabindex]:not([tabindex='-1'])," +
            "[contentEditable=true]:not([tabindex='-1']");
    }

    public static cutFromCurrentFocusedElement(): void {
        // Get the currently focused element
        const focusedElement: Element = document.activeElement;

        // Check if the focused element is an input or textarea
        if (focusedElement && (focusedElement.tagName.toLowerCase() === 'input')) {
            // Cast the element to HTMLInputElement
            const inputElement: HTMLInputElement = <HTMLInputElement>focusedElement;

            // Get the current selection
            const startPos: number = inputElement.selectionStart;
            const endPos: number = inputElement.selectionEnd;

            // Copy the selected text to the clipboard
            const selectedText: string = inputElement.value.substring(startPos, endPos);
            navigator.clipboard.writeText(selectedText);

            // Remove the selected text from the input element
            inputElement.value = inputElement.value.substring(0, startPos) + inputElement.value.substring(endPos);

            // Move the caret to the start of the removed text
            inputElement.selectionStart = inputElement.selectionEnd = startPos;
        } else if (focusedElement.tagName.toLowerCase() == 'textarea') {
            const monacoEditorId: string = DOM.getIdOfMonacoInstanceFromTextarea(focusedElement as HTMLTextAreaElement);
            MonacoEditor.cut(monacoEditorId);
        }
    }

    public static copyFromCurrentFocusedElement(): void {
        // Get the currently focused element
        const focusedElement: Element = document.activeElement;

        // Check if the focused element is an input or textarea
        if (focusedElement && (focusedElement.tagName.toLowerCase() === 'input')) {
            // Cast the element to HTMLInputElement
            const inputElement: HTMLInputElement = <HTMLInputElement>focusedElement;

            // Get the current selection
            const startPos: number = inputElement.selectionStart;
            const endPos: number = inputElement.selectionEnd;

            // Copy the selected text to the clipboard
            const selectedText: string = inputElement.value.substring(startPos, endPos);
            navigator.clipboard.writeText(selectedText);
        } else if (focusedElement.tagName.toLowerCase() == 'textarea') {
            const monacoEditorId: string = DOM.getIdOfMonacoInstanceFromTextarea(focusedElement as HTMLTextAreaElement);
            MonacoEditor.copy(monacoEditorId);
        }
    }
    
    public static pasteInCurrentFocusedElement(clipboardData: string): void {
        // Get the currently focused element
        const focusedElement: Element = document.activeElement;

        // Check if the focused element is an input or textarea
        if (focusedElement && (focusedElement.tagName.toLowerCase() === 'input')) {
            // Cast the element to HTMLInputElement
            const inputElement: HTMLInputElement = <HTMLInputElement>focusedElement;

            // Get the current caret position
            const startPos: number = inputElement.selectionStart;
            const endPos: number = inputElement.selectionEnd;

            // Insert the clipboard data at the caret position
            inputElement.value = inputElement.value.substring(0, startPos) + clipboardData + inputElement.value.substring(endPos);

            // Move the caret to the end of the inserted data
            inputElement.selectionStart = inputElement.selectionEnd = startPos + clipboardData.length;
        } else if (focusedElement.tagName.toLowerCase() == 'textarea') {
            const monacoEditorId: string = DOM.getIdOfMonacoInstanceFromTextarea(focusedElement as HTMLTextAreaElement);
            MonacoEditor.paste(monacoEditorId);
        }
    }
    
    public static selectAllInCurrentFocusedElement(): void {
        // Get the currently focused element
        const focusedElement: Element = document.activeElement;

        // Check if the focused element is an input or textarea
        if (focusedElement && (focusedElement.tagName.toLowerCase() === 'input')) {
            // Cast the element to HTMLInputElement
            const inputElement: HTMLInputElement = <HTMLInputElement>focusedElement;

            // Select all text in the input element
            inputElement.select();
        } else if (focusedElement.tagName.toLowerCase() == 'textarea') {
            const monacoEditorId: string = DOM.getIdOfMonacoInstanceFromTextarea(focusedElement as HTMLTextAreaElement);
            MonacoEditor.selectAll(monacoEditorId);
        }
    }

    public static addFontToDocument(fontDefinition: string): void {
        const css: HTMLStyleElement = document.createElement("style");
        css.innerHTML = fontDefinition;
        document.head.appendChild(css);
    }

    public static registerDocumentEventService(dotNetObjRef: DotNet.DotNetObject): void {
        (<any>document).documentEventService = dotNetObjRef;
    }

    public static subscribeDocumentEvent(eventName: string): void {
        document.addEventListener(eventName, DOM.documentEventListener);
    }

    public static unsubscribeDocumentEvent(eventName: string): void {
        document.removeEventListener(eventName, DOM.documentEventListener);
    }

    private static getIdOfMonacoInstanceFromTextarea(textarea: HTMLTextAreaElement): string | null {
        // Start with the parent of the textarea
        let parent = textarea.parentElement;

        // Traverse up the DOM tree
        while (parent) {
            // Check if the parent has the class "monaco-editor-standalone-instance"
            if (parent.classList.contains('monaco-editor-standalone-instance')) {
                // Return the id of the parent
                return parent.id;
            }

            // Move up to the next parent
            parent = parent.parentElement;
        }

        // If no parent with the class "monaco-editor-standalone-instance" is found, return null
        return null;
    }

    private static documentEventListener(e?: Event): void {
        const eventJson = DOM.stringifyEvent(e);
        const dotNetObjRef = (<DotNet.DotNetObject>(<any>document).documentEventService);
        const eventName = e.type;
        dotNetObjRef.invokeMethodAsync("EventCallback", eventName, eventJson);
    }

    private static stringifyEvent(e): string {
        const obj = {};
        for (const k in e) {
            obj[k] = e[k];
        }
        return JSON.stringify(obj, (k, v) => {
            if (v instanceof Node) return "Node";
            if (v instanceof Window) return "Window";
            return v;
        }, " ");
    }
}

export default DOM;