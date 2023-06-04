export function initializeFocusTracking(rootElement) {
    rootElement.addEventListener("keydown", onKeyDown);
}
export function dispose(rootElement) {
    rootElement.removeEventListener("keydown", onKeyDown);
}
function onKeyDown(e) {
    const focusableElements = e.currentTarget.querySelectorAll("a[href]:not([tabindex='-1'])," +
        "area[href]:not([tabindex='-1'])," +
        "button:not([disabled]):not([tabindex='-1'])," +
        "input:not([disabled]):not([tabindex='-1']):not([type='hidden'])," +
        "select:not([disabled]):not([tabindex='-1'])," +
        "textarea:not([disabled]):not([tabindex='-1'])," +
        "iframe:not([tabindex='-1'])," +
        "details:not([tabindex='-1'])," +
        "[tabindex]:not([tabindex='-1'])," +
        "[contentEditable=true]:not([tabindex='-1']");
    if (focusableElements.length > 0) {
        const firstFocusableElement = focusableElements[0];
        const lastFocusableElement = focusableElements[focusableElements.length - 1];
        const isTabPressed = (e.key === "Tab");
        if (!isTabPressed) {
            return;
        }
        if (e.shiftKey) /* shift + tab */ {
            if (document.activeElement === firstFocusableElement) {
                lastFocusableElement.focus();
                e.preventDefault();
            }
        }
        else /* tab */ {
            if (document.activeElement === lastFocusableElement) {
                firstFocusableElement.focus();
                e.preventDefault();
            }
        }
    }
}
//# sourceMappingURL=FocusTrapper.razor.js.map