export function registerResizeHandler(id, dotNetObjRef) {
    const navBar = document.getElementById(id);
    // On nav bar resize
    const resizeObserver = new ResizeObserver((navBars) => {
        dotNetObjRef.invokeMethodAsync('OnComponentResize', navBars[0].contentRect.width);
    });
    resizeObserver.observe(navBar);
}
//# sourceMappingURL=NavBar.razor.js.map