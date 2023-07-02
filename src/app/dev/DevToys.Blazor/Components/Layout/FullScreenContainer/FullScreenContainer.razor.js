let parentOfElementToFullScreen = null;
export function setToFullScreen(fullScreenHostId, elementToFullScreenId) {
    if (parentOfElementToFullScreen !== null) {
        throw "ERROR: Looks like FullScreenContainer is already in full screen mode.";
    }
    const fullScreenHost = document.getElementById(fullScreenHostId);
    const elementToFullScreen = document.getElementById(elementToFullScreenId);
    parentOfElementToFullScreen = elementToFullScreen.parentElement;
    fullScreenHost.appendChild(elementToFullScreen);
}
export function restoreFromFullScreen(elementToRestoreFromFullScreenId) {
    if (parentOfElementToFullScreen === null) {
        throw "ERROR: Looks like FullScreenContainer is not in full screen mode.";
    }
    const elementToRestoreFromFullScreen = document.getElementById(elementToRestoreFromFullScreenId);
    parentOfElementToFullScreen.appendChild(elementToRestoreFromFullScreen);
    parentOfElementToFullScreen = null;
}
//# sourceMappingURL=FullScreenContainer.razor.js.map