/*! split-grid - v1.0.11 */

(function (global, factory) {
    typeof exports === "object" && typeof module !== "undefined" ? module.exports = factory() :
        typeof define === "function" && define.amd ? define(factory) :
            (global = typeof globalThis !== "undefined" ? globalThis : global || self, global.Split = factory());
}(this, (function () {
    "use strict";

    var numeric = function (value, unit) { return Number(value.slice(0, -1 * unit.length)); };

    var parseValue = function (value) {
        if (value.endsWith("px")) { return { value: value, type: "px", numeric: numeric(value, "px") }; }
        if (value.endsWith("fr")) { return { value: value, type: "fr", numeric: numeric(value, "fr") }; }
        if (value.endsWith("%")) { return { value: value, type: "%", numeric: numeric(value, "%") }; }
        if (value === "auto") { return { value: value, type: "auto" }; }
        return null;
    };

    var parse = function (rule) { return rule.split(" ").map(parseValue); };

    var getSizeAtTrack = function (index, tracks, gap, end) {
        if (gap === void 0) { gap = 0; }
        if (end === void 0) { end = false; }

        var newIndex = end ? index + 1 : index;
        var trackSum = tracks
            .slice(0, newIndex)
            .reduce(function (accum, value) { return accum + value.numeric; }, 0);
        var gapSum = gap ? index * gap : 0;

        return trackSum + gapSum;
    };

    var getStyles = function (rule, ownRules, matchedRules) {
        return ownRules.concat(matchedRules)
            .map(function (r) { return r.style[rule]; })
            .filter(function (style) { return style !== undefined && style !== ""; });
    };

    var getGapValue = function (unit, size) {
        if (size.endsWith(unit)) {
            return Number(size.slice(0, -1 * unit.length));
        }
        return null;
    };

    var firstNonZero = function (tracks) {
        // eslint-disable-next-line no-plusplus
        for (var i = 0; i < tracks.length; i++) {
            if (tracks[i].numeric > 0) {
                return i;
            }
        }
        return null;
    };

    var NOOP = function () { return false; };

    var defaultWriteStyle = function (element, gridTemplateProp, style) {
        // eslint-disable-next-line no-param-reassign
        element.style[gridTemplateProp] = style;
    };

    var getOption = function (options, propName, def) {
        var value = options[propName];
        if (value !== undefined) {
            return value;
        }
        return def;
    };

    function getMatchedCSSRules(el) {
        var ref;

        return (ref = [])
            .concat.apply(
                ref, Array.from(el.ownerDocument.styleSheets).map(function (s) {
                    var rules = [];

                    try {
                        rules = Array.from(s.cssRules || []);
                    } catch (e) {
                        // Ignore results on security error
                    }

                    return rules;
                })
            )
            .filter(function (r) {
                var matches = false;
                try {
                    matches = el.matches(r.selectorText);
                } catch (e) {
                    // Ignore matching erros
                }

                return matches;
            });
    }

    var gridTemplatePropColumns = "grid-template-columns";
    var gridTemplatePropRows = "grid-template-rows";

    var Gutter = function Gutter(direction, options, parentOptions) {
        this.direction = direction;
        this.element = options.element;
        this.track = options.track;

        if (direction === "column") {
            this.gridTemplateProp = gridTemplatePropColumns;
            this.gridGapProp = "grid-column-gap";
            this.cursor = getOption(
                parentOptions,
                "columnCursor",
                getOption(parentOptions, "cursor", "col-resize")
            );
            this.snapOffset = getOption(
                parentOptions,
                "columnSnapOffset",
                getOption(parentOptions, "snapOffset", 30)
            );
            this.dragInterval = getOption(
                parentOptions,
                "columnDragInterval",
                getOption(parentOptions, "dragInterval", 1)
            );
            this.clientAxis = "clientX";
            this.optionStyle = getOption(parentOptions, "gridTemplateColumns");
        } else if (direction === "row") {
            this.gridTemplateProp = gridTemplatePropRows;
            this.gridGapProp = "grid-row-gap";
            this.cursor = getOption(
                parentOptions,
                "rowCursor",
                getOption(parentOptions, "cursor", "row-resize")
            );
            this.snapOffset = getOption(
                parentOptions,
                "rowSnapOffset",
                getOption(parentOptions, "snapOffset", 30)
            );
            this.dragInterval = getOption(
                parentOptions,
                "rowDragInterval",
                getOption(parentOptions, "dragInterval", 1)
            );
            this.clientAxis = "clientY";
            this.optionStyle = getOption(parentOptions, "gridTemplateRows");
        }

        this.onDragStart = getOption(parentOptions, "onDragStart", NOOP);
        this.onDragEnd = getOption(parentOptions, "onDragEnd", NOOP);
        this.onDrag = getOption(parentOptions, "onDrag", NOOP);
        this.writeStyle = getOption(
            parentOptions,
            "writeStyle",
            defaultWriteStyle
        );

        this.startDragging = this.startDragging.bind(this);
        this.stopDragging = this.stopDragging.bind(this);
        this.drag = this.drag.bind(this);

        this.minSizeStart = options.minSizeStart;
        this.minSizeEnd = options.minSizeEnd;

        if (options.element) {
            this.element.addEventListener("mousedown", this.startDragging);
            this.element.addEventListener("touchstart", this.startDragging);
        }
    };

    Gutter.prototype.getDimensions = function getDimensions() {
        var ref = this.grid.getBoundingClientRect();
        var width = ref.width;
        var height = ref.height;
        var top = ref.top;
        var bottom = ref.bottom;
        var left = ref.left;
        var right = ref.right;

        if (this.direction === "column") {
            this.start = top;
            this.end = bottom;
            this.size = height;
        } else if (this.direction === "row") {
            this.start = left;
            this.end = right;
            this.size = width;
        }
    };

    Gutter.prototype.getSizeAtTrack = function getSizeAtTrack$1(track, end) {
        return getSizeAtTrack(
            track,
            this.computedPixels,
            this.computedGapPixels,
            end
        );
    };

    Gutter.prototype.getSizeOfTrack = function getSizeOfTrack(track) {
        return this.computedPixels[track].numeric;
    };

    Gutter.prototype.getRawTracks = function getRawTracks() {
        var tracks = getStyles(
            this.gridTemplateProp,
            [this.grid],
            getMatchedCSSRules(this.grid)
        );
        if (!tracks.length) {
            if (this.optionStyle) { return this.optionStyle; }

            throw Error("Unable to determine grid template tracks from styles.");
        }
        return tracks[0];
    };

    Gutter.prototype.getGap = function getGap() {
        var gap = getStyles(
            this.gridGapProp,
            [this.grid],
            getMatchedCSSRules(this.grid)
        );
        if (!gap.length) {
            return null;
        }
        return gap[0];
    };

    Gutter.prototype.getRawComputedTracks = function getRawComputedTracks() {
        return window.getComputedStyle(this.grid)[this.gridTemplateProp];
    };

    Gutter.prototype.getRawComputedGap = function getRawComputedGap() {
        return window.getComputedStyle(this.grid)[this.gridGapProp];
    };

    Gutter.prototype.setTracks = function setTracks(raw) {
        this.tracks = raw.split(" ");
        this.trackValues = parse(raw);
    };

    Gutter.prototype.setComputedTracks = function setComputedTracks(raw) {
        this.computedTracks = raw.split(" ");
        this.computedPixels = parse(raw);
    };

    Gutter.prototype.setGap = function setGap(raw) {
        this.gap = raw;
    };

    Gutter.prototype.setComputedGap = function setComputedGap(raw) {
        this.computedGap = raw;
        this.computedGapPixels = getGapValue("px", this.computedGap) || 0;
    };

    Gutter.prototype.getMousePosition = function getMousePosition(e) {
        if ("touches" in e) { return e.touches[0][this.clientAxis]; }
        return e[this.clientAxis];
    };

    Gutter.prototype.startDragging = function startDragging(e) {
        if ("button" in e && e.button !== 0) {
            return;
        }

        // Don't actually drag the element. We emulate that in the drag function.
        e.preventDefault();

        if (this.element) {
            this.grid = this.element.parentNode;
        } else {
            this.grid = e.target.parentNode;
        }

        this.getDimensions();
        this.setTracks(this.getRawTracks());
        this.setComputedTracks(this.getRawComputedTracks());
        this.setGap(this.getGap());
        this.setComputedGap(this.getRawComputedGap());

        var trackPercentage = this.trackValues.filter(
            function (track) { return track.type === "%"; }
        );
        var trackFr = this.trackValues.filter(function (track) { return track.type === "fr"; });

        this.totalFrs = trackFr.length;

        if (this.totalFrs) {
            var track = firstNonZero(trackFr);

            if (track !== null) {
                this.frToPixels =
                    this.computedPixels[track].numeric / trackFr[track].numeric;
            }
        }

        if (trackPercentage.length) {
            var track$1 = firstNonZero(trackPercentage);

            if (track$1 !== null) {
                this.percentageToPixels =
                    this.computedPixels[track$1].numeric /
                    trackPercentage[track$1].numeric;
            }
        }

        // get start of gutter track
        var gutterStart = this.getSizeAtTrack(this.track, false) + this.start;
        this.dragStartOffset = this.getMousePosition(e) - gutterStart;

        this.aTrack = this.track - 1;

        if (this.track < this.tracks.length - 1) {
            this.bTrack = this.track + 1;
        } else {
            throw Error(
                ("Invalid track index: " + (this.track) + ". Track must be between two other tracks and only " + (this.tracks.length) + " tracks were found.")
            );
        }

        this.aTrackStart = this.getSizeAtTrack(this.aTrack, false) + this.start;
        this.bTrackEnd = this.getSizeAtTrack(this.bTrack, true) + this.start;

        // Set the dragging property of the pair object.
        this.dragging = true;

        // All the binding. `window` gets the stop events in case we drag out of the elements.
        window.addEventListener("mouseup", this.stopDragging);
        window.addEventListener("touchend", this.stopDragging);
        window.addEventListener("touchcancel", this.stopDragging);
        window.addEventListener("mousemove", this.drag);
        window.addEventListener("touchmove", this.drag);

        // Disable selection. Disable!
        this.grid.addEventListener("selectstart", NOOP);
        this.grid.addEventListener("dragstart", NOOP);

        this.grid.style.userSelect = "none";
        this.grid.style.webkitUserSelect = "none";
        this.grid.style.MozUserSelect = "none";
        this.grid.style.pointerEvents = "none";

        // Set the cursor at multiple levels
        this.grid.style.cursor = this.cursor;
        window.document.body.style.cursor = this.cursor;

        this.onDragStart(this.direction, this.track);
    };

    Gutter.prototype.stopDragging = function stopDragging() {
        this.dragging = false;

        // Remove the stored event listeners. This is why we store them.
        this.cleanup();

        this.onDragEnd(this.direction, this.track);

        if (this.needsDestroy) {
            if (this.element) {
                this.element.removeEventListener(
                    "mousedown",
                    this.startDragging
                );
                this.element.removeEventListener(
                    "touchstart",
                    this.startDragging
                );
            }
            this.destroyCb();
            this.needsDestroy = false;
            this.destroyCb = null;
        }
    };

    Gutter.prototype.drag = function drag(e) {
        var mousePosition = this.getMousePosition(e);

        var gutterSize = this.getSizeOfTrack(this.track);
        var minMousePosition =
            this.aTrackStart +
            this.minSizeStart +
            this.dragStartOffset +
            this.computedGapPixels;
        var maxMousePosition =
            this.bTrackEnd -
            this.minSizeEnd -
            this.computedGapPixels -
            (gutterSize - this.dragStartOffset);
        var minMousePositionOffset = minMousePosition + this.snapOffset;
        var maxMousePositionOffset = maxMousePosition - this.snapOffset;

        if (mousePosition < minMousePositionOffset) {
            mousePosition = minMousePosition;
        }

        if (mousePosition > maxMousePositionOffset) {
            mousePosition = maxMousePosition;
        }

        if (mousePosition < minMousePosition) {
            mousePosition = minMousePosition;
        } else if (mousePosition > maxMousePosition) {
            mousePosition = maxMousePosition;
        }

        var aTrackSize =
            mousePosition -
            this.aTrackStart -
            this.dragStartOffset -
            this.computedGapPixels;
        var bTrackSize =
            this.bTrackEnd -
            mousePosition +
            this.dragStartOffset -
            gutterSize -
            this.computedGapPixels;

        if (this.dragInterval > 1) {
            var aTrackSizeIntervaled =
                Math.round(aTrackSize / this.dragInterval) * this.dragInterval;
            bTrackSize -= aTrackSizeIntervaled - aTrackSize;
            aTrackSize = aTrackSizeIntervaled;
        }

        if (aTrackSize < this.minSizeStart) {
            aTrackSize = this.minSizeStart;
        }

        if (bTrackSize < this.minSizeEnd) {
            bTrackSize = this.minSizeEnd;
        }

        if (this.trackValues[this.aTrack].type === "px") {
            this.tracks[this.aTrack] = aTrackSize + "px";
        } else if (this.trackValues[this.aTrack].type === "fr") {
            if (this.totalFrs === 1) {
                this.tracks[this.aTrack] = "1fr";
            } else {
                var targetFr = aTrackSize / this.frToPixels;
                this.tracks[this.aTrack] = targetFr + "fr";
            }
        } else if (this.trackValues[this.aTrack].type === "%") {
            var targetPercentage = aTrackSize / this.percentageToPixels;
            this.tracks[this.aTrack] = targetPercentage + "%";
        }

        if (this.trackValues[this.bTrack].type === "px") {
            this.tracks[this.bTrack] = bTrackSize + "px";
        } else if (this.trackValues[this.bTrack].type === "fr") {
            if (this.totalFrs === 1) {
                this.tracks[this.bTrack] = "1fr";
            } else {
                var targetFr$1 = bTrackSize / this.frToPixels;
                this.tracks[this.bTrack] = targetFr$1 + "fr";
            }
        } else if (this.trackValues[this.bTrack].type === "%") {
            var targetPercentage$1 = bTrackSize / this.percentageToPixels;
            this.tracks[this.bTrack] = targetPercentage$1 + "%";
        }

        var style = this.tracks.join(" ");
        this.writeStyle(this.grid, this.gridTemplateProp, style);
        this.onDrag(this.direction, this.track, style);
    };

    Gutter.prototype.cleanup = function cleanup() {
        window.removeEventListener("mouseup", this.stopDragging);
        window.removeEventListener("touchend", this.stopDragging);
        window.removeEventListener("touchcancel", this.stopDragging);
        window.removeEventListener("mousemove", this.drag);
        window.removeEventListener("touchmove", this.drag);

        if (this.grid) {
            this.grid.removeEventListener("selectstart", NOOP);
            this.grid.removeEventListener("dragstart", NOOP);

            this.grid.style.userSelect = "";
            this.grid.style.webkitUserSelect = "";
            this.grid.style.MozUserSelect = "";
            this.grid.style.pointerEvents = "";

            this.grid.style.cursor = "";
        }

        window.document.body.style.cursor = "";
    };

    Gutter.prototype.destroy = function destroy(immediate, cb) {
        if (immediate === void 0) immediate = true;

        if (immediate || this.dragging === false) {
            this.cleanup();
            if (this.element) {
                this.element.removeEventListener(
                    "mousedown",
                    this.startDragging
                );
                this.element.removeEventListener(
                    "touchstart",
                    this.startDragging
                );
            }

            if (cb) {
                cb();
            }
        } else {
            this.needsDestroy = true;
            if (cb) {
                this.destroyCb = cb;
            }
        }
    };

    var getTrackOption = function (options, track, defaultValue) {
        if (track in options) {
            return options[track];
        }

        return defaultValue;
    };

    var createGutter = function (direction, options) {
        return function (gutterOptions) {
            if (gutterOptions.track < 1) {
                throw Error(
                    ("Invalid track index: " + (gutterOptions.track) + ". Track must be between two other tracks.")
                );
            }

            var trackMinSizes =
                direction === "column"
                    ? options.columnMinSizes || {}
                    : options.rowMinSizes || {};
            var trackMinSize = direction === "column" ? "columnMinSize" : "rowMinSize";

            return new Gutter(
                direction,
                Object.assign({}, {
                    minSizeStart: getTrackOption(
                        trackMinSizes,
                        gutterOptions.track - 1,
                        getOption(
                            options,
                            trackMinSize,
                            getOption(options, "minSize", 0)
                        )
                    ),
                    minSizeEnd: getTrackOption(
                        trackMinSizes,
                        gutterOptions.track + 1,
                        getOption(
                            options,
                            trackMinSize,
                            getOption(options, "minSize", 0)
                        )
                    )
                },
                gutterOptions),
                options
            );
        };
    };

    var Grid = function Grid(options) {
        var this$1 = this;

        this.columnGutters = {};
        this.rowGutters = {};

        this.options = Object.assign({}, {
            columnGutters: options.columnGutters || [],
            rowGutters: options.rowGutters || [],
            columnMinSizes: options.columnMinSizes || {},
            rowMinSizes: options.rowMinSizes || {}
        },
        options);

        this.options.columnGutters.forEach(function (gutterOptions) {
            this$1.columnGutters[gutterOptions.track] = createGutter(
                "column",
                this$1.options
            )(gutterOptions);
        });

        this.options.rowGutters.forEach(function (gutterOptions) {
            this$1.rowGutters[gutterOptions.track] = createGutter(
                "row",
                this$1.options
            )(gutterOptions);
        });
    };

    Grid.prototype.addColumnGutter = function addColumnGutter(element, track) {
        if (this.columnGutters[track]) {
            this.columnGutters[track].destroy();
        }

        this.columnGutters[track] = createGutter(
            "column",
            this.options
        )({
            element: element,
            track: track,
        });
    };

    Grid.prototype.addRowGutter = function addRowGutter(element, track) {
        if (this.rowGutters[track]) {
            this.rowGutters[track].destroy();
        }

        this.rowGutters[track] = createGutter(
            "row",
            this.options
        )({
            element: element,
            track: track,
        });
    };

    Grid.prototype.removeColumnGutter = function removeColumnGutter(track, immediate) {
        var this$1 = this;
        if (immediate === void 0) immediate = true;

        if (this.columnGutters[track]) {
            this.columnGutters[track].destroy(immediate, function () {
                delete this$1.columnGutters[track];
            });
        }
    };

    Grid.prototype.removeRowGutter = function removeRowGutter(track, immediate) {
        var this$1 = this;
        if (immediate === void 0) immediate = true;

        if (this.rowGutters[track]) {
            this.rowGutters[track].destroy(immediate, function () {
                delete this$1.rowGutters[track];
            });
        }
    };

    Grid.prototype.handleDragStart = function handleDragStart(e, direction, track) {
        if (direction === "column") {
            if (this.columnGutters[track]) {
                this.columnGutters[track].destroy();
            }

            this.columnGutters[track] = createGutter(
                "column",
                this.options
            )({
                track: track,
            });
            this.columnGutters[track].startDragging(e);
        } else if (direction === "row") {
            if (this.rowGutters[track]) {
                this.rowGutters[track].destroy();
            }

            this.rowGutters[track] = createGutter(
                "row",
                this.options
            )({
                track: track,
            });
            this.rowGutters[track].startDragging(e);
        }
    };

    Grid.prototype.destroy = function destroy(immediate) {
        var this$1 = this;
        if (immediate === void 0) immediate = true;

        Object.keys(this.columnGutters).forEach(function (track) {
            return this$1.columnGutters[track].destroy(immediate, function () {
                delete this$1.columnGutters[track];
            });
        }
        );
        Object.keys(this.rowGutters).forEach(function (track) {
            return this$1.rowGutters[track].destroy(immediate, function () {
                delete this$1.rowGutters[track];
            });
        }
        );
    };

    function index(options) { return new Grid(options); }

    return index;

})));