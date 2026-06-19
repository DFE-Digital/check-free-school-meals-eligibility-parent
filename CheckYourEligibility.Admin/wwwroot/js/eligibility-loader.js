function checkStatus() {
    let url = document.getElementById("content").getAttribute("data-url");
    fetch(url)
        .then(response => {
            // If the server redirected to the success page, navigate there
            if (response.url && response.url !== window.location.href) {
                clearInterval(loaderTimer);
                window.location.href = response.url;
                return;
            }
            return response.text().then(html => {
                // Parse the fetched HTML and extract the #content section
                var parser = new DOMParser();
                var doc = parser.parseFromString(html, 'text/html');
                var newContent = doc.getElementById("content");

                // Only update the content if the data-type has changed
                if (newContent.getAttribute("data-type") !== document.getElementById("content").getAttribute("data-type")) {
                    document.getElementById("content").innerHTML = newContent.innerHTML;
                    document.getElementById("content").setAttribute("data-type", newContent.getAttribute("data-type"));
                    clearInterval(loaderTimer);

                    // Re-attach print handler
                    const printLink = document.getElementById("print-link");
                    if (printLink) {
                        printLink.addEventListener("click", (e) => { e.preventDefault(); window.print(); });
                    }
                }
            });
        })
        .catch(error => {
            console.error('Error fetching status:', error);
        });
}

// Poll the server for status if JavaScript is enabled
var loaderTimer = setInterval(function () {
    checkStatus();
}, 5000);