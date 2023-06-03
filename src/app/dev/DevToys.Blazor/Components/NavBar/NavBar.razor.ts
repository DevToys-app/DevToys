export function registerResizeHandler(id: string, navId: string, dotNetObjRef: DotNet.DotNetObject): void {
    const navBar = document.getElementById(id);
    const navTag = document.getElementById(navId);

    // On nav bar resize
    const resizeObserver = new ResizeObserver((navBars) => {
        adjustSidebarBodyHeight(navTag);
        dotNetObjRef.invokeMethodAsync("OnComponentResize", navBars[0].contentRect.width);
    });

    resizeObserver.observe(navBar);
}

function adjustSidebarBodyHeight(navTag: HTMLElement) {
    const sidebarHeader = navTag.querySelector(".sidebar-header") as HTMLElement;
    const sidebarBody = navTag.querySelector(".sidebar-body") as HTMLElement;
    const sidebarFooter = navTag.querySelector(".sidebar-footer") as HTMLElement;

    const newHeight: number = navTag.offsetHeight - sidebarHeader.offsetHeight - sidebarFooter.offsetHeight;
    sidebarBody.style.height = `${newHeight}px`;
}