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
            element.style.overflowY = "auto";
        }
        else {
            element.classList.remove("mud-dialog-fullscreen");
        }
    }
}

function toggleAppSearchWidth() {
    var elements = document.getElementsByClassName("app-search-bar");
    if (elements != null && elements != undefined && elements.length == 1) {
        var searchBar = elements[0];
        var input = searchBar.getElementsByTagName("input")[0];

        input.onfocus = function () {
            searchBar.style.width = "700px";
        }

        input.addEventListener("focusout", function () {
            if (input.value.trim() == '') {
                searchBar.style.width = "250px";
            }
        });
    }
}

function resetAppSearchWidth() {
    var elements = document.getElementsByClassName("app-search-bar");
    if (elements != null && elements != undefined && elements.length == 1) {
        var searchBar = elements[0];
        searchBar.style.width = "250px";
    }
}