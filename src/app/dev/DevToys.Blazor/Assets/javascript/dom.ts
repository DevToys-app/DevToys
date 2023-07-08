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

    public static addFontToDocument(fontDefinition: string): void {
        const css: HTMLStyleElement = document.createElement("style");
        css.innerHTML = fontDefinition;
        document.head.appendChild(css);
    }
}

export default DOM;