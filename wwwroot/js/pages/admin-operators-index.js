let currentFilter = 'all';

    function setFilter(filter) {
        currentFilter = filter;
        document.querySelectorAll('.tab-btn').forEach(btn => {
            btn.classList.toggle('active', btn.getAttribute('onclick').includes(filter));
        });
        filterRows();
    }

    function handleSearch() {
        filterRows();
    }

    function filterRows() {
        const query = (document.getElementById('searchInput')?.value || '').toLowerCase().trim();
        const rows = document.querySelectorAll('.operator-row');
        let visibleCount = 0;

        rows.forEach(row => {
            const status = row.getAttribute('data-status');
            const keywords = row.getAttribute('data-keywords') || '';
            const matchesFilter = (currentFilter === 'all' || status === currentFilter);
            const matchesSearch = !query || keywords.includes(query);

            if (matchesFilter && matchesSearch) {
                row.style.display = '';
                visibleCount++;
            } else {
                row.style.display = 'none';
            }
        });

        const label = document.getElementById('recordCountLabel');
        if (label) {
            label.textContent = '(' + visibleCount + ' hồ sơ)';
        }
    }

    function openRejectModal(id, name) {
        const modal = document.getElementById('rejectModal');
        const form = document.getElementById('rejectForm');
        const nameEl = document.getElementById('rejectCompanyName');
        const noteInput = document.getElementById('rejectNoteInput');

        if (modal && form && nameEl) {
            form.action = '/admin/operators/' + id + '/reject';
            nameEl.textContent = name;
            if (noteInput) noteInput.value = '';
            modal.style.display = 'grid';
        }
    }

    function closeRejectModal() {
        const modal = document.getElementById('rejectModal');
        if (modal) modal.style.display = 'none';
    }
