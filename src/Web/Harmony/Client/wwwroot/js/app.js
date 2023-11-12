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