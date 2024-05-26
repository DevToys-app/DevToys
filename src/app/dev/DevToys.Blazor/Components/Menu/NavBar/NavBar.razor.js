export function registerResizeHandler(id, navId, dotNetObjRef) {
    const navBar = document.getElementById(id);
    const navTag = document.getElementById(navId);
    const sidebarFooter = navTag.querySelector(".sidebar-footer");
    // On nav bar resize
    const navBarResizeObserver = new ResizeObserver((entries) => {
        adjustSidebarBodyHeight(navTag);
        const navBarEntry = entries[0];
        dotNetObjRef.invokeMethodAsync("OnComponentResize", Math.trunc(navBarEntry.contentRect.width));
    });
    navBarResizeObserver.observe(navBar);
    // On footer resize
    const resizeObserver = new ResizeObserver((entries) => {
        adjustSidebarBodyHeight(navTag);
    });
    resizeObserver.observe(sidebarFooter);
}
export function registerKeyboardShortcut(id, dotNetObjRef) {
    const navBar = document.getElementById(id);
    navBar.addEventListener("keydown", function onPress(event) {
        // Ctrl + F
        if ((event.ctrlKey || event.metaKey) && event.key === "f") {
            event.preventDefault();
            dotNetObjRef.invokeMethodAsync("OnFindRequested");
        }
    });
}
function adjustSidebarBodyHeight(navTag) {
    const sidebarHeader = navTag.querySelector(".sidebar-header");
    const sidebarBody = navTag.querySelector(".sidebar-body");
    const sidebarFooter = navTag.querySelector(".sidebar-footer");
    const navTagComputedStyle = window.getComputedStyle(navTag);
    const navTagPaddingTop = parseFloat(navTagComputedStyle.paddingTop);
    const navTagPaddingBottom = parseFloat(navTagComputedStyle.paddingBottom);
    const navTagHeightWithoutPadding = navTag.offsetHeight - navTagPaddingTop - navTagPaddingBottom;
    const newHeight = navTagHeightWithoutPadding - sidebarHeader.offsetHeight - sidebarFooter.offsetHeight;
    sidebarBody.style.height = `${newHeight}px`;
}
//# sourceMappingURL=NavBar.razor.js.map