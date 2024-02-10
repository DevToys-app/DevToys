// NOTE:
// This is a fork of simplebar - v6.2.5. Some custom changes have been made. Be careful if you decide to update this library.

/**
 * simplebar - v6.2.5
 * Scrollbars, simpler.
 * https://grsmto.github.io/simplebar/
 *
 * Made by Adrien Denat from a fork by Jonathan Nicol
 * Under MIT License
 */

var SimpleBar = (function () {
    "use strict";

    /******************************************************************************
    Copyright (c) Microsoft Corporation.

    Permission to use, copy, modify, and/or distribute this software for any
    purpose with or without fee is hereby granted.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
    REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
    AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
    INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
    LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
    OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
    PERFORMANCE OF THIS SOFTWARE.
    ***************************************************************************** */
    /* global Reflect, Promise */

    var extendStatics = function(d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };

    function __extends(d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    }

    var canUseDOM = !!(
        typeof window !== "undefined" &&
      window.document &&
      window.document.createElement
    );

    var canUseDom = canUseDOM;

    /** Detect free variable `global` from Node.js. */
    var freeGlobal = typeof global == "object" && global && global.Object === Object && global;

    var freeGlobal$1 = freeGlobal;

    /** Detect free variable `self`. */
    var freeSelf = typeof self == "object" && self && self.Object === Object && self;

    /** Used as a reference to the global object. */
    var root = freeGlobal$1 || freeSelf || Function("return this")();

    var root$1 = root;

    /** Built-in value references. */
    var Symbol = root$1.Symbol;

    var Symbol$1 = Symbol;

    /** Used for built-in method references. */
    var objectProto$1 = Object.prototype;

    /** Used to check objects for own properties. */
    var hasOwnProperty = objectProto$1.hasOwnProperty;

    /**
     * Used to resolve the
     * [`toStringTag`](http://ecma-international.org/ecma-262/7.0/#sec-object.prototype.tostring)
     * of values.
     */
    var nativeObjectToString$1 = objectProto$1.toString;

    /** Built-in value references. */
    var symToStringTag$1 = Symbol$1 ? Symbol$1.toStringTag : undefined;

    /**
     * A specialized version of `baseGetTag` which ignores `Symbol.toStringTag` values.
     *
     * @private
     * @param {*} value The value to query.
     * @returns {string} Returns the raw `toStringTag`.
     */
    function getRawTag(value) {
        var isOwn = hasOwnProperty.call(value, symToStringTag$1),
            tag = value[symToStringTag$1];

        try {
            value[symToStringTag$1] = undefined;
            var unmasked = true;
        } catch (e) {}

        var result = nativeObjectToString$1.call(value);
        if (unmasked) {
            if (isOwn) {
                value[symToStringTag$1] = tag;
            } else {
                delete value[symToStringTag$1];
            }
        }
        return result;
    }

    /** Used for built-in method references. */
    var objectProto = Object.prototype;

    /**
     * Used to resolve the
     * [`toStringTag`](http://ecma-international.org/ecma-262/7.0/#sec-object.prototype.tostring)
     * of values.
     */
    var nativeObjectToString = objectProto.toString;

    /**
     * Converts `value` to a string using `Object.prototype.toString`.
     *
     * @private
     * @param {*} value The value to convert.
     * @returns {string} Returns the converted string.
     */
    function objectToString(value) {
        return nativeObjectToString.call(value);
    }

    /** `Object#toString` result references. */
    var nullTag = "[object Null]",
        undefinedTag = "[object Undefined]";

    /** Built-in value references. */
    var symToStringTag = Symbol$1 ? Symbol$1.toStringTag : undefined;

    /**
     * The base implementation of `getTag` without fallbacks for buggy environments.
     *
     * @private
     * @param {*} value The value to query.
     * @returns {string} Returns the `toStringTag`.
     */
    function baseGetTag(value) {
        if (value == null) {
            return value === undefined ? undefinedTag : nullTag;
        }
        return (symToStringTag && symToStringTag in Object(value))
            ? getRawTag(value)
            : objectToString(value);
    }

    /**
     * Checks if `value` is object-like. A value is object-like if it's not `null`
     * and has a `typeof` result of "object".
     *
     * @static
     * @memberOf _
     * @since 4.0.0
     * @category Lang
     * @param {*} value The value to check.
     * @returns {boolean} Returns `true` if `value` is object-like, else `false`.
     * @example
     *
     * _.isObjectLike({});
     * // => true
     *
     * _.isObjectLike([1, 2, 3]);
     * // => true
     *
     * _.isObjectLike(_.noop);
     * // => false
     *
     * _.isObjectLike(null);
     * // => false
     */
    function isObjectLike(value) {
        return value != null && typeof value == "object";
    }

    /** `Object#toString` result references. */
    var symbolTag = "[object Symbol]";

    /**
     * Checks if `value` is classified as a `Symbol` primitive or object.
     *
     * @static
     * @memberOf _
     * @since 4.0.0
     * @category Lang
     * @param {*} value The value to check.
     * @returns {boolean} Returns `true` if `value` is a symbol, else `false`.
     * @example
     *
     * _.isSymbol(Symbol.iterator);
     * // => true
     *
     * _.isSymbol('abc');
     * // => false
     */
    function isSymbol(value) {
        return typeof value == "symbol" ||
        (isObjectLike(value) && baseGetTag(value) == symbolTag);
    }

    /** Used to match a single whitespace character. */
    var reWhitespace = /\s/;

    /**
     * Used by `_.trim` and `_.trimEnd` to get the index of the last non-whitespace
     * character of `string`.
     *
     * @private
     * @param {string} string The string to inspect.
     * @returns {number} Returns the index of the last non-whitespace character.
     */
    function trimmedEndIndex(string) {
        var index = string.length;

        while (index-- && reWhitespace.test(string.charAt(index))) {}
        return index;
    }

    /** Used to match leading whitespace. */
    var reTrimStart = /^\s+/;

    /**
     * The base implementation of `_.trim`.
     *
     * @private
     * @param {string} string The string to trim.
     * @returns {string} Returns the trimmed string.
     */
    function baseTrim(string) {
        return string
            ? string.slice(0, trimmedEndIndex(string) + 1).replace(reTrimStart, "")
            : string;
    }

    /**
     * Checks if `value` is the
     * [language type](http://www.ecma-international.org/ecma-262/7.0/#sec-ecmascript-language-types)
     * of `Object`. (e.g. arrays, functions, objects, regexes, `new Number(0)`, and `new String('')`)
     *
     * @static
     * @memberOf _
     * @since 0.1.0
     * @category Lang
     * @param {*} value The value to check.
     * @returns {boolean} Returns `true` if `value` is an object, else `false`.
     * @example
     *
     * _.isObject({});
     * // => true
     *
     * _.isObject([1, 2, 3]);
     * // => true
     *
     * _.isObject(_.noop);
     * // => true
     *
     * _.isObject(null);
     * // => false
     */
    function isObject(value) {
        var type = typeof value;
        return value != null && (type == "object" || type == "function");
    }

    /** Used as references for various `Number` constants. */
    var NAN = 0 / 0;

    /** Used to detect bad signed hexadecimal string values. */
    var reIsBadHex = /^[-+]0x[0-9a-f]+$/i;

    /** Used to detect binary string values. */
    var reIsBinary = /^0b[01]+$/i;

    /** Used to detect octal string values. */
    var reIsOctal = /^0o[0-7]+$/i;

    /** Built-in method references without a dependency on `root`. */
    var freeParseInt = parseInt;

    /**
     * Converts `value` to a number.
     *
     * @static
     * @memberOf _
     * @since 4.0.0
     * @category Lang
     * @param {*} value The value to process.
     * @returns {number} Returns the number.
     * @example
     *
     * _.toNumber(3.2);
     * // => 3.2
     *
     * _.toNumber(Number.MIN_VALUE);
     * // => 5e-324
     *
     * _.toNumber(Infinity);
     * // => Infinity
     *
     * _.toNumber('3.2');
     * // => 3.2
     */
    function toNumber(value) {
        if (typeof value == "number") {
            return value;
        }
        if (isSymbol(value)) {
            return NAN;
        }
        if (isObject(value)) {
            var other = typeof value.valueOf == "function" ? value.valueOf() : value;
            value = isObject(other) ? (other + "") : other;
        }
        if (typeof value != "string") {
            return value === 0 ? value : +value;
        }
        value = baseTrim(value);
        var isBinary = reIsBinary.test(value);
        return (isBinary || reIsOctal.test(value))
            ? freeParseInt(value.slice(2), isBinary ? 2 : 8)
            : (reIsBadHex.test(value) ? NAN : +value);
    }

    /**
     * Gets the timestamp of the number of milliseconds that have elapsed since
     * the Unix epoch (1 January 1970 00:00:00 UTC).
     *
     * @static
     * @memberOf _
     * @since 2.4.0
     * @category Date
     * @returns {number} Returns the timestamp.
     * @example
     *
     * _.defer(function(stamp) {
     *   console.log(_.now() - stamp);
     * }, _.now());
     * // => Logs the number of milliseconds it took for the deferred invocation.
     */
    var now = function() {
        return root$1.Date.now();
    };

    var now$1 = now;

    /** Error message constants. */
    var FUNC_ERROR_TEXT$1 = "Expected a function";

    /* Built-in method references for those with the same name as other `lodash` methods. */
    var nativeMax = Math.max,
        nativeMin = Math.min;

    /**
     * Creates a debounced function that delays invoking `func` until after `wait`
     * milliseconds have elapsed since the last time the debounced function was
     * invoked. The debounced function comes with a `cancel` method to cancel
     * delayed `func` invocations and a `flush` method to immediately invoke them.
     * Provide `options` to indicate whether `func` should be invoked on the
     * leading and/or trailing edge of the `wait` timeout. The `func` is invoked
     * with the last arguments provided to the debounced function. Subsequent
     * calls to the debounced function return the result of the last `func`
     * invocation.
     *
     * **Note:** If `leading` and `trailing` options are `true`, `func` is
     * invoked on the trailing edge of the timeout only if the debounced function
     * is invoked more than once during the `wait` timeout.
     *
     * If `wait` is `0` and `leading` is `false`, `func` invocation is deferred
     * until to the next tick, similar to `setTimeout` with a timeout of `0`.
     *
     * See [David Corbacho's article](https://css-tricks.com/debouncing-throttling-explained-examples/)
     * for details over the differences between `_.debounce` and `_.throttle`.
     *
     * @static
     * @memberOf _
     * @since 0.1.0
     * @category Function
     * @param {Function} func The function to debounce.
     * @param {number} [wait=0] The number of milliseconds to delay.
     * @param {Object} [options={}] The options object.
     * @param {boolean} [options.leading=false]
     *  Specify invoking on the leading edge of the timeout.
     * @param {number} [options.maxWait]
     *  The maximum time `func` is allowed to be delayed before it's invoked.
     * @param {boolean} [options.trailing=true]
     *  Specify invoking on the trailing edge of the timeout.
     * @returns {Function} Returns the new debounced function.
     * @example
     *
     * // Avoid costly calculations while the window size is in flux.
     * jQuery(window).on('resize', _.debounce(calculateLayout, 150));
     *
     * // Invoke `sendMail` when clicked, debouncing subsequent calls.
     * jQuery(element).on('click', _.debounce(sendMail, 300, {
     *   'leading': true,
     *   'trailing': false
     * }));
     *
     * // Ensure `batchLog` is invoked once after 1 second of debounced calls.
     * var debounced = _.debounce(batchLog, 250, { 'maxWait': 1000 });
     * var source = new EventSource('/stream');
     * jQuery(source).on('message', debounced);
     *
     * // Cancel the trailing debounced invocation.
     * jQuery(window).on('popstate', debounced.cancel);
     */
    function debounce(func, wait, options) {
        var lastArgs,
            lastThis,
            maxWait,
            result,
            timerId,
            lastCallTime,
            lastInvokeTime = 0,
            leading = false,
            maxing = false,
            trailing = true;

        if (typeof func != "function") {
            throw new TypeError(FUNC_ERROR_TEXT$1);
        }
        wait = toNumber(wait) || 0;
        if (isObject(options)) {
            leading = !!options.leading;
            maxing = "maxWait" in options;
            maxWait = maxing ? nativeMax(toNumber(options.maxWait) || 0, wait) : maxWait;
            trailing = "trailing" in options ? !!options.trailing : trailing;
        }

        function invokeFunc(time) {
            var args = lastArgs,
                thisArg = lastThis;

            lastArgs = lastThis = undefined;
            lastInvokeTime = time;
            result = func.apply(thisArg, args);
            return result;
        }

        function leadingEdge(time) {
        // Reset any `maxWait` timer.
            lastInvokeTime = time;
            // Start the timer for the trailing edge.
            timerId = setTimeout(timerExpired, wait);
            // Invoke the leading edge.
            return leading ? invokeFunc(time) : result;
        }

        function remainingWait(time) {
            var timeSinceLastCall = time - lastCallTime,
                timeSinceLastInvoke = time - lastInvokeTime,
                timeWaiting = wait - timeSinceLastCall;

            return maxing
                ? nativeMin(timeWaiting, maxWait - timeSinceLastInvoke)
                : timeWaiting;
        }

        function shouldInvoke(time) {
            var timeSinceLastCall = time - lastCallTime,
                timeSinceLastInvoke = time - lastInvokeTime;

            // Either this is the first call, activity has stopped and we're at the
            // trailing edge, the system time has gone backwards and we're treating
            // it as the trailing edge, or we've hit the `maxWait` limit.
            return (lastCallTime === undefined || (timeSinceLastCall >= wait) ||
          (timeSinceLastCall < 0) || (maxing && timeSinceLastInvoke >= maxWait));
        }

        function timerExpired() {
            var time = now$1();
            if (shouldInvoke(time)) {
                return trailingEdge(time);
            }
            // Restart the timer.
            timerId = setTimeout(timerExpired, remainingWait(time));
        }

        function trailingEdge(time) {
            timerId = undefined;

            // Only invoke if we have `lastArgs` which means `func` has been
            // debounced at least once.
            if (trailing && lastArgs) {
                return invokeFunc(time);
            }
            lastArgs = lastThis = undefined;
            return result;
        }

        function cancel() {
            if (timerId !== undefined) {
                clearTimeout(timerId);
            }
            lastInvokeTime = 0;
            lastArgs = lastCallTime = lastThis = timerId = undefined;
        }

        function flush() {
            return timerId === undefined ? result : trailingEdge(now$1());
        }

        function debounced() {
            var time = now$1(),
                isInvoking = shouldInvoke(time);

            lastArgs = arguments;
            lastThis = this;
            lastCallTime = time;

            if (isInvoking) {
                if (timerId === undefined) {
                    return leadingEdge(lastCallTime);
                }
                if (maxing) {
                    // Handle invocations in a tight loop.
                    clearTimeout(timerId);
                    timerId = setTimeout(timerExpired, wait);
                    return invokeFunc(lastCallTime);
                }
            }
            if (timerId === undefined) {
                timerId = setTimeout(timerExpired, wait);
            }
            return result;
        }
        debounced.cancel = cancel;
        debounced.flush = flush;
        return debounced;
    }

    /** Error message constants. */
    var FUNC_ERROR_TEXT = "Expected a function";

    /**
     * Creates a throttled function that only invokes `func` at most once per
     * every `wait` milliseconds. The throttled function comes with a `cancel`
     * method to cancel delayed `func` invocations and a `flush` method to
     * immediately invoke them. Provide `options` to indicate whether `func`
     * should be invoked on the leading and/or trailing edge of the `wait`
     * timeout. The `func` is invoked with the last arguments provided to the
     * throttled function. Subsequent calls to the throttled function return the
     * result of the last `func` invocation.
     *
     * **Note:** If `leading` and `trailing` options are `true`, `func` is
     * invoked on the trailing edge of the timeout only if the throttled function
     * is invoked more than once during the `wait` timeout.
     *
     * If `wait` is `0` and `leading` is `false`, `func` invocation is deferred
     * until to the next tick, similar to `setTimeout` with a timeout of `0`.
     *
     * See [David Corbacho's article](https://css-tricks.com/debouncing-throttling-explained-examples/)
     * for details over the differences between `_.throttle` and `_.debounce`.
     *
     * @static
     * @memberOf _
     * @since 0.1.0
     * @category Function
     * @param {Function} func The function to throttle.
     * @param {number} [wait=0] The number of milliseconds to throttle invocations to.
     * @param {Object} [options={}] The options object.
     * @param {boolean} [options.leading=true]
     *  Specify invoking on the leading edge of the timeout.
     * @param {boolean} [options.trailing=true]
     *  Specify invoking on the trailing edge of the timeout.
     * @returns {Function} Returns the new throttled function.
     * @example
     *
     * // Avoid excessively updating the position while scrolling.
     * jQuery(window).on('scroll', _.throttle(updatePosition, 100));
     *
     * // Invoke `renewToken` when the click event is fired, but not more than once every 5 minutes.
     * var throttled = _.throttle(renewToken, 300000, { 'trailing': false });
     * jQuery(element).on('click', throttled);
     *
     * // Cancel the trailing throttled invocation.
     * jQuery(window).on('popstate', throttled.cancel);
     */
    function throttle(func, wait, options) {
        var leading = true,
            trailing = true;

        if (typeof func != "function") {
            throw new TypeError(FUNC_ERROR_TEXT);
        }
        if (isObject(options)) {
            leading = "leading" in options ? !!options.leading : leading;
            trailing = "trailing" in options ? !!options.trailing : trailing;
        }
        return debounce(func, wait, {
            "leading": leading,
            "maxWait": wait,
            "trailing": trailing
        });
    }

    /**
     * simplebar-core - v1.2.4
     * Scrollbars, simpler.
     * https://grsmto.github.io/simplebar/
     *
     * Made by Adrien Denat from a fork by Jonathan Nicol
     * Under MIT License
     */

    /******************************************************************************
    Copyright (c) Microsoft Corporation.

    Permission to use, copy, modify, and/or distribute this software for any
    purpose with or without fee is hereby granted.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
    REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
    AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
    INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
    LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
    OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
    PERFORMANCE OF THIS SOFTWARE.
    ***************************************************************************** */

    var __assign = function() {
        __assign = Object.assign || function __assign(t) {
            for (var s, i = 1, n = arguments.length; i < n; i++) {
                s = arguments[i];
                for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p)) t[p] = s[p];
            }
            return t;
        };
        return __assign.apply(this, arguments);
    };

    var cachedScrollbarWidth = null;
    var cachedDevicePixelRatio = null;
    if (canUseDom) {
        window.addEventListener("resize", function () {
            if (cachedDevicePixelRatio !== window.devicePixelRatio) {
                cachedDevicePixelRatio = window.devicePixelRatio;
                cachedScrollbarWidth = null;
            }
        });
    }
    function scrollbarWidth() {
        if (cachedScrollbarWidth === null) {
            if (typeof document === "undefined") {
                cachedScrollbarWidth = 0;
                return cachedScrollbarWidth;
            }
            var body = document.body;
            var box = document.createElement("div");
            box.classList.add("simplebar-hide-scrollbar");
            body.appendChild(box);
            var width = box.getBoundingClientRect().right;
            body.removeChild(box);
            cachedScrollbarWidth = width;
        }
        return cachedScrollbarWidth;
    }

    function getElementWindow$1(element) {
        if (!element ||
            !element.ownerDocument ||
            !element.ownerDocument.defaultView) {
            return window;
        }
        return element.ownerDocument.defaultView;
    }
    function getElementDocument$1(element) {
        if (!element || !element.ownerDocument) {
            return document;
        }
        return element.ownerDocument;
    }
    // Helper function to retrieve options from element attributes
    var getOptions$1 = function (obj) {
        var initialObj = {};
        var options = Array.prototype.reduce.call(obj, function (acc, attribute) {
            var option = attribute.name.match(/data-simplebar-(.+)/);
            if (option) {
                var key = option[1].replace(/\W+(.)/g, function (_, chr) { return chr.toUpperCase(); });
                switch (attribute.value) {
                case "true":
                    acc[key] = true;
                    break;
                case "false":
                    acc[key] = false;
                    break;
                case undefined:
                    acc[key] = true;
                    break;
                default:
                    acc[key] = attribute.value;
                }
            }
            return acc;
        }, initialObj);
        return options;
    };
    function addClasses$1(el, classes) {
        var _a;
        if (!el)
            return;
        (_a = el.classList).add.apply(_a, classes.split(" "));
    }
    function removeClasses$1(el, classes) {
        if (!el)
            return;
        classes.split(" ").forEach(function (className) {
            el.classList.remove(className);
        });
    }
    function classNamesToQuery$1(classNames) {
        return ".".concat(classNames.split(" ").join("."));
    }

    var helpers = /*#__PURE__*/Object.freeze({
        __proto__: null,
        getElementWindow: getElementWindow$1,
        getElementDocument: getElementDocument$1,
        getOptions: getOptions$1,
        addClasses: addClasses$1,
        removeClasses: removeClasses$1,
        classNamesToQuery: classNamesToQuery$1
    });

    var getElementWindow = getElementWindow$1, getElementDocument = getElementDocument$1, getOptions$2 = getOptions$1, addClasses$2 = addClasses$1, removeClasses = removeClasses$1, classNamesToQuery = classNamesToQuery$1;
    var SimpleBarCore = /** @class */ (function () {
        function SimpleBarCore(element, options) {
            if (options === void 0) { options = {}; }
            var _this = this;
            this.removePreventClickId = null;
            this.minScrollbarWidth = 20;
            this.stopScrollDelay = 175;
            this.isScrolling = false;
            this.isMouseEntering = false;
            this.scrollXTicking = false;
            this.scrollYTicking = false;
            this.wrapperEl = null;
            this.contentWrapperEl = null;
            this.contentEl = null;
            this.offsetEl = null;
            this.maskEl = null;
            this.placeholderEl = null;
            this.heightAutoObserverWrapperEl = null;
            this.heightAutoObserverEl = null;
            this.rtlHelpers = null;
            this.scrollbarWidth = 0;
            this.resizeObserver = null;
            this.mutationObserver = null;
            this.elStyles = null;
            this.isRtl = null;
            this.mouseX = 0;
            this.mouseY = 0;
            this.onMouseMove = function () { };
            this.onWindowResize = function () { };
            this.onStopScrolling = function () { };
            this.onMouseEntered = function () { };
            /**
             * On scroll event handling
             */
            this.onScroll = function () {
                var elWindow = getElementWindow(_this.el);
                if (!_this.scrollXTicking) {
                    elWindow.requestAnimationFrame(_this.scrollX);
                    _this.scrollXTicking = true;
                }
                if (!_this.scrollYTicking) {
                    elWindow.requestAnimationFrame(_this.scrollY);
                    _this.scrollYTicking = true;
                }
                if (!_this.isScrolling) {
                    _this.isScrolling = true;
                    addClasses$2(_this.el, _this.classNames.scrolling);
                }
                _this.showScrollbar("x");
                _this.showScrollbar("y");
                _this.onStopScrolling();
            };
            this.scrollX = function () {
                if (_this.axis.x.isOverflowing) {
                    _this.positionScrollbar("x");
                }
                _this.scrollXTicking = false;
            };
            this.scrollY = function () {
                if (_this.axis.y.isOverflowing) {
                    _this.positionScrollbar("y");
                }
                _this.scrollYTicking = false;
            };
            this._onStopScrolling = function () {
                removeClasses(_this.el, _this.classNames.scrolling);
                if (_this.options.autoHide) {
                    _this.hideScrollbar("x");
                    _this.hideScrollbar("y");
                }
                _this.isScrolling = false;
            };
            this.onMouseEnter = function () {
                if (!_this.isMouseEntering) {
                    addClasses$2(_this.el, _this.classNames.mouseEntered);
                    _this.showScrollbar("x");
                    _this.showScrollbar("y");
                    _this.isMouseEntering = true;
                }
                _this.onMouseEntered();
            };
            this._onMouseEntered = function () {
                removeClasses(_this.el, _this.classNames.mouseEntered);
                if (_this.options.autoHide) {
                    _this.hideScrollbar("x");
                    _this.hideScrollbar("y");
                }
                _this.isMouseEntering = false;
            };
            this._onMouseMove = function (e) {
                _this.mouseX = e.clientX;
                _this.mouseY = e.clientY;
                if (_this.axis.x.isOverflowing || _this.axis.x.forceVisible) {
                    _this.onMouseMoveForAxis("x");
                }
                if (_this.axis.y.isOverflowing || _this.axis.y.forceVisible) {
                    _this.onMouseMoveForAxis("y");
                }
            };
            this.onMouseLeave = function () {
                _this.onMouseMove.cancel();
                if (_this.axis.x.isOverflowing || _this.axis.x.forceVisible) {
                    _this.onMouseLeaveForAxis("x");
                }
                if (_this.axis.y.isOverflowing || _this.axis.y.forceVisible) {
                    _this.onMouseLeaveForAxis("y");
                }
                _this.mouseX = -1;
                _this.mouseY = -1;
            };
            this._onWindowResize = function () {
                // Recalculate scrollbarWidth in case it's a zoom
                _this.scrollbarWidth = _this.getScrollbarWidth();
                _this.hideNativeScrollbar();
            };
            this.onPointerEvent = function (e) {
                if (!_this.axis.x.track.el ||
                    !_this.axis.y.track.el ||
                    !_this.axis.x.scrollbar.el ||
                    !_this.axis.y.scrollbar.el)
                    return;
                var isWithinTrackXBounds, isWithinTrackYBounds;
                _this.axis.x.track.rect = _this.axis.x.track.el.getBoundingClientRect();
                _this.axis.y.track.rect = _this.axis.y.track.el.getBoundingClientRect();
                if (_this.axis.x.isOverflowing || _this.axis.x.forceVisible) {
                    isWithinTrackXBounds = _this.isWithinBounds(_this.axis.x.track.rect);
                }
                if (_this.axis.y.isOverflowing || _this.axis.y.forceVisible) {
                    isWithinTrackYBounds = _this.isWithinBounds(_this.axis.y.track.rect);
                }
                // If any pointer event is called on the scrollbar
                if (isWithinTrackXBounds || isWithinTrackYBounds) {
                    // Prevent event leaking
                    e.stopPropagation();
                    if (e.type === "pointerdown" && e.pointerType !== "touch") {
                        if (isWithinTrackXBounds) {
                            _this.axis.x.scrollbar.rect =
                                _this.axis.x.scrollbar.el.getBoundingClientRect();
                            if (_this.isWithinBounds(_this.axis.x.scrollbar.rect)) {
                                _this.onDragStart(e, "x");
                            }
                            else {
                                _this.onTrackClick(e, "x");
                            }
                        }
                        if (isWithinTrackYBounds) {
                            _this.axis.y.scrollbar.rect =
                                _this.axis.y.scrollbar.el.getBoundingClientRect();
                            if (_this.isWithinBounds(_this.axis.y.scrollbar.rect)) {
                                _this.onDragStart(e, "y");
                            }
                            else {
                                _this.onTrackClick(e, "y");
                            }
                        }
                    }
                }
            };
            /**
             * Drag scrollbar handle
             */
            this.drag = function (e) {
                var _a, _b, _c, _d, _e, _f, _g, _h, _j, _k, _l;
                if (!_this.draggedAxis || !_this.contentWrapperEl)
                    return;
                var eventOffset;
                var track = _this.axis[_this.draggedAxis].track;
                var trackSize = (_b = (_a = track.rect) === null || _a === void 0 ? void 0 : _a[_this.axis[_this.draggedAxis].sizeAttr]) !== null && _b !== void 0 ? _b : 0;
                var scrollbar = _this.axis[_this.draggedAxis].scrollbar;
                var contentSize = (_d = (_c = _this.contentWrapperEl) === null || _c === void 0 ? void 0 : _c[_this.axis[_this.draggedAxis].scrollSizeAttr]) !== null && _d !== void 0 ? _d : 0;
                var hostSize = parseInt((_f = (_e = _this.elStyles) === null || _e === void 0 ? void 0 : _e[_this.axis[_this.draggedAxis].sizeAttr]) !== null && _f !== void 0 ? _f : "0px", 10);
                e.preventDefault();
                e.stopPropagation();
                if (_this.draggedAxis === "y") {
                    eventOffset = e.pageY;
                }
                else {
                    eventOffset = e.pageX;
                }
                // Calculate how far the user's mouse is from the top/left of the scrollbar (minus the dragOffset).
                var dragPos = eventOffset -
                    ((_h = (_g = track.rect) === null || _g === void 0 ? void 0 : _g[_this.axis[_this.draggedAxis].offsetAttr]) !== null && _h !== void 0 ? _h : 0) -
                    _this.axis[_this.draggedAxis].dragOffset;
                dragPos = _this.draggedAxis === "x" && _this.isRtl
                    ? ((_k = (_j = track.rect) === null || _j === void 0 ? void 0 : _j[_this.axis[_this.draggedAxis].sizeAttr]) !== null && _k !== void 0 ? _k : 0) -
                        scrollbar.size -
                        dragPos
                    : dragPos;
                // Convert the mouse position into a percentage of the scrollbar height/width.
                var dragPerc = dragPos / (trackSize - scrollbar.size);
                // Scroll the content by the same percentage.
                var scrollPos = dragPerc * (contentSize - hostSize);
                // Fix browsers inconsistency on RTL
                if (_this.draggedAxis === "x" && _this.isRtl) {
                    scrollPos = ((_l = SimpleBarCore.getRtlHelpers()) === null || _l === void 0 ? void 0 : _l.isScrollingToNegative)
                        ? -scrollPos
                        : scrollPos;
                }
                _this.contentWrapperEl[_this.axis[_this.draggedAxis].scrollOffsetAttr] =
                    scrollPos;
            };
            /**
             * End scroll handle drag
             */
            this.onEndDrag = function (e) {
                var elDocument = getElementDocument(_this.el);
                var elWindow = getElementWindow(_this.el);
                e.preventDefault();
                e.stopPropagation();
                removeClasses(_this.el, _this.classNames.dragging);
                elDocument.removeEventListener("mousemove", _this.drag, true);
                elDocument.removeEventListener("mouseup", _this.onEndDrag, true);
                _this.removePreventClickId = elWindow.setTimeout(function () {
                    // Remove these asynchronously so we still suppress click events
                    // generated simultaneously with mouseup.
                    elDocument.removeEventListener("click", _this.preventClick, true);
                    elDocument.removeEventListener("dblclick", _this.preventClick, true);
                    _this.removePreventClickId = null;
                });
            };
            /**
             * Handler to ignore click events during drag
             */
            this.preventClick = function (e) {
                e.preventDefault();
                e.stopPropagation();
            };
            this.el = element;
            this.options = __assign(__assign({}, SimpleBarCore.defaultOptions), options);
            this.classNames = __assign(__assign({}, SimpleBarCore.defaultOptions.classNames), options.classNames);
            this.axis = {
                x: {
                    scrollOffsetAttr: "scrollLeft",
                    sizeAttr: "width",
                    scrollSizeAttr: "scrollWidth",
                    offsetSizeAttr: "offsetWidth",
                    offsetAttr: "left",
                    overflowAttr: "overflowX",
                    dragOffset: 0,
                    isOverflowing: true,
                    forceVisible: false,
                    track: { size: null, el: null, rect: null, isVisible: false },
                    scrollbar: { size: null, el: null, rect: null, isVisible: false }
                },
                y: {
                    scrollOffsetAttr: "scrollTop",
                    sizeAttr: "height",
                    scrollSizeAttr: "scrollHeight",
                    offsetSizeAttr: "offsetHeight",
                    offsetAttr: "top",
                    overflowAttr: "overflowY",
                    dragOffset: 0,
                    isOverflowing: true,
                    forceVisible: false,
                    track: { size: null, el: null, rect: null, isVisible: false },
                    scrollbar: { size: null, el: null, rect: null, isVisible: false }
                }
            };
            if (typeof this.el !== "object" || !this.el.nodeName) {
                throw new Error("Argument passed to SimpleBar must be an HTML element instead of ".concat(this.el));
            }
            this.onMouseMove = throttle(this._onMouseMove, 64);
            this.onWindowResize = debounce(this._onWindowResize, 64, { leading: true });
            this.onStopScrolling = debounce(this._onStopScrolling, this.stopScrollDelay);
            this.onMouseEntered = debounce(this._onMouseEntered, this.stopScrollDelay);
            this.init();
        }
        /**
         * Helper to fix browsers inconsistency on RTL:
         *  - Firefox inverts the scrollbar initial position
         *  - IE11 inverts both scrollbar position and scrolling offset
         * Directly inspired by @KingSora's OverlayScrollbars https://github.com/KingSora/OverlayScrollbars/blob/master/js/OverlayScrollbars.js#L1634
         */
        SimpleBarCore.getRtlHelpers = function () {
            if (SimpleBarCore.rtlHelpers) {
                return SimpleBarCore.rtlHelpers;
            }
            var dummyDiv = document.createElement("div");
            dummyDiv.innerHTML =
                "<div class=\"simplebar-dummy-scrollbar-size\"><div></div></div>";
            var scrollbarDummyEl = dummyDiv.firstElementChild;
            var dummyChild = scrollbarDummyEl === null || scrollbarDummyEl === void 0 ? void 0 : scrollbarDummyEl.firstElementChild;
            if (!dummyChild)
                return null;
            document.body.appendChild(scrollbarDummyEl);
            scrollbarDummyEl.scrollLeft = 0;
            var dummyContainerOffset = SimpleBarCore.getOffset(scrollbarDummyEl);
            var dummyChildOffset = SimpleBarCore.getOffset(dummyChild);
            scrollbarDummyEl.scrollLeft = -999;
            var dummyChildOffsetAfterScroll = SimpleBarCore.getOffset(dummyChild);
            document.body.removeChild(scrollbarDummyEl);
            SimpleBarCore.rtlHelpers = {
                // determines if the scrolling is responding with negative values
                isScrollOriginAtZero: dummyContainerOffset.left !== dummyChildOffset.left,
                // determines if the origin scrollbar position is inverted or not (positioned on left or right)
                isScrollingToNegative: dummyChildOffset.left !== dummyChildOffsetAfterScroll.left
            };
            return SimpleBarCore.rtlHelpers;
        };
        SimpleBarCore.prototype.getScrollbarWidth = function () {
            // Try/catch for FF 56 throwing on undefined computedStyles
            try {
                // Detect browsers supporting CSS scrollbar styling and do not calculate
                if ((this.contentWrapperEl &&
                    getComputedStyle(this.contentWrapperEl, "::-webkit-scrollbar")
                        .display === "none") ||
                    "scrollbarWidth" in document.documentElement.style ||
                    "-ms-overflow-style" in document.documentElement.style) {
                    return 0;
                }
                else {
                    return scrollbarWidth();
                }
            }
            catch (e) {
                return scrollbarWidth();
            }
        };
        SimpleBarCore.getOffset = function (el) {
            var rect = el.getBoundingClientRect();
            var elDocument = getElementDocument(el);
            var elWindow = getElementWindow(el);
            return {
                top: rect.top +
                    (elWindow.pageYOffset || elDocument.documentElement.scrollTop),
                left: rect.left +
                    (elWindow.pageXOffset || elDocument.documentElement.scrollLeft)
            };
        };
        SimpleBarCore.prototype.init = function () {
            // We stop here on server-side
            if (canUseDom) {
                this.initDOM();
                this.rtlHelpers = SimpleBarCore.getRtlHelpers();
                this.scrollbarWidth = this.getScrollbarWidth();
                this.recalculate();
                this.initListeners();
            }
        };
        SimpleBarCore.prototype.initDOM = function () {
            var _a, _b;
            // assume that element has his DOM already initiated
            this.wrapperEl = this.el.querySelector(classNamesToQuery(this.classNames.wrapper));
            this.contentWrapperEl =
                this.options.scrollableNode ||
                    this.el.querySelector(classNamesToQuery(this.classNames.contentWrapper));
            this.contentEl =
                this.options.contentNode ||
                    this.el.querySelector(classNamesToQuery(this.classNames.contentEl));
            this.offsetEl = this.el.querySelector(classNamesToQuery(this.classNames.offset));
            this.maskEl = this.el.querySelector(classNamesToQuery(this.classNames.mask));
            this.placeholderEl = this.findChild(this.wrapperEl, classNamesToQuery(this.classNames.placeholder));
            this.heightAutoObserverWrapperEl = this.el.querySelector(classNamesToQuery(this.classNames.heightAutoObserverWrapperEl));
            this.heightAutoObserverEl = this.el.querySelector(classNamesToQuery(this.classNames.heightAutoObserverEl));
            this.axis.x.track.el = this.findChild(this.el, "".concat(classNamesToQuery(this.classNames.track)).concat(classNamesToQuery(this.classNames.horizontal)));
            this.axis.y.track.el = this.findChild(this.el, "".concat(classNamesToQuery(this.classNames.track)).concat(classNamesToQuery(this.classNames.vertical)));
            this.axis.x.scrollbar.el =
                ((_a = this.axis.x.track.el) === null || _a === void 0 ? void 0 : _a.querySelector(classNamesToQuery(this.classNames.scrollbar))) || null;
            this.axis.y.scrollbar.el =
                ((_b = this.axis.y.track.el) === null || _b === void 0 ? void 0 : _b.querySelector(classNamesToQuery(this.classNames.scrollbar))) || null;
            if (!this.options.autoHide) {
                addClasses$2(this.axis.x.scrollbar.el, this.classNames.visible);
                addClasses$2(this.axis.y.scrollbar.el, this.classNames.visible);
            }
        };
        SimpleBarCore.prototype.initListeners = function () {
            var _this = this;
            var _a;
            var elWindow = getElementWindow(this.el);
            // Event listeners
            this.el.addEventListener("mouseenter", this.onMouseEnter);
            this.el.addEventListener("pointerdown", this.onPointerEvent, true);
            this.el.addEventListener("mousemove", this.onMouseMove);
            this.el.addEventListener("mouseleave", this.onMouseLeave);
            (_a = this.contentWrapperEl) === null || _a === void 0 ? void 0 : _a.addEventListener("scroll", this.onScroll);
            // Browser zoom triggers a window resize
            elWindow.addEventListener("resize", this.onWindowResize);
            if (!this.contentEl)
                return;
            if (window.ResizeObserver) {
                // Hack for https://github.com/WICG/ResizeObserver/issues/38
                var resizeObserverStarted_1 = false;
                var resizeObserver = elWindow.ResizeObserver || ResizeObserver;
                this.resizeObserver = new resizeObserver(function () {
                    if (!resizeObserverStarted_1)
                        return;
                    elWindow.requestAnimationFrame(function () {
                        _this.recalculate();
                    });
                });
                this.resizeObserver.observe(this.el);
                this.resizeObserver.observe(this.contentEl);
                if (this.contentEl.firstChild !== undefined) {
                    this.resizeObserver.observe(this.contentEl.firstChild);
                }
                elWindow.requestAnimationFrame(function () {
                    resizeObserverStarted_1 = true;
                });
            }
            // This is required to detect horizontal scroll. Vertical scroll only needs the resizeObserver.
            this.mutationObserver = new elWindow.MutationObserver(function () {
                elWindow.requestAnimationFrame(function () {
                    _this.recalculate();
                });
            });
            this.mutationObserver.observe(this.contentEl, {
                childList: true,
                subtree: true,
                characterData: true
            });
        };
        SimpleBarCore.prototype.recalculate = function () {
            if (!this.heightAutoObserverEl ||
                !this.contentEl ||
                !this.contentWrapperEl ||
                !this.wrapperEl ||
                !this.placeholderEl)
                return;
            var elWindow = getElementWindow(this.el);
            this.elStyles = elWindow.getComputedStyle(this.el);
            this.isRtl = this.elStyles.direction === "rtl";
            var contentElOffsetWidth = this.contentEl.offsetWidth;
            var isHeightAuto = this.heightAutoObserverEl.offsetHeight <= 1;
            var isWidthAuto = this.heightAutoObserverEl.offsetWidth <= 1 || contentElOffsetWidth > 0;
            var contentWrapperElOffsetWidth = this.contentWrapperEl.offsetWidth;
            var elOverflowX = this.elStyles.overflowX;
            var elOverflowY = this.elStyles.overflowY;
            this.contentEl.style.padding = "".concat(this.elStyles.paddingTop, " ").concat(this.elStyles.paddingRight, " ").concat(this.elStyles.paddingBottom, " ").concat(this.elStyles.paddingLeft);
            this.wrapperEl.style.margin = "-".concat(this.elStyles.paddingTop, " -").concat(this.elStyles.paddingRight, " -").concat(this.elStyles.paddingBottom, " -").concat(this.elStyles.paddingLeft);
            var contentElScrollHeight = this.contentEl.scrollHeight;
            var contentElScrollWidth = this.contentEl.scrollWidth;
            this.contentWrapperEl.style.height = isHeightAuto ? "auto" : "100%";
            // Determine placeholder size
            this.placeholderEl.style.width = isWidthAuto
                ? "100%"//"".concat(contentElOffsetWidth || contentElScrollWidth, "px")
                : "auto";
            this.placeholderEl.style.height = "".concat(contentElScrollHeight, "px");
            var contentWrapperElOffsetHeight = this.contentWrapperEl.offsetHeight;
            this.axis.x.isOverflowing =
                contentElOffsetWidth !== 0 && contentElScrollWidth > contentElOffsetWidth;
            this.axis.y.isOverflowing =
                contentElScrollHeight > contentWrapperElOffsetHeight;
            // Set isOverflowing to false if user explicitely set hidden overflow
            this.axis.x.isOverflowing =
                elOverflowX === "hidden" ? false : this.axis.x.isOverflowing;
            this.axis.y.isOverflowing =
                elOverflowY === "hidden" ? false : this.axis.y.isOverflowing;
            this.axis.x.forceVisible =
                this.options.forceVisible === "x" || this.options.forceVisible === true;
            this.axis.y.forceVisible =
                this.options.forceVisible === "y" || this.options.forceVisible === true;
            this.hideNativeScrollbar();
            // Set isOverflowing to false if scrollbar is not necessary (content is shorter than offset)
            var offsetForXScrollbar = this.axis.x.isOverflowing
                ? this.scrollbarWidth
                : 0;
            var offsetForYScrollbar = this.axis.y.isOverflowing
                ? this.scrollbarWidth
                : 0;
            this.axis.x.isOverflowing =
                this.axis.x.isOverflowing &&
                    contentElScrollWidth > contentWrapperElOffsetWidth - offsetForYScrollbar;
            this.axis.y.isOverflowing =
                this.axis.y.isOverflowing &&
                    contentElScrollHeight >
                        contentWrapperElOffsetHeight - offsetForXScrollbar;
            this.axis.x.scrollbar.size = this.getScrollbarSize("x");
            this.axis.y.scrollbar.size = this.getScrollbarSize("y");
            if (this.axis.x.scrollbar.el)
                this.axis.x.scrollbar.el.style.width = "".concat(this.axis.x.scrollbar.size, "px");
            if (this.axis.y.scrollbar.el)
                this.axis.y.scrollbar.el.style.height = "".concat(this.axis.y.scrollbar.size, "px");
            this.positionScrollbar("x");
            this.positionScrollbar("y");
            this.toggleTrackVisibility("x");
            this.toggleTrackVisibility("y");
        };
        /**
         * Calculate scrollbar size
         */
        SimpleBarCore.prototype.getScrollbarSize = function (axis) {
            var _a, _b;
            if (axis === void 0) { axis = "y"; }
            if (!this.axis[axis].isOverflowing || !this.contentEl) {
                return 0;
            }
            var contentSize = this.contentEl[this.axis[axis].scrollSizeAttr];
            var trackSize = (_b = (_a = this.axis[axis].track.el) === null || _a === void 0 ? void 0 : _a[this.axis[axis].offsetSizeAttr]) !== null && _b !== void 0 ? _b : 0;
            var scrollbarRatio = trackSize / contentSize;
            var scrollbarSize;
            // Calculate new height/position of drag handle.
            scrollbarSize = Math.max(~~(scrollbarRatio * trackSize), this.options.scrollbarMinSize);
            if (this.options.scrollbarMaxSize) {
                scrollbarSize = Math.min(scrollbarSize, this.options.scrollbarMaxSize);
            }
            return scrollbarSize;
        };
        SimpleBarCore.prototype.positionScrollbar = function (axis) {
            var _a, _b, _c;
            if (axis === void 0) { axis = "y"; }
            var scrollbar = this.axis[axis].scrollbar;
            if (!this.axis[axis].isOverflowing ||
                !this.contentWrapperEl ||
                !scrollbar.el ||
                !this.elStyles) {
                return;
            }
            var contentSize = this.contentWrapperEl[this.axis[axis].scrollSizeAttr];
            var trackSize = ((_a = this.axis[axis].track.el) === null || _a === void 0 ? void 0 : _a[this.axis[axis].offsetSizeAttr]) || 0;
            var hostSize = parseInt(this.elStyles[this.axis[axis].sizeAttr], 10);
            var scrollOffset = this.contentWrapperEl[this.axis[axis].scrollOffsetAttr];
            scrollOffset =
                axis === "x" &&
                    this.isRtl &&
                    ((_b = SimpleBarCore.getRtlHelpers()) === null || _b === void 0 ? void 0 : _b.isScrollOriginAtZero)
                    ? -scrollOffset
                    : scrollOffset;
            if (axis === "x" && this.isRtl) {
                scrollOffset = ((_c = SimpleBarCore.getRtlHelpers()) === null || _c === void 0 ? void 0 : _c.isScrollingToNegative)
                    ? scrollOffset
                    : -scrollOffset;
            }
            var scrollPourcent = scrollOffset / (contentSize - hostSize);
            var handleOffset = ~~((trackSize - scrollbar.size) * scrollPourcent);
            handleOffset =
                axis === "x" && this.isRtl
                    ? -handleOffset + (trackSize - scrollbar.size)
                    : handleOffset;
            scrollbar.el.style.transform =
                axis === "x"
                    ? "translate3d(".concat(handleOffset, "px, 0, 0)")
                    : "translate3d(0, ".concat(handleOffset, "px, 0)");
        };
        SimpleBarCore.prototype.toggleTrackVisibility = function (axis) {
            if (axis === void 0) { axis = "y"; }
            var track = this.axis[axis].track.el;
            var scrollbar = this.axis[axis].scrollbar.el;
            if (!track || !scrollbar || !this.contentWrapperEl)
                return;
            if (this.axis[axis].isOverflowing || this.axis[axis].forceVisible) {
                track.style.visibility = "visible";
                this.contentWrapperEl.style[this.axis[axis].overflowAttr] = "scroll";
                this.el.classList.add("".concat(this.classNames.scrollable, "-").concat(axis));
            }
            else {
                track.style.visibility = "hidden";
                this.contentWrapperEl.style[this.axis[axis].overflowAttr] = "hidden";
                this.el.classList.remove("".concat(this.classNames.scrollable, "-").concat(axis));
            }
            // Even if forceVisible is enabled, scrollbar itself should be hidden
            if (this.axis[axis].isOverflowing) {
                scrollbar.style.display = "block";
            }
            else {
                scrollbar.style.display = "none";
            }
        };
        SimpleBarCore.prototype.showScrollbar = function (axis) {
            if (axis === void 0) { axis = "y"; }
            if (this.axis[axis].isOverflowing && !this.axis[axis].scrollbar.isVisible) {
                addClasses$2(this.axis[axis].scrollbar.el, this.classNames.visible);
                this.axis[axis].scrollbar.isVisible = true;
            }
        };
        SimpleBarCore.prototype.hideScrollbar = function (axis) {
            if (axis === void 0) { axis = "y"; }
            if (this.axis[axis].isOverflowing && this.axis[axis].scrollbar.isVisible) {
                removeClasses(this.axis[axis].scrollbar.el, this.classNames.visible);
                this.axis[axis].scrollbar.isVisible = false;
            }
        };
        SimpleBarCore.prototype.hideNativeScrollbar = function () {
            if (!this.offsetEl)
                return;
            this.offsetEl.style[this.isRtl ? "left" : "right"] =
                this.axis.y.isOverflowing || this.axis.y.forceVisible
                    ? "-".concat(this.scrollbarWidth, "px")
                    : "0px";
            this.offsetEl.style.bottom =
                this.axis.x.isOverflowing || this.axis.x.forceVisible
                    ? "-".concat(this.scrollbarWidth, "px")
                    : "0px";
        };
        SimpleBarCore.prototype.onMouseMoveForAxis = function (axis) {
            if (axis === void 0) { axis = "y"; }
            var currentAxis = this.axis[axis];
            if (!currentAxis.track.el || !currentAxis.scrollbar.el)
                return;
            currentAxis.track.rect = currentAxis.track.el.getBoundingClientRect();
            currentAxis.scrollbar.rect =
                currentAxis.scrollbar.el.getBoundingClientRect();

            this.showScrollbar(axis);

            if (this.isWithinBounds(currentAxis.track.rect)) {
                this.showScrollbar(axis);
                addClasses$2(currentAxis.track.el, this.classNames.hover);
                if (this.isWithinBounds(currentAxis.scrollbar.rect)) {
                    addClasses$2(currentAxis.scrollbar.el, this.classNames.hover);
                }
                else {
                    removeClasses(currentAxis.scrollbar.el, this.classNames.hover);
                }
            }
            else {
                removeClasses(currentAxis.track.el, this.classNames.hover);
                if (this.options.autoHide) {
                    //this.hideScrollbar(axis);
                }
            }
        };
        SimpleBarCore.prototype.onMouseLeaveForAxis = function (axis) {
            if (axis === void 0) { axis = "y"; }
            removeClasses(this.axis[axis].track.el, this.classNames.hover);
            removeClasses(this.axis[axis].scrollbar.el, this.classNames.hover);
            if (this.options.autoHide) {
                this.hideScrollbar(axis);
            }
        };
        /**
         * on scrollbar handle drag movement starts
         */
        SimpleBarCore.prototype.onDragStart = function (e, axis) {
            var _a;
            if (axis === void 0) { axis = "y"; }
            var elDocument = getElementDocument(this.el);
            var elWindow = getElementWindow(this.el);
            var scrollbar = this.axis[axis].scrollbar;
            // Measure how far the user's mouse is from the top of the scrollbar drag handle.
            var eventOffset = axis === "y" ? e.pageY : e.pageX;
            this.axis[axis].dragOffset =
                eventOffset - (((_a = scrollbar.rect) === null || _a === void 0 ? void 0 : _a[this.axis[axis].offsetAttr]) || 0);
            this.draggedAxis = axis;
            addClasses$2(this.el, this.classNames.dragging);
            elDocument.addEventListener("mousemove", this.drag, true);
            elDocument.addEventListener("mouseup", this.onEndDrag, true);
            if (this.removePreventClickId === null) {
                elDocument.addEventListener("click", this.preventClick, true);
                elDocument.addEventListener("dblclick", this.preventClick, true);
            }
            else {
                elWindow.clearTimeout(this.removePreventClickId);
                this.removePreventClickId = null;
            }
        };
        SimpleBarCore.prototype.onTrackClick = function (e, axis) {
            var _this = this;
            var _a, _b, _c, _d;
            if (axis === void 0) { axis = "y"; }
            var currentAxis = this.axis[axis];
            if (!this.options.clickOnTrack ||
                !currentAxis.scrollbar.el ||
                !this.contentWrapperEl)
                return;
            // Preventing the event's default to trigger click underneath
            e.preventDefault();
            var elWindow = getElementWindow(this.el);
            this.axis[axis].scrollbar.rect =
                currentAxis.scrollbar.el.getBoundingClientRect();
            var scrollbar = this.axis[axis].scrollbar;
            var scrollbarOffset = (_b = (_a = scrollbar.rect) === null || _a === void 0 ? void 0 : _a[this.axis[axis].offsetAttr]) !== null && _b !== void 0 ? _b : 0;
            var hostSize = parseInt((_d = (_c = this.elStyles) === null || _c === void 0 ? void 0 : _c[this.axis[axis].sizeAttr]) !== null && _d !== void 0 ? _d : "0px", 10);
            var scrolled = this.contentWrapperEl[this.axis[axis].scrollOffsetAttr];
            var t = axis === "y"
                ? this.mouseY - scrollbarOffset
                : this.mouseX - scrollbarOffset;
            var dir = t < 0 ? -1 : 1;
            var scrollSize = dir === -1 ? scrolled - hostSize : scrolled + hostSize;
            var speed = 40;
            var scrollTo = function () {
                if (!_this.contentWrapperEl)
                    return;
                if (dir === -1) {
                    if (scrolled > scrollSize) {
                        scrolled -= speed;
                        _this.contentWrapperEl[_this.axis[axis].scrollOffsetAttr] = scrolled;
                        elWindow.requestAnimationFrame(scrollTo);
                    }
                }
                else {
                    if (scrolled < scrollSize) {
                        scrolled += speed;
                        _this.contentWrapperEl[_this.axis[axis].scrollOffsetAttr] = scrolled;
                        elWindow.requestAnimationFrame(scrollTo);
                    }
                }
            };
            scrollTo();
        };
        /**
         * Getter for content element
         */
        SimpleBarCore.prototype.getContentElement = function () {
            return this.contentEl;
        };
        /**
         * Getter for original scrolling element
         */
        SimpleBarCore.prototype.getScrollElement = function () {
            return this.contentWrapperEl;
        };
        SimpleBarCore.prototype.removeListeners = function () {
            var elWindow = getElementWindow(this.el);
            // Event listeners
            this.el.removeEventListener("mouseenter", this.onMouseEnter);
            this.el.removeEventListener("pointerdown", this.onPointerEvent, true);
            this.el.removeEventListener("mousemove", this.onMouseMove);
            this.el.removeEventListener("mouseleave", this.onMouseLeave);
            if (this.contentWrapperEl) {
                this.contentWrapperEl.removeEventListener("scroll", this.onScroll);
            }
            elWindow.removeEventListener("resize", this.onWindowResize);
            if (this.mutationObserver) {
                this.mutationObserver.disconnect();
            }
            if (this.resizeObserver) {
                this.resizeObserver.disconnect();
            }
            // Cancel all debounced functions
            this.onMouseMove.cancel();
            this.onWindowResize.cancel();
            this.onStopScrolling.cancel();
            this.onMouseEntered.cancel();
        };
        /**
         * Remove all listeners from DOM nodes
         */
        SimpleBarCore.prototype.unMount = function () {
            this.removeListeners();
        };
        /**
         * Check if mouse is within bounds
         */
        SimpleBarCore.prototype.isWithinBounds = function (bbox) {
            return (this.mouseX >= bbox.left &&
                this.mouseX <= bbox.left + bbox.width &&
                this.mouseY >= bbox.top &&
                this.mouseY <= bbox.top + bbox.height);
        };
        /**
         * Find element children matches query
         */
        SimpleBarCore.prototype.findChild = function (el, query) {
            var matches = el.matches ||
                el.webkitMatchesSelector ||
                el.mozMatchesSelector ||
                el.msMatchesSelector;
            return Array.prototype.filter.call(el.children, function (child) {
                return matches.call(child, query);
            })[0];
        };
        SimpleBarCore.rtlHelpers = null;
        SimpleBarCore.defaultOptions = {
            forceVisible: false,
            clickOnTrack: true,
            scrollbarMinSize: 25,
            scrollbarMaxSize: 0,
            ariaLabel: "scrollable content",
            classNames: {
                contentEl: "simplebar-content",
                contentWrapper: "simplebar-content-wrapper",
                offset: "simplebar-offset",
                mask: "simplebar-mask",
                wrapper: "simplebar-wrapper",
                placeholder: "simplebar-placeholder",
                scrollbar: "simplebar-scrollbar",
                track: "simplebar-track",
                heightAutoObserverWrapperEl: "simplebar-height-auto-observer-wrapper",
                heightAutoObserverEl: "simplebar-height-auto-observer",
                visible: "simplebar-visible",
                horizontal: "simplebar-horizontal",
                vertical: "simplebar-vertical",
                hover: "simplebar-hover",
                dragging: "simplebar-dragging",
                scrolling: "simplebar-scrolling",
                scrollable: "simplebar-scrollable",
                mouseEntered: "simplebar-mouse-entered"
            },
            scrollableNode: null,
            contentNode: null,
            autoHide: true
        };
        /**
         * Static functions
         */
        SimpleBarCore.getOptions = getOptions$2;
        SimpleBarCore.helpers = helpers;
        return SimpleBarCore;
    }());

    var _a = SimpleBarCore.helpers, getOptions = _a.getOptions, addClasses = _a.addClasses;
    var SimpleBar = /** @class */ (function (_super) {
        __extends(SimpleBar, _super);
        function SimpleBar() {
            var args = [];
            for (var _i = 0; _i < arguments.length; _i++) {
                args[_i] = arguments[_i];
            }
            var _this = _super.apply(this, args) || this;
            // // Save a reference to the instance, so we know this DOM node has already been instancied
            SimpleBar.instances.set(args[0], _this);
            return _this;
        }
        SimpleBar.initDOMLoadedElements = function () {
            document.removeEventListener("DOMContentLoaded", this.initDOMLoadedElements);
            window.removeEventListener("load", this.initDOMLoadedElements);
            Array.prototype.forEach.call(document.querySelectorAll("[data-simplebar]"), function (el) {
                if (el.getAttribute("data-simplebar") !== "init" &&
                    !SimpleBar.instances.has(el))
                    new SimpleBar(el, getOptions(el.attributes));
            });
        };
        SimpleBar.removeObserver = function () {
            var _a;
            (_a = SimpleBar.globalObserver) === null || _a === void 0 ? void 0 : _a.disconnect();
        };
        SimpleBar.prototype.initDOM = function () {
            var _this = this;
            var _a, _b, _c;
            // make sure this element doesn't have the elements yet
            if (!Array.prototype.filter.call(this.el.children, function (child) {
                return child.classList.contains(_this.classNames.wrapper);
            }).length) {
                // Prepare DOM
                this.wrapperEl = document.createElement("div");
                this.contentWrapperEl = document.createElement("div");
                this.offsetEl = document.createElement("div");
                this.maskEl = document.createElement("div");
                this.contentEl = document.createElement("div");
                this.placeholderEl = document.createElement("div");
                this.heightAutoObserverWrapperEl = document.createElement("div");
                this.heightAutoObserverEl = document.createElement("div");
                addClasses(this.wrapperEl, this.classNames.wrapper);
                addClasses(this.contentWrapperEl, this.classNames.contentWrapper);
                addClasses(this.offsetEl, this.classNames.offset);
                addClasses(this.maskEl, this.classNames.mask);
                addClasses(this.contentEl, this.classNames.contentEl);
                addClasses(this.placeholderEl, this.classNames.placeholder);
                addClasses(this.heightAutoObserverWrapperEl, this.classNames.heightAutoObserverWrapperEl);
                addClasses(this.heightAutoObserverEl, this.classNames.heightAutoObserverEl);
                while (this.el.firstChild) {
                    this.contentEl.appendChild(this.el.firstChild);
                }
                this.contentWrapperEl.appendChild(this.contentEl);
                this.offsetEl.appendChild(this.contentWrapperEl);
                this.maskEl.appendChild(this.offsetEl);
                this.heightAutoObserverWrapperEl.appendChild(this.heightAutoObserverEl);
                this.wrapperEl.appendChild(this.heightAutoObserverWrapperEl);
                this.wrapperEl.appendChild(this.maskEl);
                this.wrapperEl.appendChild(this.placeholderEl);
                this.el.appendChild(this.wrapperEl);
                (_a = this.contentWrapperEl) === null || _a === void 0 ? void 0 : _a.setAttribute("tabindex", "0");
                (_b = this.contentWrapperEl) === null || _b === void 0 ? void 0 : _b.setAttribute("role", "region");
                (_c = this.contentWrapperEl) === null || _c === void 0 ? void 0 : _c.setAttribute("aria-label", this.options.ariaLabel);
            }
            if (!this.axis.x.track.el || !this.axis.y.track.el) {
                var track = document.createElement("div");
                var scrollbar = document.createElement("div");
                addClasses(track, this.classNames.track);
                addClasses(scrollbar, this.classNames.scrollbar);
                track.appendChild(scrollbar);
                this.axis.x.track.el = track.cloneNode(true);
                addClasses(this.axis.x.track.el, this.classNames.horizontal);
                this.axis.y.track.el = track.cloneNode(true);
                addClasses(this.axis.y.track.el, this.classNames.vertical);
                this.el.appendChild(this.axis.x.track.el);
                this.el.appendChild(this.axis.y.track.el);
            }
            SimpleBarCore.prototype.initDOM.call(this);
            this.el.setAttribute("data-simplebar", "init");
        };
        SimpleBar.prototype.unMount = function () {
            SimpleBarCore.prototype.unMount.call(this);
            SimpleBar.instances["delete"](this.el);
        };
        SimpleBar.initHtmlApi = function () {
            this.initDOMLoadedElements = this.initDOMLoadedElements.bind(this);
            // MutationObserver is IE11+
            if (typeof MutationObserver !== "undefined") {
                // Mutation observer to observe dynamically added elements
                this.globalObserver = new MutationObserver(SimpleBar.handleMutations);
                this.globalObserver.observe(document, { childList: true, subtree: true });
            }
            // Taken from jQuery `ready` function
            // Instantiate elements already present on the page
            if (document.readyState === "complete" || // @ts-ignore: IE specific
                (document.readyState !== "loading" && !document.documentElement.doScroll)) {
                // Handle it asynchronously to allow scripts the opportunity to delay init
                window.setTimeout(this.initDOMLoadedElements);
            }
            else {
                document.addEventListener("DOMContentLoaded", this.initDOMLoadedElements);
                window.addEventListener("load", this.initDOMLoadedElements);
            }
        };
        SimpleBar.handleMutations = function (mutations) {
            mutations.forEach(function (mutation) {
                mutation.addedNodes.forEach(function (addedNode) {
                    if (addedNode.nodeType === 1) {
                        if (addedNode.hasAttribute("data-simplebar")) {
                            !SimpleBar.instances.has(addedNode) &&
                                document.documentElement.contains(addedNode) &&
                                new SimpleBar(addedNode, getOptions(addedNode.attributes));
                        }
                        else {
                            addedNode
                                .querySelectorAll("[data-simplebar]")
                                .forEach(function (el) {
                                    if (el.getAttribute("data-simplebar") !== "init" &&
                                    !SimpleBar.instances.has(el) &&
                                    document.documentElement.contains(el))
                                        new SimpleBar(el, getOptions(el.attributes));
                                });
                        }
                    }
                });
                mutation.removedNodes.forEach(function (removedNode) {
                    if (removedNode.nodeType === 1) {
                        if (removedNode.getAttribute("data-simplebar") === "init") {
                            SimpleBar.instances.has(removedNode) &&
                                !document.documentElement.contains(removedNode) &&
                                SimpleBar.instances.get(removedNode).unMount();
                        }
                        else {
                            Array.prototype.forEach.call(removedNode.querySelectorAll("[data-simplebar=\"init\"]"), function (el) {
                                SimpleBar.instances.has(el) &&
                                    !document.documentElement.contains(el) &&
                                    SimpleBar.instances.get(el).unMount();
                            });
                        }
                    }
                });
            });
        };
        SimpleBar.instances = new WeakMap();
        return SimpleBar;
    }(SimpleBarCore));
    /**
     * HTML API
     * Called only in a browser env.
     */
    if (canUseDom) {
        SimpleBar.initHtmlApi();
    }

    return SimpleBar;

})();