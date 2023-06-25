export function initializeFocusTracking(rootElement) {
    rootElement.addEventListener("keydown", onKeyDown);
}
export function dispose(rootElement) {
    rootElement.removeEventListener("keydown", onKeyDown);
}
function onKeyDown(e) {
    const focusableElements = window.devtoys.DOM.getFocusableElements(e.currentTarget);
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