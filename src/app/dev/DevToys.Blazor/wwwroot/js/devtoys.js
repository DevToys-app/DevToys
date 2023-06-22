/* eslint-disable no-undef */
/* eslint-disable @typescript-eslint/no-var-requires */
/* eslint-disable-next-line @typescript-eslint/no-var-requires */
require.config({ paths: { "split-grid": "_content/DevToys.Blazor/lib/split-grid" } });
require(["split-grid/split-grid"], function (split) {
    window.Split = split;
});