export function initializeStickyHeaders(scrollViewerId: string): void {
    const gridView = document.getElementById(scrollViewerId);
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-ignore
    const simpleBar = SimpleBar.instances.get(gridView);
    simpleBar.getScrollElement().addEventListener("scroll", onGridViewScroll);
}

export function initializeDynamicItemSize(id: string, itemMinWidth: number): void {
    let gridView = document.getElementById(id);
    const gridViewBounds = gridView.getBoundingClientRect();
    fitGridViewItemsToContainer(gridView, gridViewBounds, itemMinWidth);

    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-ignore
    const simpleBar = SimpleBar.instances.get(gridView);
    gridView = simpleBar.getScrollElement();

    // On grid view resize
    const resizeObserver = new ResizeObserver((gridViews) => {
        fitGridViewItemsToContainer(gridViews[0].target as HTMLElement, gridViews[0].contentRect, itemMinWidth);
    });

    resizeObserver.observe(gridView);
}

function onGridViewScroll(ev: Event): void {
    // This method's goal is to clip the top of each group's item container so
    // it doesn't pass behind the sticky header. Since our sticky header is transparent,
    // this script is necessary as the sticky header doesn't have an opaque color to hide
    // the items passing under it.
    // The sticky header is transparent so we can see the Mica effect through it on Windows.

    const gridView = ev.target as HTMLElement;
    const gridViewPosition = gridView.getBoundingClientRect();
    const gridViewPaddingTop = parseFloat(getComputedStyle(gridView).paddingTop);

    const groups = gridView.querySelectorAll(".grid-view-group");

    for (let i = 0; i < groups.length; i++) {
        const group = groups[i];
        const header = group.querySelector(".grid-view-group-header") as HTMLElement;
        const itemsContainer = group.querySelector(".grid-view-items-container") as HTMLElement;
        const headerMarginBottom = parseFloat(getComputedStyle(header).marginBottom);
        const headerTopPositionInGridView = header.getBoundingClientRect().top - gridViewPosition.top - gridViewPaddingTop;

        if (headerTopPositionInGridView <= 0) {
            const itemsContainerTopPositionInGridView = itemsContainer.getBoundingClientRect().top - gridViewPosition.top - gridViewPaddingTop;
            itemsContainer.style.clipPath = `inset(${header.offsetHeight + headerMarginBottom - itemsContainerTopPositionInGridView}px 0 0 0)`;
        }
        else {
            itemsContainer.style.clipPath = "none";
        }
    }
}

// Calculate the best size items in the grid view should take in order to fill
// as much space possible while having as many columns at possible.
function fitGridViewItemsToContainer(gridView: HTMLElement, gridViewBounds: DOMRectReadOnly, itemMinWidth: number): void {
    const gridViewContentContainerStyle = getComputedStyle(gridView.getElementsByClassName("grid-view")[0]);
    const newGridViewContentSize = gridViewBounds;
    const gridViewWidth
        = newGridViewContentSize.width
        - parseInt(gridViewContentContainerStyle.paddingLeft)
        - parseInt(gridViewContentContainerStyle.paddingRight);

    const groups = gridView.querySelectorAll(".grid-view-group");

    if (groups.length > 0) {
        // Calculating the number of columns based on the width of the page
        const firstGroupItemsContainer = groups[0].querySelector(".grid-view-items-container") as HTMLElement;
        const gapBetweenItems = parseFloat(getComputedStyle(firstGroupItemsContainer).gap);
        const itemMinWidthWithGap = itemMinWidth + gapBetweenItems;
        const columns = Math.max(1, Math.floor(gridViewWidth / itemMinWidthWithGap));

        // Calculating the new width of the grid view item.
        let newItemWidth: string;
        if (columns == 1) {
            newItemWidth = "100%";
        }
        else {
            const gridViewWidthWithoutGaps = gridViewWidth - (gapBetweenItems * (columns - 1));
            newItemWidth = `${Math.max(itemMinWidth, (gridViewWidthWithoutGaps / columns))}px`;
        }

        // Apply the new width to every items.
        const items = gridView.querySelectorAll(".grid-view-item");
        for (let i = 0; i < items.length; i++) {
            const item = items[i] as HTMLElement;
            item.style.width = newItemWidth;
        }
    }
}