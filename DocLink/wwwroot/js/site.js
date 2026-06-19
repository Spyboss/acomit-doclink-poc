document.addEventListener('DOMContentLoaded', function () {
    var companyName = document.getElementById('companyName');
    var companyFooter = document.getElementById('companyFooter');

    if (companyName) {
        companyName.textContent = 'DocLink';
    }
    if (companyFooter) {
        companyFooter.textContent = 'DocLink \u2022 Document Delivery';
    }
});
