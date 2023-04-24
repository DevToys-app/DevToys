export function initializeStickyHeaders(id) {
    const gridView = document.getElementById(id);
    gridView.addEventListener("scroll", onGridViewScroll);
}
export function initializeDynamicItemSize(id, itemMinWidth) {
    const gridView = document.getElementById(id);
    const gridViewBounds = gridView.getBoundingClientRect();
    fitGridViewItemsToContainer(gridView, gridViewBounds, itemMinWidth);
    // On grid view resize
    const resizeObserver = new ResizeObserver((gridViews) => {
        fitGridViewItemsToContainer(gridViews[0].target, gridViews[0].contentRect, itemMinWidth);
    });
    resizeObserver.observe(gridView);
}
function onGridViewScroll(ev) {
    // This method's goal is to clip the top of each group's item container so
    // it doesn't pass behind the sticky header. Since our sticky header is transparent,
    // this script is necessary as the sticky header doesn't have an opaque color to hide
    // the items passing under it.
    // The sticky header is transparent so we can see the Mica effect through it on Windows.
    const gridView = ev.target;
    const gridViewPosition = gridView.getBoundingClientRect();
    const gridViewPaddingTop = parseFloat(getComputedStyle(gridView).paddingTop);
    let groups = gridView.querySelectorAll(".grid-view-group");
    for (var i = 0; i < groups.length; i++) {
        let group = groups[i];
        let header = group.querySelector(".grid-view-group-header");
        let itemsContainer = group.querySelector(".grid-view-items-container");
        let headerTopPositionInGridView = header.getBoundingClientRect().top - gridViewPosition.top - gridViewPaddingTop;
        if (headerTopPositionInGridView <= 0) {
            let itemsContainerTopPositionInGridView = itemsContainer.getBoundingClientRect().top - gridViewPosition.top - gridViewPaddingTop;
            itemsContainer.style.clipPath = `inset(${header.offsetHeight - itemsContainerTopPositionInGridView}px 0 0 0)`;
        }
        else {
            itemsContainer.style.clipPath = `none`;
        }
    }
}
// Calculate the best size items in the grid view should take in order to fill
// as much space possible while having as many columns at possible.
function fitGridViewItemsToContainer(gridView, gridViewBounds, itemMinWidth) {
    const newGridViewContentSize = gridViewBounds;
    const gridViewItemHorizontalSpace = newGridViewContentSize.width;
    let groups = gridView.querySelectorAll(".grid-view-group");
    if (groups.length > 0) {
        // Calculating the number of columns based on the width of the page
        let firstGroupItemsContainer = groups[0].querySelector(".grid-view-items-container");
        let gapBetweenItems = parseFloat(getComputedStyle(firstGroupItemsContainer).gap);
        let adjustedGridViewItemMaxWidth = itemMinWidth + gapBetweenItems;
        let columns = Math.floor(gridViewItemHorizontalSpace / adjustedGridViewItemMaxWidth);
        // Calculating the new width of the grid view item.
        let newItemWidth = Math.max(itemMinWidth, (gridViewItemHorizontalSpace / Math.max(1, columns)) - gapBetweenItems);
        // Apply the new width to every items.
        let items = gridView.querySelectorAll(".grid-view-item");
        for (var i = 0; i < items.length; i++) {
            let item = items[i];
            item.style.width = `${newItemWidth}px`;
        }
    }
}
//# sourceMappingURL=GridView.razor.js.map