function checkBulkStatus() {
    const content = document.getElementById("content");

    if (!content) {
        return;
    }

    const url = content.getAttribute("data-url");

    fetch(url)
        .then(response => {
            // If the server redirected to the success page, navigate there.
            if (response.url && response.url !== window.location.href) {
                clearInterval(loaderTimer);
                window.location.href = response.url;
                return null;
            }

            return response.text();
        })
        .then(html => {
            if (!html) {
                return;
            }

            const parser = new DOMParser();
            const doc = parser.parseFromString(html, "text/html");
            const newContent = doc.getElementById("content");

            if (!newContent) {
                return;
            }

            // Keep polling after updating so progress continues to refresh.
            if (newContent.getAttribute("data-type") !== content.getAttribute("data-type")) {
                content.innerHTML = newContent.innerHTML;
                content.setAttribute("data-type", newContent.getAttribute("data-type"));
            }
        })
        .catch(error => {
            console.error("Error fetching status:", error);
        });
}

const loaderTimer = setInterval(function () {
    checkBulkStatus();
}, 5000);