const componentList = [];
let lastBreakpoint = null;

// Recalculate breakpoint on resize
if (window.attachEvent) {
    window.attachEvent('load', windowBreakpoint)
    window.attachEvent('onresize', windowBreakpoint);
}
else if (window.addEventListener) {
    window.addEventListener('load', windowBreakpoint, true);
    window.addEventListener('resize', windowBreakpoint, true);
}

function windowBreakpoint() {
   if (componentList && componentList.length > 0) {
        var currentBreakpoint = getBreakpoint();
        if (lastBreakpoint !== currentBreakpoint) {
            lastBreakpoint = currentBreakpoint;
            for (let index = 0; index < componentList.length; ++index) {
                onBreakpoint(componentList[index].dotnetAdapter, currentBreakpoint);
            }
        }
    }
}

lastBreakpoint = getBreakpoint();

// Get the current breakpoint
export function getBreakpoint() {
    return window.getComputedStyle(document.body, ':before').content.replace(/\"/g, '');
}

function onBreakpoint(dotnetAdapter, currentBreakpoint) {
    dotnetAdapter.invokeMethodAsync('OnBreakpoint', currentBreakpoint);
}

export function registerBreakpointComponent(dotnetAdapter, componentId) {
    if (isBreakpointComponent(componentId) !== true) {
        componentList.push({ elementId: componentId, dotnetAdapter: dotnetAdapter });
    }
}

export function isBreakpointComponent(elementId) {
    for (let index = 0; index < componentList.length; ++index) {
        if (componentList[index].elementId === elementId)
            return true;
    }

    return false;
}