export function initializeScrollViewer(id: string): void {
    const scrollViewer = document.getElementById(id);

    scrollViewer.addEventListener("mousemove", onScrollViewerMouseMove);
}

// TODO: Handle horizontal scrollbar
function onScrollViewerMouseMove(ev: MouseEvent): void {
    const scrollViewer = ev.target as HTMLElement;
    let distance = scrollViewer.offsetLeft + scrollViewer.offsetWidth - ev.pageX;

    if (distance < 12 && distance > -12) {
        scrollViewer.classList.add('on-hover');
    }
    else {
        scrollViewer.classList.remove('on-hover');
    }
}