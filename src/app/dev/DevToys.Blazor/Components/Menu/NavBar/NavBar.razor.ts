export function registerResizeHandler(id: string, navId: string, dotNetObjRef: DotNet.DotNetObject): void {
    const navBar = document.getElementById(id);
    const navTag = document.getElementById(navId);
    const sidebarFooter = navTag.querySelector(".sidebar-footer") as HTMLElement;

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

    const navTagComputedStyle: CSSStyleDeclaration = window.getComputedStyle(navTag);
    const navTagPaddingTop = parseFloat(navTagComputedStyle.paddingTop);
    const navTagPaddingBottom = parseFloat(navTagComputedStyle.paddingBottom);
    const navTagHeightWithoutPadding = navTag.offsetHeight - navTagPaddingTop - navTagPaddingBottom;

    const newHeight: number = navTagHeightWithoutPadding - sidebarHeader.offsetHeight - sidebarFooter.offsetHeight;
    sidebarBody.style.height = `${newHeight}px`;
}