export function initializeScrollViewer(id) {
    const scrollViewer = document.getElementById(id);
    scrollViewer.addEventListener("mousemove", onScrollViewerMouseMove);
}
// TODO: Handle horizontal scrollbar
function onScrollViewerMouseMove(ev) {
    const scrollViewer = ev.target;
    const distance = scrollViewer.offsetLeft + scrollViewer.offsetWidth - ev.pageX;
    if (distance < 12 && distance > -12) {
        scrollViewer.classList.add("on-hover");
    }
    else {
        scrollViewer.classList.remove("on-hover");
    }
}
//# sourceMappingURL=ScrollViewer.razor.js.map