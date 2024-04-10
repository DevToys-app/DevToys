// eslint-disable-next-line @typescript-eslint/triple-slash-reference
///<reference path="../../../node_modules/split-grid/index.d.ts" />
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
export function initializeSplitGrid(rootElement, gutter, vertical, minSize) {
    return __awaiter(this, void 0, void 0, function* () {
        yield loadSplitGridLibrary();
        const windowSplitGrid = window;
        let splitInstance;
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
    });
}
export function dispose(rootElement) {
    if (rootElement.splitInstance) {
        rootElement.splitInstance.destroy(true);
    }
}
function loadSplitGridLibrary() {
    return __awaiter(this, void 0, void 0, function* () {
        const windowSplitGrid = window;
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
        yield loadLibraryTask;
    });
}
//# sourceMappingURL=SplitGrid.razor.js.map