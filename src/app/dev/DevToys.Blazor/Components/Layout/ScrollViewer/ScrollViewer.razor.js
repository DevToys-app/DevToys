export function scrollToElement(scrollViewer, elementToScrollToId) {
    const elementToScrollTo = document.getElementById(elementToScrollToId);
    if (elementToScrollTo != null) {
        const absoluteElementTop = elementToScrollTo.offsetTop - scrollViewer.offsetTop;
        const middle = absoluteElementTop - (scrollViewer.offsetHeight / 2);
        scrollViewer.scrollTop = middle;
    }
}
//# sourceMappingURL=ScrollViewer.razor.js.map