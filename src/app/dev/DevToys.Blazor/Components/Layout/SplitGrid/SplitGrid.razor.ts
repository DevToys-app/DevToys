// eslint-disable-next-line @typescript-eslint/triple-slash-reference
///<reference path="../../../node_modules/split-grid/index.d.ts" />

// eslint-disable-next-line @typescript-eslint/no-explicit-any
declare const require: any;

import { SplitInstance, SplitOptions } from "split-grid";

interface SplitHTMLElement extends HTMLElement {
    splitInstance: SplitInstance;
}

interface WindowSplitGrid extends Window {
    Split(options: SplitOptions): SplitInstance;
}

export async function initializeSplitGrid(
    rootElement: SplitHTMLElement,
    gutter: HTMLElement,
    vertical: boolean,
    minSize: number)
    : Promise<any> {

    await loadSplitGridLibrary();

    const windowSplitGrid = window as unknown as WindowSplitGrid;
    let splitInstance: SplitInstance;

    if (vertical) {
        splitInstance = windowSplitGrid.Split({
            minSize: minSize,
            snapOffset: 0,
            columnGutters: [{
                track: 1,
                element: gutter,
            }]
        });
    }
    else {
        splitInstance = windowSplitGrid.Split({
            minSize: minSize,
            snapOffset: 0,
            rowGutters: [{
                track: 1,
                element: gutter,
            }]
        });
    }

    rootElement.splitInstance = splitInstance;
}

export function dispose(rootElement: SplitHTMLElement): void {
    if (rootElement.splitInstance) {
        rootElement.splitInstance.destroy(true);
    }
}

async function loadSplitGridLibrary(): Promise<any> {
    const windowSplitGrid = window as unknown as WindowSplitGrid;

    const loadLibraryTask = new Promise((resolve, reject) => {
        // Load split-grid library, if needed.
        if (!windowSplitGrid.Split) {
            /* eslint-disable no-undef */
            require(["_content/DevToys.Blazor/wwwroot/lib/split-grid/split-grid"], function (split) {
                windowSplitGrid.Split = split;
                resolve(null);
            });
        }
        else {
            resolve(null);
        }
    });

    await loadLibraryTask;
}