export function scrollToElement(scrollViewer: HTMLElement, elementToScrollToId: string): void {
    const elementToScrollTo = document.getElementById(elementToScrollToId);

    if (elementToScrollTo != null) {
        const absoluteElementTop = elementToScrollTo.offsetTop - scrollViewer.offsetTop;
        const middle = absoluteElementTop - (scrollViewer.offsetHeight / 2);
        scrollViewer.scrollTop = middle;
    }
}