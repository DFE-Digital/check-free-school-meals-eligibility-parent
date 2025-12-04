//BEGIN-- Back link in views
document.addEventListener("DOMContentLoaded", function () {
    const backs = document.getElementsByClassName("backLinkJS");
    for (var i = 0; i < backs.length; i++) {
        backs[i].onclick = function () { history.back(); return false; };
    }
});
//END-- Back link in views