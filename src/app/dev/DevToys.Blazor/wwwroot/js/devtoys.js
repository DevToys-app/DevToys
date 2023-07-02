window.addEventListener("dragover", function (e) {
    e.preventDefault();

    // Prevent drop anything on the window (except where we will allow it).
    e.dataTransfer.dropEffect = "none";
}, false);

window.addEventListener("drop", function (e) {
    e.preventDefault();
}, false);