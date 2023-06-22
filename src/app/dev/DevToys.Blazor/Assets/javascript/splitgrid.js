// eslint-disable-next-line @typescript-eslint/triple-slash-reference
///<reference path="../../node_modules/split-grid/index.d.ts" />
class SplitGrid {
    static initializeSplitGrid(rootElement, gutter, vertical, minSize) {
        let splitInstance;
        if (vertical) {
            splitInstance = window.Split({
                minSize: minSize,
                snapOffset: 0,
                columnGutters: [{
                        track: 1,
                        element: gutter,
                    }]
            });
        }
        else {
            splitInstance = window.Split({
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
    static dispose(rootElement) {
        rootElement.splitInstance.destroy(true);
    }
}
export default SplitGrid;
//# sourceMappingURL=splitgrid.js.map