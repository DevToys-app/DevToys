export function initializeFocusTracking(rootElement: HTMLElement): void {
    rootElement.addEventListener("keydown", onKeyDown);
}

export function dispose(rootElement: HTMLElement): void {
    rootElement.removeEventListener("keydown", onKeyDown);
}

function onKeyDown(e: KeyboardEvent): void {
    const focusableElements = (window as any).devtoys.DOM.getFocusableElements(e.currentTarget as HTMLElement) as NodeListOf<Element>;

    if (focusableElements.length > 0) {
        const firstFocusableElement = focusableElements[0] as HTMLElement;
        const lastFocusableElement = focusableElements[focusableElements.length - 1] as HTMLElement;

        const isTabPressed = (e.key === "Tab");

        if (!isTabPressed) {
            return;
        }

        if (e.shiftKey) /* shift + tab */ {
            if (document.activeElement === firstFocusableElement) {
                lastFocusableElement.focus();
                e.preventDefault();
            }
        } else /* tab */ {
            if (document.activeElement === lastFocusableElement) {
                firstFocusableElement.focus();
                e.preventDefault();
            }
        }
    }
}