(function () {
    var hrefs = [];
    var summary = document.getElementById("error-summary");
    if (summary) {
        var links = summary.querySelectorAll("a");
    }
    function linkAndStyleErrors() {
        var href, element, parent;
        for (let i = 0; i < links?.length || 0; i++) {
            href = links[i].getAttribute("href");
            hrefs[i] = href;
            try {
                element = document.querySelector(href);
            } catch {
                if (!element) {
                    var hrefwithoutHash = href.replace("#", "");
                    element = document.getElementById(hrefwithoutHash);
                }
            }

            if (element) {
                let dobElement = element.closest('.govuk-form-group[data-type="dob-input"]');
                parent = dobElement ? dobElement.closest('.govuk-form-group[data-type="dob-form-group"]') : element.closest('.govuk-form-group');

                if (!parent) {
                    parent = element.parentElement;
                }

                if (parent) {
                    parent.classList.add("govuk-form-group--error");
                    setErrorStyle(parent);
                }
            }
        }
    }
    function setErrorStyle(parent) {
        let input = parent.querySelector(".govuk-form-input");
        if (input !== null) {
            input.classList.add("govuk-form-input--error");
        }
    }
    function setFocusOnSummary() {
        let summary = document.querySelector('.govuk-error-summary');
        let successSummary = document.querySelector('.govuk-success-message');
        if (successSummary) {
            successSummary.focus();
        }
        if (summary) {
            window.onload = function () {
                summary.focus();
            }
        }
    }
    if (links) {
        linkAndStyleErrors();
    }
    setFocusOnSummary();
})();