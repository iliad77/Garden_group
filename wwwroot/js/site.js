// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Live in-memory table filter for already loaded ticket list
function filterTicketTable(keywordInputId, tableBodyId) {
    const input = document.getElementById(keywordInputId);
    const filter = input.value.toLowerCase();
    const rows = document.querySelectorAll(`#${tableBodyId} tr`);

    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        row.style.display = text.includes(filter) ? "" : "none";
    });
}
