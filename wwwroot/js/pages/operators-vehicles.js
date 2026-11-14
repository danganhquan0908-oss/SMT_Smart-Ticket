function updatePlatePreview(val) {
        const text = document.getElementById('platePreviewText');
        if (text) {
            text.textContent = val.trim().toUpperCase() || '51B-123.45';
        }
    }

    function handleVehicleSearch() {
        const query = (document.getElementById('vehicleSearchInput')?.value || '').toLowerCase().trim();
        const rows = document.querySelectorAll('.vehicle-row');
        let count = 0;
        rows.forEach(row => {
            const kw = row.getAttribute('data-keywords') || '';
            if (!query || kw.includes(query)) {
                row.style.display = '';
                count++;
            } else {
                row.style.display = 'none';
            }
        });
        const label = document.getElementById('vehicleCountLabel');
        if (label) label.textContent = '(' + count + ' xe)';
    }

    function openEditVehicleModal(id, plate, type, templateId) {
        const modal = document.getElementById('editVehicleModal');
        const form = document.getElementById('editVehicleForm');
        const plateInput = document.getElementById('editPlateInput');
        const typeInput = document.getElementById('editTypeInput');
        const templateSelect = document.getElementById('editTemplateSelect');

        if (modal && form && plateInput && typeInput && templateSelect) {
            form.action = '/operator/vehicles/' + id + '/edit';
            plateInput.value = plate;
            typeInput.value = type;
            templateSelect.value = templateId;
            modal.style.display = 'grid';
        }
    }

    function closeEditVehicleModal() {
        const modal = document.getElementById('editVehicleModal');
        if (modal) modal.style.display = 'none';
    }
