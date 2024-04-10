var devtoys = (function (exports) {
    'use strict';

    class DOM {
        static setFocus(element) {
            element.focus();
            if (document.activeElement != element) {
                // We failed to give focus to the given element. Let's find the first child element that can
                // accept the focus and let's try to give it.
                const focusableChildrenElements = DOM.getFocusableElements(element);
                if (focusableChildrenElements.length > 0) {
                    focusableChildrenElements[0].focus();
                    return document.activeElement == element;
                }
                return false;
            }
            else {
                return true;
            }
        }
        static getFocusableElements(element) {
            return element.querySelectorAll("a[href]:not([tabindex='-1'])," +
                "area[href]:not([tabindex='-1'])," +
                "button:not([disabled]):not([tabindex='-1'])," +
                "input:not([disabled]):not([tabindex='-1']):not([type='hidden'])," +
                "select:not([disabled]):not([tabindex='-1'])," +
                "textarea:not([disabled]):not([tabindex='-1'])," +
                "iframe:not([tabindex='-1'])," +
                "details:not([tabindex='-1'])," +
                "[tabindex]:not([tabindex='-1'])," +
                "[contentEditable=true]:not([tabindex='-1']");
        }
        static addFontToDocument(fontDefinition) {
            const css = document.createElement("style");
            css.innerHTML = fontDefinition;
            document.head.appendChild(css);
        }
        static registerDocumentEventService(dotNetObjRef) {
            document.documentEventService = dotNetObjRef;
        }
        static subscribeDocumentEvent(eventName) {
            document.addEventListener(eventName, DOM.documentEventListener);
        }
        static unsubscribeDocumentEvent(eventName) {
            document.removeEventListener(eventName, DOM.documentEventListener);
        }
        static documentEventListener(e) {
            const eventJson = DOM.stringifyEvent(e);
            const dotNetObjRef = document.documentEventService;
            const eventName = e.type;
            dotNetObjRef.invokeMethodAsync("EventCallback", eventName, eventJson);
        }
        static stringifyEvent(e) {
            const obj = {};
            for (const k in e) {
                obj[k] = e[k];
            }
            return JSON.stringify(obj, (k, v) => {
                if (v instanceof Node)
                    return "Node";
                if (v instanceof Window)
                    return "Window";
                return v;
            }, " ");
        }
    }

    class PopoverInfo {
    }
    class PopoverPosition {
    }
    class Popover {
        static callback(id, mutationsList, observer) {
            for (const mutation of mutationsList) {
                if (mutation.type === "attributes") {
                    const target = mutation.target;
                    if (mutation.attributeName == "class") {
                        if (target.classList.contains("popover-overflow-flip-onopen") && target.classList.contains("popover-open") == false) {
                            target.popoverFliped = null;
                            target.removeAttribute("data-popover-flip");
                        }
                        Popover.placePopoverByNode(target);
                    }
                    else if (mutation.attributeName == "data-ticks") {
                        const parent = target.parentElement;
                        const tickValues = [];
                        for (let i = 0; i < parent.children.length; i++) {
                            const childNode = parent.children[i];
                            const tickValue = parseInt(childNode.getAttribute("data-ticks"));
                            if (tickValue == 0) {
                                continue;
                            }
                            if (tickValues.indexOf(tickValue) >= 0) {
                                continue;
                            }
                            tickValues.push(tickValue);
                        }
                        if (tickValues.length == 0) {
                            continue;
                        }
                        const sortedTickValues = tickValues.sort((x, y) => x - y);
                        for (let i = 0; i < parent.children.length; i++) {
                            const childNode = parent.children[i];
                            const tickValue = parseInt(childNode.getAttribute("data-ticks"));
                            if (tickValue == 0) {
                                continue;
                            }
                            if (childNode.skipZIndex == true) {
                                continue;
                            }
                            childNode.style["z-index"] = "calc(var(--popover-zindex) + " + (sortedTickValues.indexOf(tickValue) + 3).toString() + ")";
                        }
                    }
                }
            }
        }
        static initialize(containerClass, flipMargin) {
            const mainContent = document.getElementsByClassName(containerClass);
            if (mainContent.length == 0) {
                return;
            }
            if (flipMargin) {
                Popover.flipMargin = flipMargin;
            }
            Popover.mainContainerClass = containerClass;
            const mainContentFirstItem = mainContent[0];
            if (!mainContentFirstItem.PopoverMark) {
                mainContentFirstItem.PopoverMark = "popovered";
                if (Popover.contentObserver != null) {
                    Popover.contentObserver.disconnect();
                    Popover.contentObserver = null;
                }
                Popover.contentObserver = new ResizeObserver(entries => {
                    Popover.placePopoverByClassSelector();
                });
                Popover.contentObserver.observe(mainContent[0]);
            }
        }
        static connect(id) {
            Popover.initialize(Popover.mainContainerClass);
            const popoverNode = document.getElementById("popover-" + id);
            const popoverContentNode = document.getElementById("popovercontent-" + id);
            if (popoverNode && popoverNode.parentNode && popoverContentNode) {
                Popover.placePopover(popoverNode);
                const config = { attributeFilter: ["class", "data-ticks"] };
                const mutationObserver = new MutationObserver(this.callback.bind(this, id));
                mutationObserver.observe(popoverContentNode, config);
                const resizeObserver = new ResizeObserver(entries => {
                    for (const entry of entries) {
                        const target = entry.target;
                        for (let i = 0; i < target.childNodes.length; i++) {
                            const childNode = target.childNodes[i];
                            if (childNode.id && childNode.id.startsWith("popover-")) {
                                Popover.placePopover(childNode);
                            }
                        }
                    }
                });
                resizeObserver.observe(popoverNode.parentNode);
                const contentNodeObserver = new ResizeObserver(entries => {
                    for (const entry of entries) {
                        const target = entry.target;
                        Popover.placePopoverByNode(target);
                    }
                });
                contentNodeObserver.observe(popoverContentNode);
                const popoverInfo = new PopoverInfo();
                popoverInfo.mutationObserver = mutationObserver;
                popoverInfo.resizeObserver = resizeObserver;
                popoverInfo.contentNodeObserver = contentNodeObserver;
                Popover.map[id] = popoverInfo;
            }
        }
        static disconnect(id) {
            if (Popover.map[id]) {
                const popoverInfo = Popover.map[id];
                popoverInfo.mutationObserver.disconnect();
                popoverInfo.resizeObserver.disconnect();
                popoverInfo.contentNodeObserver.disconnect();
                delete Popover.map[id];
            }
        }
        static dispose() {
            for (const i in Popover.map) {
                Popover.disconnect(i);
            }
            if (Popover.contentObserver != null) {
                Popover.contentObserver.disconnect();
                Popover.contentObserver = null;
            }
        }
        static getAllObservedContainers() {
            const result = [];
            for (const i in this.map) {
                result.push(i);
            }
            return result;
        }
        static calculatePopoverPosition(classList, boundingRect, selfRect) {
            let top = 0;
            let left = 0;
            if (classList.indexOf("popover-anchor-top-left") >= 0) {
                left = boundingRect.left;
                top = boundingRect.top;
            }
            else if (classList.indexOf("popover-anchor-top-center") >= 0) {
                left = boundingRect.left + boundingRect.width / 2;
                top = boundingRect.top;
            }
            else if (classList.indexOf("popover-anchor-top-right") >= 0) {
                left = boundingRect.left + boundingRect.width;
                top = boundingRect.top;
            }
            else if (classList.indexOf("popover-anchor-center-left") >= 0) {
                left = boundingRect.left;
                top = boundingRect.top + boundingRect.height / 2;
            }
            else if (classList.indexOf("popover-anchor-center-center") >= 0) {
                left = boundingRect.left + boundingRect.width / 2;
                top = boundingRect.top + boundingRect.height / 2;
            }
            else if (classList.indexOf("popover-anchor-center-right") >= 0) {
                left = boundingRect.left + boundingRect.width;
                top = boundingRect.top + boundingRect.height / 2;
            }
            else if (classList.indexOf("popover-anchor-bottom-left") >= 0) {
                left = boundingRect.left;
                top = boundingRect.top + boundingRect.height;
            }
            else if (classList.indexOf("popover-anchor-bottom-center") >= 0) {
                left = boundingRect.left + boundingRect.width / 2;
                top = boundingRect.top + boundingRect.height;
            }
            else if (classList.indexOf("popover-anchor-bottom-right") >= 0) {
                left = boundingRect.left + boundingRect.width;
                top = boundingRect.top + boundingRect.height;
            }
            let offsetX = 0;
            let offsetY = 0;
            if (classList.indexOf("popover-top-left") >= 0) {
                offsetX = 0;
                offsetY = 0;
            }
            else if (classList.indexOf("popover-top-center") >= 0) {
                offsetX = -selfRect.width / 2;
                offsetY = 0;
            }
            else if (classList.indexOf("popover-top-right") >= 0) {
                offsetX = -selfRect.width;
                offsetY = 0;
            }
            else if (classList.indexOf("popover-center-left") >= 0) {
                offsetX = 0;
                offsetY = -selfRect.height / 2;
            }
            else if (classList.indexOf("popover-center-center") >= 0) {
                offsetX = -selfRect.width / 2;
                offsetY = -selfRect.height / 2;
            }
            else if (classList.indexOf("popover-center-right") >= 0) {
                offsetX = -selfRect.width;
                offsetY = -selfRect.height / 2;
            }
            else if (classList.indexOf("popover-bottom-left") >= 0) {
                offsetX = 0;
                offsetY = -selfRect.height;
            }
            else if (classList.indexOf("popover-bottom-center") >= 0) {
                offsetX = -selfRect.width / 2;
                offsetY = -selfRect.height;
            }
            else if (classList.indexOf("popover-bottom-right") >= 0) {
                offsetX = -selfRect.width;
                offsetY = -selfRect.height;
            }
            const result = new PopoverPosition();
            result.top = top;
            result.left = left;
            result.offsetX = offsetX;
            result.offsetY = offsetY;
            return result;
        }
        static getPositionForFlippedPopver(inputClassListArray, selector, boundingRect, selfRect) {
            const classList = [];
            for (let i = 0; i < inputClassListArray.length; i++) {
                const item = inputClassListArray[i];
                const replacments = Popover.flipClassReplacements[selector][item];
                if (replacments) {
                    classList.push(replacments);
                }
                else {
                    classList.push(item);
                }
            }
            return Popover.calculatePopoverPosition(classList, boundingRect, selfRect);
        }
        static placePopover(popoverNode, classSelector) {
            if (popoverNode && popoverNode.parentNode) {
                const id = popoverNode.id.substring(8);
                const popoverContentNode = document.getElementById("popovercontent-" + id);
                if (!popoverContentNode) {
                    return;
                }
                if (popoverContentNode.classList.contains("popover-open") == false) {
                    return;
                }
                if (classSelector) {
                    if (popoverContentNode.classList.contains(classSelector) == false) {
                        return;
                    }
                }
                const boundingRect = popoverNode.parentNode.getBoundingClientRect();
                if (popoverContentNode.classList.contains("popover-relative-width")) {
                    popoverContentNode.style["max-width"] = (boundingRect.width) + "px";
                }
                const selfRect = popoverContentNode.getBoundingClientRect();
                const classList = popoverContentNode.classList;
                const classListArray = Array.from(popoverContentNode.classList);
                const position = Popover.calculatePopoverPosition(classListArray, boundingRect, selfRect);
                let left = position.left;
                let top = position.top;
                let offsetX = position.offsetX;
                let offsetY = position.offsetY;
                if (classList.contains("popover-overflow-flip-onopen") || classList.contains("popover-overflow-flip-always")) {
                    const graceMargin = Popover.flipMargin;
                    const deltaToLeft = left + offsetX;
                    const deltaToRight = window.innerWidth - left - selfRect.width;
                    const deltaTop = top - selfRect.height;
                    const spaceToTop = top;
                    const deltaBottom = window.innerHeight - top - selfRect.height;
                    const popoverContentNodeAny = popoverContentNode;
                    let selector = popoverContentNodeAny.popoverFliped;
                    if (!selector) {
                        if (classList.contains("popover-top-left")) {
                            if (deltaBottom < graceMargin && deltaToRight < graceMargin && spaceToTop >= selfRect.height && deltaToLeft >= selfRect.width) {
                                selector = "top-and-left";
                            }
                            else if (deltaBottom < graceMargin && spaceToTop >= selfRect.height) {
                                selector = "top";
                            }
                            else if (deltaToRight < graceMargin && deltaToLeft >= selfRect.width) {
                                selector = "left";
                            }
                        }
                        else if (classList.contains("popover-top-center")) {
                            if (deltaBottom < graceMargin && spaceToTop >= selfRect.height) {
                                selector = "top";
                            }
                        }
                        else if (classList.contains("popover-top-right")) {
                            if (deltaBottom < graceMargin && deltaToLeft < graceMargin && spaceToTop >= selfRect.height && deltaToRight >= selfRect.width) {
                                selector = "top-and-right";
                            }
                            else if (deltaBottom < graceMargin && spaceToTop >= selfRect.height) {
                                selector = "top";
                            }
                            else if (deltaToLeft < graceMargin && deltaToRight >= selfRect.width) {
                                selector = "right";
                            }
                        }
                        else if (classList.contains("popover-center-left")) {
                            if (deltaToRight < graceMargin && deltaToLeft >= selfRect.width) {
                                selector = "left";
                            }
                        }
                        else if (classList.contains("popover-center-right")) {
                            if (deltaToLeft < graceMargin && deltaToRight >= selfRect.width) {
                                selector = "right";
                            }
                        }
                        else if (classList.contains("popover-bottom-left")) {
                            if (deltaTop < graceMargin && deltaToRight < graceMargin && deltaBottom >= 0 && deltaToLeft >= selfRect.width) {
                                selector = "bottom-and-left";
                            }
                            else if (deltaTop < graceMargin && deltaBottom >= 0) {
                                selector = "bottom";
                            }
                            else if (deltaToRight < graceMargin && deltaToLeft >= selfRect.width) {
                                selector = "left";
                            }
                        }
                        else if (classList.contains("popover-bottom-center")) {
                            if (deltaTop < graceMargin && deltaBottom >= 0) {
                                selector = "bottom";
                            }
                        }
                        else if (classList.contains("popover-bottom-right")) {
                            if (deltaTop < graceMargin && deltaToLeft < graceMargin && deltaBottom >= 0 && deltaToRight >= selfRect.width) {
                                selector = "bottom-and-right";
                            }
                            else if (deltaTop < graceMargin && deltaBottom >= 0) {
                                selector = "bottom";
                            }
                            else if (deltaToLeft < graceMargin && deltaToRight >= selfRect.width) {
                                selector = "right";
                            }
                        }
                    }
                    if (selector && selector != "none") {
                        const newPosition = Popover.getPositionForFlippedPopver(classListArray, selector, boundingRect, selfRect);
                        left = newPosition.left;
                        top = newPosition.top;
                        offsetX = newPosition.offsetX;
                        offsetY = newPosition.offsetY;
                        popoverContentNode.setAttribute("data-popover-flip", "flipped");
                    }
                    else {
                        popoverContentNode.removeAttribute("data-popover-flip");
                    }
                    if (classList.contains("popover-overflow-flip-onopen")) {
                        const popoverContentNodeAny = popoverContentNode;
                        if (!popoverContentNodeAny.popoverFliped) {
                            popoverContentNodeAny.popoverFliped = selector || "none";
                        }
                    }
                }
                if (popoverContentNode.classList.contains("popover-fixed")) ;
                else if (window.getComputedStyle(popoverNode).position == "fixed") {
                    popoverContentNode.style["position"] = "fixed";
                }
                else {
                    offsetX += window.scrollX;
                    offsetY += window.scrollY;
                }
                popoverContentNode.style["left"] = (left + offsetX) + "px";
                popoverContentNode.style["top"] = (top + offsetY) + "px";
                if (window.getComputedStyle(popoverNode).getPropertyValue("z-index") != "auto") {
                    popoverContentNode.style["z-index"] = window.getComputedStyle(popoverNode).getPropertyValue("z-index");
                    popoverContentNode.skipZIndex = true;
                }
            }
        }
        static placePopoverByClassSelector(classSelector = null) {
            const items = Popover.getAllObservedContainers();
            for (let i = 0; i < items.length; i++) {
                const popoverNode = document.getElementById("popover-" + items[i]);
                Popover.placePopover(popoverNode, classSelector);
            }
        }
        static placePopoverByNode(target) {
            const id = target.id.substring(15);
            const popoverNode = document.getElementById("popover-" + id);
            Popover.placePopover(popoverNode);
        }
    }
    Popover.flipClassReplacements = {
        "top": {
            "popover-top-left": "popover-bottom-left",
            "popover-top-center": "popover-bottom-center",
            "popover-anchor-bottom-center": "popover-anchor-top-center",
            "popover-top-right": "popover-bottom-right",
        },
        "left": {
            "popover-top-left": "popover-top-right",
            "popover-center-left": "popover-center-right",
            "popover-anchor-center-right": "popover-anchor-center-left",
            "popover-bottom-left": "popover-bottom-right",
        },
        "right": {
            "popover-top-right": "popover-top-left",
            "popover-center-right": "popover-center-left",
            "popover-anchor-center-left": "popover-anchor-center-right",
            "popover-bottom-right": "popover-bottom-left",
        },
        "bottom": {
            "popover-bottom-left": "popover-top-left",
            "popover-bottom-center": "popover-top-center",
            "popover-anchor-top-center": "popover-anchor-bottom-center",
            "popover-bottom-right": "popover-top-right",
        },
        "top-and-left": {
            "popover-top-left": "popover-bottom-right",
        },
        "top-and-right": {
            "popover-top-right": "popover-bottom-left",
        },
        "bottom-and-left": {
            "popover-bottom-left": "popover-top-right",
        },
        "bottom-and-right": {
            "popover-bottom-right": "popover-top-left",
        },
    };
    Popover.flipMargin = 0;
    Popover.map = {};
    Popover.contentObserver = null;
    Popover.mainContainerClass = null;
    // constructor
    (() => {
        window.addEventListener("scroll", () => {
            Popover.placePopoverByClassSelector("popover-fixed");
            Popover.placePopoverByClassSelector("popover-overflow-flip-always");
        });
        window.addEventListener("resize", () => {
            Popover.placePopoverByClassSelector();
        });
    })();

    // eslint-disable-next-line @typescript-eslint/triple-slash-reference
    ///<reference path="../../node_modules/monaco-editor/monaco.d.ts" />
    var __awaiter = (undefined && undefined.__awaiter) || function (thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
    class MonacoEditor {
        // create a new instance of Monaco Editor.
        static create(id, options, override, dotNetObjRef) {
            if (options == null) {
                options = {};
            }
            const oldEditor = MonacoEditor.getStandaloneCodeEditor(id, true);
            if (oldEditor != null) {
                options.value = oldEditor.getValue();
                MonacoEditor.editors.splice(MonacoEditor.editors.findIndex(item => item.id == id), 1);
                oldEditor.dispose();
            }
            if (typeof monaco === "undefined") {
                throw new Error("Error : Monaco Editor library isn't loaded.");
            }
            const newEditor = monaco.editor.create(document.getElementById(id), options, override);
            MonacoEditor.editors.push({
                id: id,
                editor: newEditor,
                dotNetObjRef: dotNetObjRef,
                isDiffEditor: false
            });
        }
        // create a new instance of Diff Monaco Editor.
        static createDiffEditor(id, options, override, dotNetObjRef, dotNetObjRefOriginal, dotNetObjRefModified) {
            if (options == null) {
                options = {};
            }
            const oldEditor = MonacoEditor.getStandaloneDiffEditor(id, true);
            let oldModel = null;
            if (oldEditor !== null) {
                oldModel = oldEditor.getModel();
                MonacoEditor.editors.splice(MonacoEditor.editors.findIndex(item => item.id === id + "_original"), 1);
                MonacoEditor.editors.splice(MonacoEditor.editors.findIndex(item => item.id === id + "_modified"), 1);
                MonacoEditor.editors.splice(MonacoEditor.editors.findIndex(item => item.id === id), 1);
                oldEditor.dispose();
            }
            if (typeof monaco === "undefined") {
                throw new Error("Error : Monaco Editor library isn't loaded.");
            }
            const editor = monaco.editor.createDiffEditor(document.getElementById(id), options, override);
            MonacoEditor.editors.push({
                id: id,
                editor: editor,
                dotNetObjRef: dotNetObjRef,
                isDiffEditor: true
            });
            MonacoEditor.editors.push({
                id: id + "_original",
                editor: editor.getOriginalEditor(),
                dotNetObjRef: dotNetObjRefOriginal,
                isDiffEditor: false
            });
            MonacoEditor.editors.push({
                id: id + "_modified",
                editor: editor.getModifiedEditor(),
                dotNetObjRef: dotNetObjRefModified,
                isDiffEditor: false
            });
            if (oldModel !== null)
                editor.setModel(oldModel);
        }
        static createModel(value, language, uriStr) {
            // uri is the key; if no uri exists create one
            if (uriStr == null || uriStr == "") {
                uriStr = "generatedUriKey_" + this.uuidv4();
            }
            const uri = monaco.Uri.parse(uriStr);
            const model = monaco.editor.createModel(value, language, uri);
            if (model == null) {
                return null;
            }
            return {
                id: model.id,
                uri: model.uri.toString()
            };
        }
        // dipose an editor.
        static dispose(id) {
            var _a;
            (_a = this.getEditor(id)) === null || _a === void 0 ? void 0 : _a.dispose();
        }
        // update options.
        static updateOptions(id, options) {
            var _a;
            (_a = this.getEditor(id)) === null || _a === void 0 ? void 0 : _a.updateOptions(options);
        }
        // set value.
        static setValue(id, value) {
            var _a;
            (_a = this.getStandaloneCodeEditor(id)) === null || _a === void 0 ? void 0 : _a.setValue(value);
        }
        // get value.
        static getValue(id, preserveBOM, lineEnding) {
            const editor = this.getStandaloneCodeEditor(id);
            let options = null;
            if (preserveBOM != null && lineEnding != null) {
                options = {
                    preserveBOM: preserveBOM,
                    lineEnding: lineEnding
                };
            }
            return editor.getValue(options);
        }
        static colorize(text, languageId, options) {
            return __awaiter(this, void 0, void 0, function* () {
                const promise = monaco.editor.colorize(text, languageId, options);
                return yield promise;
            });
        }
        static colorizeElement(elementId, options) {
            return __awaiter(this, void 0, void 0, function* () {
                const promise = monaco.editor.colorizeElement(document.getElementById(elementId), options);
                return yield promise;
            });
        }
        static colorizeModelLine(uriStr, lineNumber, tabSize) {
            const model = MonacoEditor.TextModel.getModel(uriStr);
            if (model == null) {
                return null;
            }
            return monaco.editor.colorizeModelLine(model, lineNumber, tabSize);
        }
        static defineTheme(themeName, themeData) {
            monaco.editor.defineTheme(themeName, themeData);
        }
        static getModel(uriStr) {
            const model = MonacoEditor.TextModel.getModel(uriStr);
            if (model == null) {
                return null;
            }
            return {
                id: model.id,
                uri: model.uri.toString()
            };
        }
        static getModels() {
            return monaco.editor.getModels().map(function (value) {
                return {
                    id: value.id,
                    uri: value.uri.toString()
                };
            });
        }
        static remeasureFonts() {
            monaco.editor.remeasureFonts();
        }
        static setModelLanguage(uriStr, languageId) {
            const model = MonacoEditor.TextModel.getModel(uriStr);
            if (model == null) {
                return null;
            }
            monaco.editor.setModelLanguage(model, languageId);
        }
        static setTheme(theme) {
            monaco.editor.setTheme(theme);
            return true;
        }
        static addAction(id, actionDescriptor) {
            const editorHolder = this.getEditorHolder(id);
            editorHolder.editor.addAction({
                id: actionDescriptor.id,
                label: actionDescriptor.label,
                keybindings: actionDescriptor.keybindings,
                precondition: actionDescriptor.precondition,
                keybindingContext: actionDescriptor.keybindingContext,
                contextMenuGroupId: actionDescriptor.contextMenuGroupId,
                contextMenuOrder: actionDescriptor.contextMenuOrder,
                run(editor, args) {
                    editorHolder.dotNetObjRef.invokeMethodAsync("ActionCallback", actionDescriptor.id);
                }
            });
        }
        static addCommand(id, keybinding, context) {
            const editorHolder = this.getEditorHolder(id);
            editorHolder.editor.addCommand(keybinding, function (args) {
                editorHolder.dotNetObjRef.invokeMethodAsync("CommandCallback", keybinding);
            }, context);
        }
        static focus(id) {
            const editor = this.getEditor(id);
            editor.focus();
        }
        static executeEdits(id, source, edits, endCursorState) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.executeEdits(source, edits, endCursorState);
        }
        static getContainerDomNodeId(id) {
            const editor = this.getStandaloneCodeEditor(id);
            const containerNode = editor.getContainerDomNode();
            if (containerNode == null) {
                return null;
            }
            return containerNode.id;
        }
        static getContentHeight(id) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getContentHeight();
        }
        static getContentWidth(id) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getContentWidth();
        }
        static getDomNodeId(id) {
            const editor = this.getStandaloneCodeEditor(id);
            const domeNode = editor.getDomNode();
            if (domeNode == null) {
                return null;
            }
            return domeNode.id;
        }
        static getEditorType(id) {
            const editor = this.getEditor(id);
            return editor.getEditorType();
        }
        static getInstanceModel(id) {
            const editor = this.getEditor(id);
            const model = editor.getModel();
            if (model == null) {
                return null;
            }
            return {
                id: model.id,
                uri: model.uri.toString()
            };
        }
        static getInstanceDiffModel(id) {
            const editor = this.getEditor(id);
            const model = editor.getModel();
            if (model == null) {
                return null;
            }
            return {
                original: {
                    id: model.original.id,
                    uri: model.original.uri.toString()
                },
                modified: {
                    id: model.modified.id,
                    uri: model.modified.uri.toString()
                }
            };
        }
        static getLayoutInfo(id) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getLayoutInfo();
        }
        static getOffsetForColumn(id, lineNumber, column) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getOffsetForColumn(lineNumber, column);
        }
        static getOption(id, optionId) {
            const editor = this.getStandaloneCodeEditor(id);
            return JSON.stringify(editor.getOption(optionId));
        }
        static getOptions(id) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getOptions()._values.map(function (value) {
                return JSON.stringify(value);
            });
        }
        static getPosition(id) {
            const editor = this.getEditor(id);
            return editor.getPosition();
        }
        static getRawOptions(id) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getRawOptions();
        }
        static getScrollHeight(id) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getScrollHeight();
        }
        static getScrollLeft(id) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getScrollLeft();
        }
        static getScrollTop(id) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getScrollTop();
        }
        static getScrollWidth(id) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getScrollWidth();
        }
        static getScrolledVisiblePosition(id, position) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getScrolledVisiblePosition(position);
        }
        static getSelection(id) {
            const editor = this.getEditor(id);
            return editor.getSelection();
        }
        static getSelections(id) {
            const editor = this.getEditor(id);
            return editor.getSelections();
        }
        static getTargetAtClientPoint(id, clientX, clientY) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getTargetAtClientPoint(clientX, clientY);
        }
        static getTopForLineNumber(id, lineNumber) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getTopForLineNumber(lineNumber);
        }
        static getTopForPosition(id, lineNumber, column) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getTopForPosition(lineNumber, column);
        }
        static getVisibleColumnFromPosition(id, position) {
            const editor = this.getEditor(id);
            return editor.getVisibleColumnFromPosition(position);
        }
        static getVisibleRanges(id) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.getVisibleRanges();
        }
        static hasTextFocus(id) {
            const editor = this.getEditor(id);
            return editor.hasTextFocus();
        }
        static hasWidgetFocus(id) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.hasWidgetFocus();
        }
        static layout(id, dimension) {
            const editor = this.getEditor(id);
            editor.layout(dimension);
        }
        static pushUndoStop(id) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.pushUndoStop();
        }
        static popUndoStop(id) {
            const editor = this.getStandaloneCodeEditor(id);
            return editor.popUndoStop();
        }
        static render(id, forceRedraw) {
            const editor = this.getStandaloneCodeEditor(id);
            editor.render(forceRedraw);
        }
        static revealLine(id, lineNumber, scrollType) {
            const editor = this.getEditor(id);
            editor.revealLine(lineNumber, scrollType);
        }
        static revealLineInCenter(id, lineNumber, scrollType) {
            const editor = this.getEditor(id);
            editor.revealLineInCenter(lineNumber, scrollType);
        }
        static revealLineInCenterIfOutsideViewport(id, lineNumber, scrollType) {
            const editor = this.getEditor(id);
            editor.revealLineInCenterIfOutsideViewport(lineNumber, scrollType);
        }
        static revealLines(id, startLineNumber, endLineNumber, scrollType) {
            const editor = this.getEditor(id);
            editor.revealLines(startLineNumber, endLineNumber, scrollType);
        }
        static revealLinesInCenter(id, startLineNumber, endLineNumber, scrollType) {
            const editor = this.getEditor(id);
            editor.revealLinesInCenter(startLineNumber, endLineNumber, scrollType);
        }
        static revealLinesInCenterIfOutsideViewport(id, startLineNumber, endLineNumber, scrollType) {
            const editor = this.getEditor(id);
            editor.revealLinesInCenterIfOutsideViewport(startLineNumber, endLineNumber, scrollType);
        }
        static revealPosition(id, position, scrollType) {
            const editor = this.getEditor(id);
            editor.revealPosition(position, scrollType);
        }
        static revealPositionInCenter(id, position, scrollType) {
            const editor = this.getEditor(id);
            editor.revealPositionInCenter(position, scrollType);
        }
        static revealPositionInCenterIfOutsideViewport(id, position, scrollType) {
            const editor = this.getEditor(id);
            editor.revealPositionInCenterIfOutsideViewport(position, scrollType);
        }
        static revealRange(id, range, scrollType) {
            const editor = this.getEditor(id);
            editor.revealRange(range, scrollType);
        }
        static revealRangeAtTop(id, range, scrollType) {
            const editor = this.getEditor(id);
            editor.revealRangeAtTop(range, scrollType);
        }
        static revealRangeInCenter(id, range, scrollType) {
            const editor = this.getEditor(id);
            editor.revealRangeInCenter(range, scrollType);
        }
        static revealRangeInCenterIfOutsideViewport(id, range, scrollType) {
            const editor = this.getEditor(id);
            editor.revealRangeInCenterIfOutsideViewport(range, scrollType);
        }
        static setEventListener(id, eventName) {
            const editorHolder = this.getEditorHolder(id);
            const editor = editorHolder.editor;
            const dotNetObjRef = editorHolder.dotNetObjRef;
            const listener = function (e) {
                let eventJson = JSON.stringify(e);
                if (eventName == "OnDidChangeModel") {
                    const eStrong = e;
                    eventJson = JSON.stringify({
                        oldModelUri: eStrong.oldModelUrl == null ? null : eStrong.oldModelUrl.toString(),
                        newModelUri: eStrong.newModelUrl == null ? null : eStrong.newModelUrl.toString(),
                    });
                }
                else if (eventName == "OnDidChangeConfiguration") {
                    const eStrong = e;
                    eventJson = JSON.stringify(eStrong._values);
                }
                dotNetObjRef.invokeMethodAsync("EventCallbackAsync", eventName, eventJson);
            };
            switch (eventName) {
                case "OnDidCompositionEnd":
                    editor.onDidCompositionEnd(listener);
                    break;
                case "OnDidCompositionStart":
                    editor.onDidCompositionStart(listener);
                    break;
                case "OnContextMenu":
                    editor.onContextMenu(listener);
                    break;
                case "OnDidBlurEditorText":
                    editor.onDidBlurEditorText(listener);
                    break;
                case "OnDidBlurEditorWidget":
                    editor.onDidBlurEditorWidget(listener);
                    break;
                case "OnDidChangeConfiguration":
                    editor.onDidChangeConfiguration(listener);
                    break;
                case "OnDidChangeCursorPosition":
                    editor.onDidChangeCursorPosition(listener);
                    break;
                case "OnDidChangeCursorSelection":
                    editor.onDidChangeCursorSelection(listener);
                    break;
                case "OnDidChangeModel":
                    editor.onDidChangeModel(listener);
                    break;
                case "OnDidChangeModelContent":
                    editor.onDidChangeModelContent(listener);
                    break;
                case "OnDidChangeModelDecorations":
                    editor.onDidChangeModelDecorations(listener);
                    break;
                case "OnDidChangeModelLanguage":
                    editor.onDidChangeModelLanguage(listener);
                    break;
                case "OnDidChangeModelLanguageConfiguration":
                    editor.onDidChangeModelLanguageConfiguration(listener);
                    break;
                case "OnDidChangeModelOptions":
                    editor.onDidChangeModelOptions(listener);
                    break;
                case "OnDidContentSizeChange":
                    editor.onDidContentSizeChange(listener);
                    break;
                case "OnDidDispose":
                    editor.onDidDispose(listener);
                    break;
                case "OnDidFocusEditorText":
                    editor.onDidFocusEditorText(listener);
                    break;
                case "OnDidFocusEditorWidget":
                    editor.onDidFocusEditorWidget(listener);
                    break;
                case "OnDidLayoutChange":
                    editor.onDidLayoutChange(listener);
                    break;
                case "OnDidPaste":
                    editor.onDidPaste(listener);
                    break;
                case "OnDidScrollChange":
                    editor.onDidScrollChange(listener);
                    break;
                case "OnKeyDown":
                    editor.onKeyDown(listener);
                    break;
                case "OnKeyUp":
                    editor.onKeyUp(listener);
                    break;
                case "OnMouseDown":
                    editor.onMouseDown(listener);
                    break;
                case "OnMouseLeave":
                    editor.onMouseLeave(listener);
                    break;
                case "OnMouseMove":
                    editor.onMouseMove(listener);
                    break;
                case "OnMouseUp":
                    editor.onMouseUp(listener);
                    break;
            }
        }
        static setInstanceModel(id, uriStr) {
            const model = MonacoEditor.TextModel.getModel(uriStr);
            if (model == null) {
                return;
            }
            const editor = this.getEditor(id);
            editor.setModel(model);
        }
        static setInstanceDiffModel(id, model) {
            const original_model = MonacoEditor.TextModel.getModel(model.original.uri.toString());
            const modified_model = MonacoEditor.TextModel.getModel(model.modified.uri.toString());
            if (original_model == null || modified_model == null) {
                return;
            }
            const editor = this.getEditor(id);
            editor.setModel({
                original: original_model,
                modified: modified_model,
            });
        }
        static setPosition(id, position) {
            const editor = this.getEditor(id);
            editor.setPosition(position);
        }
        static setScrollLeft(id, newScrollLeft, scrollType) {
            const editor = this.getStandaloneCodeEditor(id);
            editor.setScrollLeft(newScrollLeft, scrollType);
        }
        static setScrollPosition(id, newPosition, scrollType) {
            const editor = this.getStandaloneCodeEditor(id);
            editor.setScrollPosition(newPosition, scrollType);
        }
        static setScrollTop(id, newScrollTop, scrollType) {
            const editor = this.getStandaloneCodeEditor(id);
            editor.setScrollTop(newScrollTop, scrollType);
        }
        static setSelection(id, selection) {
            const editor = this.getEditor(id);
            editor.setSelection(selection);
        }
        static setSelections(id, selections) {
            const editor = this.getEditor(id);
            editor.setSelections(selections);
        }
        static trigger(id, source, handlerId, payload) {
            const editor = this.getEditor(id);
            editor.trigger(source, handlerId, payload);
        }
        static uuidv4() {
            return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
                const r = Math.random() * 16 | 0, v = c == "x" ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        }
        static getEditor(id, unobstrusive = false) {
            const editorHolder = MonacoEditor.getEditorHolder(id, unobstrusive);
            return editorHolder == null ? null : editorHolder.editor;
        }
        static getStandaloneCodeEditor(id, unobstrusive = false) {
            const editorHolder = MonacoEditor.getEditorHolder(id, unobstrusive);
            if (editorHolder === null) {
                return null;
            }
            if (editorHolder.isDiffEditor) {
                throw "Code editor was expected but a Diff editor has been found.";
            }
            return editorHolder.editor;
        }
        static getStandaloneDiffEditor(id, unobstrusive = false) {
            const editorHolder = MonacoEditor.getEditorHolder(id, unobstrusive);
            if (editorHolder === null) {
                return null;
            }
            if (!editorHolder.isDiffEditor) {
                throw "Diff editor was expected but a Code editor has been found.";
            }
            return editorHolder.editor;
        }
        static getEditorHolder(id, unobstrusive = false) {
            const editorHolder = MonacoEditor.editors.find(e => e.id === id);
            if (!editorHolder) {
                if (unobstrusive) {
                    console.log("WARNING : Couldn't find the editor with id: " + id + " editors.length: " + MonacoEditor.editors.length);
                    return null;
                }
                throw "Couldn't find the editor with id: " + id + " editors.length: " + MonacoEditor.editors.length;
            }
            else if (!editorHolder.editor) {
                if (unobstrusive) {
                    console.log("WARNING : editor is null for editorHolder: " + editorHolder);
                    return null;
                }
                throw "editor is null for editorHolder: " + editorHolder;
            }
            return editorHolder;
        }
    }
    // list of editor instances
    MonacoEditor.editors = [];
    // constructor
    (() => {
        // this will force loading the monaco library. It happens on the app startup.
        require.config({ paths: { "vs": "_content/DevToys.Blazor/wwwroot/lib/monaco-editor/min/vs" } });
        require(["vs/editor/editor.main"]);
    })();
    MonacoEditor.TextModel = class {
        static getModel(uriStr) {
            const uri = monaco.Uri.parse(uriStr);
            if (uri == null) {
                return null;
            }
            return monaco.editor.getModel(uri);
        }
        static getOptions(uriStr) {
            const model = this.getModel(uriStr);
            return model.getOptions();
        }
        static getVersionId(uriStr) {
            const model = this.getModel(uriStr);
            return model.getVersionId();
        }
        static getAlternativeVersionId(uriStr) {
            const model = this.getModel(uriStr);
            return model.getAlternativeVersionId();
        }
        static setValue(uriStr, newValue) {
            const model = this.getModel(uriStr);
            model.setValue(newValue);
        }
        static getValue(uriStr, eol, preserveBOM) {
            const model = this.getModel(uriStr);
            return model.getValue(eol, preserveBOM);
        }
        static getValueLength(uriStr, eol, preserveBOM) {
            const model = this.getModel(uriStr);
            return model.getValueLength(eol, preserveBOM);
        }
        static getValueInRange(uriStr, range, eol) {
            const model = this.getModel(uriStr);
            return model.getValueInRange(range, eol);
        }
        static getValueLengthInRange(uriStr, range) {
            const model = this.getModel(uriStr);
            return model.getValueLengthInRange(range);
        }
        static getCharacterCountInRange(uriStr, range) {
            const model = this.getModel(uriStr);
            return model.getCharacterCountInRange(range);
        }
        static getLineCount(uriStr) {
            const model = this.getModel(uriStr);
            return model.getLineCount();
        }
        static getLineContent(uriStr, lineNumber) {
            const model = this.getModel(uriStr);
            return model.getLineContent(lineNumber);
        }
        static getLineLength(uriStr, lineNumber) {
            const model = this.getModel(uriStr);
            return model.getLineLength(lineNumber);
        }
        static getLinesContent(uriStr) {
            const model = this.getModel(uriStr);
            return model.getLinesContent();
        }
        static getEOL(uriStr) {
            const model = this.getModel(uriStr);
            return model.getEOL();
        }
        static getEndOfLineSequence(uriStr) {
            const model = this.getModel(uriStr);
            return model.getEndOfLineSequence();
        }
        static getLineMinColumn(uriStr, lineNumber) {
            const model = this.getModel(uriStr);
            return model.getLineMinColumn(lineNumber);
        }
        static getLineMaxColumn(uriStr, lineNumber) {
            const model = this.getModel(uriStr);
            return model.getLineMaxColumn(lineNumber);
        }
        static getLineFirstNonWhitespaceColumn(uriStr, lineNumber) {
            const model = this.getModel(uriStr);
            return model.getLineFirstNonWhitespaceColumn(lineNumber);
        }
        static getLineLastNonWhitespaceColumn(uriStr, lineNumber) {
            const model = this.getModel(uriStr);
            return model.getLineLastNonWhitespaceColumn(lineNumber);
        }
        static validatePosition(uriStr, position) {
            const model = this.getModel(uriStr);
            return model.validatePosition(position);
        }
        static modifyPosition(uriStr, position, offset) {
            const model = this.getModel(uriStr);
            return model.modifyPosition(position, offset);
        }
        static validateRange(uriStr, range) {
            const model = this.getModel(uriStr);
            return model.validateRange(range);
        }
        static getOffsetAt(uriStr, position) {
            const model = this.getModel(uriStr);
            return model.getOffsetAt(position);
        }
        static getPositionAt(uriStr, offset) {
            const model = this.getModel(uriStr);
            return model.getPositionAt(offset);
        }
        static getFullModelRange(uriStr) {
            const model = this.getModel(uriStr);
            return model.getFullModelRange();
        }
        static isDisposed(uriStr) {
            const model = this.getModel(uriStr);
            return model.isDisposed();
        }
        static findMatches(uriStr, searchString, searchScope_or_searchOnlyEditableRange, isRegex, matchCase, wordSeparators, captureMatches, limitResultCount) {
            const model = this.getModel(uriStr);
            return model.findMatches(searchString, searchScope_or_searchOnlyEditableRange, isRegex, matchCase, wordSeparators, captureMatches, limitResultCount);
        }
        static findNextMatch(uriStr, searchString, searchStart, isRegex, matchCase, wordSeparators, captureMatches) {
            const model = this.getModel(uriStr);
            return model.findNextMatch(searchString, searchStart, isRegex, matchCase, wordSeparators, captureMatches);
        }
        static findPreviousMatch(uriStr, searchString, searchStart, isRegex, matchCase, wordSeparators, captureMatches) {
            const model = this.getModel(uriStr);
            return model.findPreviousMatch(searchString, searchStart, isRegex, matchCase, wordSeparators, captureMatches);
        }
        static getLanguageId(uriStr) {
            const model = this.getModel(uriStr);
            return model.getLanguageId();
        }
        static getWordAtPosition(uriStr, position) {
            const model = this.getModel(uriStr);
            return model.getWordAtPosition(position);
        }
        static getWordUntilPosition(uriStr, position) {
            const model = this.getModel(uriStr);
            return model.getWordUntilPosition(position);
        }
        static deltaDecorations(uriStr, oldDecorations, newDecorations, ownerId) {
            const model = this.getModel(uriStr);
            return model.deltaDecorations(oldDecorations, newDecorations, ownerId);
        }
        static getDecorationOptions(uriStr, id) {
            const model = this.getModel(uriStr);
            return model.getDecorationOptions(id);
        }
        static getDecorationRange(uriStr, id) {
            const model = this.getModel(uriStr);
            return model.getDecorationRange(id);
        }
        static getLineDecorations(uriStr, lineNumber, ownerId, filterOutValidation) {
            const model = this.getModel(uriStr);
            return model.getLineDecorations(lineNumber, ownerId, filterOutValidation);
        }
        static getLinesDecorations(uriStr, startLineNumber, endLineNumber, ownerId, filterOutValidation) {
            const model = this.getModel(uriStr);
            return model.getLinesDecorations(startLineNumber, endLineNumber, ownerId, filterOutValidation);
        }
        static getDecorationsInRange(uriStr, range, ownerId, filterOutValidation) {
            const model = this.getModel(uriStr);
            return model.getDecorationsInRange(range, ownerId, filterOutValidation);
        }
        static getAllDecorations(uriStr, ownerId, filterOutValidation) {
            const model = this.getModel(uriStr);
            return model.getAllDecorations(ownerId, filterOutValidation);
        }
        static getInjectedTextDecorations(uriStr, ownerId) {
            const model = this.getModel(uriStr);
            return model.getInjectedTextDecorations(ownerId);
        }
        static getOverviewRulerDecorations(uriStr, ownerId, filterOutValidation) {
            const model = this.getModel(uriStr);
            return model.getOverviewRulerDecorations(ownerId, filterOutValidation);
        }
        static normalizeIndentation(uriStr, str) {
            const model = this.getModel(uriStr);
            return model.normalizeIndentation(str);
        }
        static updateOptions(uriStr, newOpts) {
            const model = this.getModel(uriStr);
            return model.updateOptions(newOpts);
        }
        static detectIndentation(uriStr, defaultInsertSpaces, defaultTabSize) {
            const model = this.getModel(uriStr);
            model.detectIndentation(defaultInsertSpaces, defaultTabSize);
        }
        static pushStackElement(uriStr) {
            const model = this.getModel(uriStr);
            model.pushStackElement();
        }
        static popStackElement(uriStr) {
            const model = this.getModel(uriStr);
            model.popStackElement();
        }
        static pushEOL(uriStr, eol) {
            const model = this.getModel(uriStr);
            model.pushEOL(eol);
        }
        static applyEdits(uriStr, operations, computeUndoEdits) {
            const model = this.getModel(uriStr);
            model.applyEdits(operations, computeUndoEdits);
        }
        static setEOL(uriStr, eol) {
            const model = this.getModel(uriStr);
            model.setEOL(eol);
        }
        static dispose(uriStr) {
            const model = this.getModel(uriStr);
            model.dispose();
        }
    };

    exports.DOM = DOM;
    exports.MonacoEditor = MonacoEditor;
    exports.Popover = Popover;

    Object.defineProperty(exports, '__esModule', { value: true });

    return exports;

})({});
//# sourceMappingURL=index.esm.js.map
