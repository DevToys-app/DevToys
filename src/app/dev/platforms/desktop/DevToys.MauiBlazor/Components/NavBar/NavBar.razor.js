export function registerResizeHandler(id, navId, dotNetObjRef) {
    const navBar = document.getElementById(id);
    const navTag = document.getElementById(navId);
    // On nav bar resize
    const resizeObserver = new ResizeObserver((navBars) => {
        adjustSidebarBodyHeight(navTag);
        dotNetObjRef.invokeMethodAsync('OnComponentResize', navBars[0].contentRect.width);
    });
    resizeObserver.observe(navBar);
}
function adjustSidebarBodyHeight(navTag) {
    let sidebarHeader = navTag.querySelector(".sidebar-header");
    let sidebarBody = navTag.querySelector(".sidebar-body");
    let sidebarFooter = navTag.querySelector(".sidebar-footer");
    let newHeight = navTag.offsetHeight - sidebarHeader.offsetHeight - sidebarFooter.offsetHeight;
    sidebarBody.style.height = `${newHeight}px`;
}
//# sourceMappingURL=NavBar.razor.js.map