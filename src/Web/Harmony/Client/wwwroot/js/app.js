function scrollToTop() {
    var element = document.querySelector("#app");

    // smooth scroll to element and align it at the bottom
    element.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function scrollToElement(id) {
    setTimeout(() => {
        var element = document.getElementById(id);

        if (element != "undefined") {
            // smooth scroll to element and align it at the bottom
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    }, 500)
}

function toggleFullScreenModal(modalClass, showFullScreen) {
    var elements = document.getElementsByClassName(modalClass);
    if (elements != null && elements != undefined && elements.length == 1) {
        var element = elements[0];
        if (showFullScreen) {
            element.classList.add("mud-dialog-fullscreen");
        }
        else {
            element.classList.remove("mud-dialog-fullscreen");
        }
    }
}