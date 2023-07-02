let parentOfElementToFullScreen: HTMLElement = null;

export function setToFullScreen(fullScreenHostId: string, elementToFullScreenId: string): void {
    if (parentOfElementToFullScreen !== null) {
        throw "ERROR: Looks like FullScreenContainer is already in full screen mode.";
    }

    const fullScreenHost: HTMLElement = document.getElementById(fullScreenHostId);
    const elementToFullScreen: HTMLElement = document.getElementById(elementToFullScreenId);

    parentOfElementToFullScreen = elementToFullScreen.parentElement;

    fullScreenHost.appendChild(elementToFullScreen);
}

export function restoreFromFullScreen(elementToRestoreFromFullScreenId: string): void {
    if (parentOfElementToFullScreen === null) {
        throw "ERROR: Looks like FullScreenContainer is not in full screen mode.";
    }

    const elementToRestoreFromFullScreen: HTMLElement = document.getElementById(elementToRestoreFromFullScreenId);

    parentOfElementToFullScreen.appendChild(elementToRestoreFromFullScreen);
    parentOfElementToFullScreen = null;
}