function switchTab(tabId, btn) {
    document.querySelectorAll('.tickets-tabs-nav .tab-pill').forEach(el => el.classList.remove('active'));
    document.querySelectorAll('.tab-pane').forEach(el => el.style.display = 'none');

    btn.classList.add('active');
    const target = document.getElementById('tab-' + tabId);
    if (target) target.style.display = 'block';
}
