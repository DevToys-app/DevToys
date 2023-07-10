export function registerResizeHandler(id: string, navId: string, dotNetObjRef: DotNet.DotNetObject): void {
    const navBar = document.getElementById(id);
    const navTag = document.getElementById(navId);

    // On nav bar resize
    const resizeObserver = new ResizeObserver((navBars) => {
        adjustSidebarBodyHeight(navTag);
        dotNetObjRef.invokeMethodAsync("OnComponentResize", Math.trunc(navBars[0].contentRect.width));
    });

    resizeObserver.observe(navBar);
}

export function registerKeyboardShortcut(id: string, dotNetObjRef: DotNet.DotNetObject): void {
    const navBar = document.getElementById(id);

    navBar.addEventListener("keydown", function onPress(event) {
        // Ctrl + F
        if ((event.ctrlKey || event.metaKey) && event.key === "f") {
            event.preventDefault();
            dotNetObjRef.invokeMethodAsync("OnFindRequested");
        }
    });
}

function adjustSidebarBodyHeight(navTag: HTMLElement) {
    const sidebarHeader = navTag.querySelector(".sidebar-header") as HTMLElement;
    const sidebarBody = navTag.querySelector(".sidebar-body") as HTMLElement;
    const sidebarFooter = navTag.querySelector(".sidebar-footer") as HTMLElement;

    const newHeight: number = navTag.offsetHeight - sidebarHeader.offsetHeight - sidebarFooter.offsetHeight;
    sidebarBody.style.height = `${newHeight}px`;
}