export function registerResizeHandler(id, navId, dotNetObjRef) {
    const navBar = document.getElementById(id);
    const navTag = document.getElementById(navId);
    // On nav bar resize
    const resizeObserver = new ResizeObserver((navBars) => {
        adjustSidebarBodyHeight(navTag);
        dotNetObjRef.invokeMethodAsync("OnComponentResize", Math.trunc(navBars[0].contentRect.width));
    });
    resizeObserver.observe(navBar);
}
function adjustSidebarBodyHeight(navTag) {
    const sidebarHeader = navTag.querySelector(".sidebar-header");
    const sidebarBody = navTag.querySelector(".sidebar-body");
    const sidebarFooter = navTag.querySelector(".sidebar-footer");
    const newHeight = navTag.offsetHeight - sidebarHeader.offsetHeight - sidebarFooter.offsetHeight;
    sidebarBody.style.height = `${newHeight}px`;
}
//# sourceMappingURL=NavBar.razor.js.map