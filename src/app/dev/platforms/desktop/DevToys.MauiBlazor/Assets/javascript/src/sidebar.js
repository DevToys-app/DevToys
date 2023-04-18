/**
 * --------------------------------------------------------------------------
 * Bootstrap (v5.2.3): collapse.js
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/main/LICENSE)
 * --------------------------------------------------------------------------
 */

import {
    defineJQueryPlugin,
    getElement,
    getElementFromSelector,
    getSelectorFromElement,
    reflow
} from '../util/index'
import EventHandler from '../dom/event-handler'
import SelectorEngine from '../dom/selector-engine'
import BaseComponent from './base-component'

/**
 * Constants
 */

const NAME = 'sidebar-collapse'
const DATA_KEY = 'dt.collapse'
const EVENT_KEY = `.${DATA_KEY}`
const DATA_API_KEY = '.data-api'

const EVENT_SHOW = `show${EVENT_KEY}`
const EVENT_SHOWN = `shown${EVENT_KEY}`
const EVENT_HIDE = `hide${EVENT_KEY}`
const EVENT_HIDDEN = `hidden${EVENT_KEY}`
const EVENT_CLICK_DATA_API = `click${EVENT_KEY}${DATA_API_KEY}`
const EVENT_RESIZE_DATA_API = 'resize'

const CLASS_NAME_SHOW = 'show'
const CLASS_NAME_COLLAPSE = 'sidebar-collapse'
const CLASS_NAME_COLLAPSING = 'sidebar-collapsing'
const CLASS_NAME_COLLAPSED = 'sidebar-collapsed'
const CLASS_NAME_DEEPER_CHILDREN = `:scope .${CLASS_NAME_COLLAPSE} .${CLASS_NAME_COLLAPSE}`

const WIDTH = 'width'
const HEIGHT = 'height'

const SELECTOR_ACTIVES = '.collapse.show, .collapse.collapsing'
const SELECTOR_DATA_TOGGLE = '[data-toggle="sidebar"]'

const Default = {
    parent: null,
    toggle: true
}

const DefaultType = {
    parent: '(null|element)',
    toggle: 'boolean'
}

/**
 * Class definition
 */

class Sidebar extends BaseComponent {
    constructor(element, config) {
        super(element, config)

        this._isTransitioning = false
        this._triggerArray = []

        const toggleList = SelectorEngine.find(SELECTOR_DATA_TOGGLE)

        for (const elem of toggleList) {
            const selector = getSelectorFromElement(elem)
            const filterElement = SelectorEngine.find(selector)
                .filter(foundElement => foundElement === this._element)

            if (selector !== null && filterElement.length) {
                this._triggerArray.push(elem)
            }
        }

        this._initializeChildren()

        if (!this._config.parent) {
            this._addAriaAndCollapsedClass(this._triggerArray, this._isShown())
        }

        if (this._config.toggle) {
            this.toggle()
        }
    }

    // Getters
    static get Default() {
        return Default
    }

    static get DefaultType() {
        return DefaultType
    }

    static get NAME() {
        return NAME
    }

    // Public
    toggle() {
        if (this._isShown()) {
            this.hide()
        } else {
            this.show()
        }
    }

    show() {
        if (this._isTransitioning || this._isShown()) {
            return
        }

        let activeChildren = []

        // find active children
        if (this._config.parent) {
            activeChildren = this._getFirstLevelChildren(SELECTOR_ACTIVES)
                .filter(element => element !== this._element)
                .map(element => Collapse.getOrCreateInstance(element, { toggle: false }))
        }

        if (activeChildren.length && activeChildren[0]._isTransitioning) {
            return
        }

        const startEvent = EventHandler.trigger(this._element, EVENT_SHOW)
        if (startEvent.defaultPrevented) {
            return
        }

        for (const activeInstance of activeChildren) {
            activeInstance.hide()
        }

        this._element.classList.remove(CLASS_NAME_COLLAPSE)
        this._element.classList.add(CLASS_NAME_COLLAPSING)

        this._element.style[WIDTH] = 0

        this._addAriaAndCollapsedClass(this._triggerArray, true)
        this._isTransitioning = true

        const complete = () => {
            this._isTransitioning = false

            this._element.classList.remove(CLASS_NAME_COLLAPSING)
            this._element.classList.add(CLASS_NAME_COLLAPSE, CLASS_NAME_SHOW)

            this._element.style[WIDTH] = ''

            EventHandler.trigger(this._element, EVENT_SHOWN)
        }


        const capitalizedDimension = WIDTH[0].toUpperCase() + WIDTH.slice(1)
        const scrollSize = `scroll${capitalizedDimension}`

        this._queueCallback(complete, this._element, true)
        this._element.style[WIDTH] = `${this._element[scrollSize]}px`
    }

    hide() {
        if (this._isTransitioning || !this._isShown()) {
            return
        }

        const startEvent = EventHandler.trigger(this._element, EVENT_HIDE)
        if (startEvent.defaultPrevented) {
            return
        }

        this._element.style[WIDTH] = `${this._element.getBoundingClientRect()[WIDTH]}px`

        reflow(this._element)

        this._element.classList.add(CLASS_NAME_COLLAPSING)
        this._element.classList.remove(CLASS_NAME_COLLAPSE, CLASS_NAME_SHOW)

        for (const trigger of this._triggerArray) {
            const element = getElementFromSelector(trigger)

            if (element && !this._isShown(element)) {
                this._addAriaAndCollapsedClass([trigger], false)
            }
        }

        this._isTransitioning = true

        const complete = () => {
            this._isTransitioning = false
            this._element.classList.remove(CLASS_NAME_COLLAPSING)
            this._element.classList.add(CLASS_NAME_COLLAPSE)
            EventHandler.trigger(this._element, EVENT_HIDDEN)
        }

        this._element.style[WIDTH] = ''

        this._queueCallback(complete, this._element, true)
    }

    _isShown(element = this._element) {
        return element.classList.contains(CLASS_NAME_SHOW)
    }

    // Private
    _configAfterMerge(config) {
        config.toggle = Boolean(config.toggle) // Coerce string values
        config.parent = getElement(config.parent)
        return config
    }

    _initializeChildren() {
        if (!this._config.parent) {
            return
        }

        const children = this._getFirstLevelChildren(SELECTOR_DATA_TOGGLE)

        for (const element of children) {
            const selected = getElementFromSelector(element)

            if (selected) {
                this._addAriaAndCollapsedClass([element], this._isShown(selected))
            }
        }
    }

    _getFirstLevelChildren(selector) {
        const children = SelectorEngine.find(CLASS_NAME_DEEPER_CHILDREN, this._config.parent)
        // remove children if greater depth
        return SelectorEngine.find(selector, this._config.parent).filter(element => !children.includes(element))
    }

    _addAriaAndCollapsedClass(triggerArray, isOpen) {
        if (!triggerArray.length) {
            return
        }

        for (const element of triggerArray) {
            element.classList.toggle(CLASS_NAME_COLLAPSED, !isOpen)
            element.setAttribute('aria-expanded', isOpen)
        }
    }

    // Static
    static jQueryInterface(config) {
        const _config = {}
        if (typeof config === 'string' && /show|hide/.test(config)) {
            _config.toggle = false
        }

        return this.each(function () {
            const data = Collapse.getOrCreateInstance(this, _config)

            if (typeof config === 'string') {
                if (typeof data[config] === 'undefined') {
                    throw new TypeError(`No method named "${config}"`)
                }

                data[config]()
            }
        })
    }
}

/**
 * Data API implementation
 */

EventHandler.on(document, EVENT_CLICK_DATA_API, SELECTOR_DATA_TOGGLE, function (event) {
    // preventDefault only for <a> elements (which change the URL) not inside the collapsible element
    if (event.target.tagName === 'A' || (event.delegateTarget && event.delegateTarget.tagName === 'A')) {
        event.preventDefault()
    }

    const selector = getSelectorFromElement(this)
    const selectorElements = SelectorEngine.find(selector)

    for (const element of selectorElements) {
        Sidebar.getOrCreateInstance(element, { toggle: false }).toggle()
    }
})

/**
 * jQuery
 */

defineJQueryPlugin(Sidebar)

export default Sidebar
