function filterStations() {
    const input = document.getElementById("station-filter");
    const filter = input.value.toUpperCase();
    const table = document.getElementById("stations-table");
    const tr = table.getElementsByTagName("tr");

    for (let i = 1; i < tr.length; i++) {
        const text = tr[i].textContent || tr[i].innerText;
        if (text.toUpperCase().indexOf(filter) > -1) {
            tr[i].style.display = "";
        } else {
            tr[i].style.display = "none";
        }
    }
}
