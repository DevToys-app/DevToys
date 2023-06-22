// eslint-disable-next-line @typescript-eslint/triple-slash-reference
///<reference path="../../node_modules/split-grid/index.d.ts" />

import { SplitInstance } from "split-grid";

interface SplitHTMLElement extends HTMLElement {
    splitInstance: SplitInstance;
}

class SplitGrid {
    public static initializeSplitGrid(
        rootElement: SplitHTMLElement,
        gutter: HTMLElement,
        vertical: boolean,
        minSize: number)
        : void {

        let splitInstance: SplitInstance;

        if (vertical) {
            splitInstance = (<any>window).Split({
                minSize: minSize,
                snapOffset: 0,
                columnGutters: [{
                    track: 1,
                    element: gutter,
                }]
            });
        }
        else {
            splitInstance = (<any>window).Split({
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

    public static dispose(rootElement: SplitHTMLElement): void {
        rootElement.splitInstance.destroy(true);
    }
}

export default SplitGrid;