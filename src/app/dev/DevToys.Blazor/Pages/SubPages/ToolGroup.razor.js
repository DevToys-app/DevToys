export function initializeHeroParallax(scrollViewerId, heroId) {
    const gridView = document.getElementById(scrollViewerId);
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-ignore
    const simpleBar = SimpleBar.instances.get(gridView);
    simpleBar
        .getScrollElement()
        .addEventListener("scroll", (ev) => {
        onGridViewScroll(ev, heroId);
    });
}
function onGridViewScroll(ev, heroId) {
    const gridView = ev.target;
    const hero = document.getElementById(heroId);
    // Apply parallax effect. We have to do it in JS instead of CSS because of the structure of the HTML document.
    // The hero background is not a child of the grid view.
    const translationParallaxFactor = 3;
    const opacityParallaxFactor = 2.5;
    const opacity = 1 - Math.log(gridView.scrollTop / opacityParallaxFactor + 1) / Math.log(hero.clientHeight * opacityParallaxFactor);
    hero.style.opacity = opacity > 0 ? opacity.toString() : "0";
    if (gridView.scrollTop > 0) {
        hero.style.transform = `translateY(-${gridView.scrollTop / translationParallaxFactor}px)`;
    }
    else {
        hero.style.transform = "translateY(0)";
    }
}
//# sourceMappingURL=ToolGroup.razor.js.map