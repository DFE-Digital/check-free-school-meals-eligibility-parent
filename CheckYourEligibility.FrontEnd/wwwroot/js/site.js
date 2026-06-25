document.body.className += ' js-enabled' + ('noModule' in HTMLScriptElement.prototype ? ' govuk-frontend-supported' : '');

import { initAll } from './govuk-frontend-6.0.0.min.js'

initAll();

//BEGIN-- Can show elements only when JavaScript is enabled by using this class on the element
document.querySelectorAll('.js-only').forEach(x => x.classList.add("show"))
//END-- Can show elements only when JavaScript is enabled by using this class on the element

function escapeHtml(unsafe) {
    return unsafe.replace(/[&<>"'`=\/]/g, function (s) {
        return {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;',
            '/': '&#x2F;',
            '`': '&#x60;',
            '=': '&#x3D;'
        }[s];
    });
}

//BEGIN-- Summon print dialogue from a link
document.addEventListener("DOMContentLoaded", function () {
    const printLink = document.getElementById("print-link");
    if (printLink) {
        printLink.addEventListener("click", (e) => { e.preventDefault(); window.print(); });
    }
});

//END-- Summon print dialogue from a link

//BEGIN-- Back link in views
document.addEventListener("DOMContentLoaded", function () {
    const backs = document.getElementsByClassName("backLinkJS");
    for (var i = 0; i < backs.length; i++) {
        backs[i].onclick = function () { history.back(); return false; };
    }
});
//END-- Back link in views