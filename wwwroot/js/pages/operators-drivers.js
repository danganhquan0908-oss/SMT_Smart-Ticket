function handleDriverSearch() {
        const query = (document.getElementById('driverSearchInput')?.value || '').toLowerCase().trim();
        const rows = document.querySelectorAll('.driver-row');
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
        const label = document.getElementById('driverCountLabel');
        if (label) label.textContent = '(' + count + ' tài xế)';
    }

    function openEditDriverModal(id, name, phone, license) {
        const modal = document.getElementById('editDriverModal');
        const form = document.getElementById('editDriverForm');
        const nameInput = document.getElementById('editFullNameInput');
        const phoneInput = document.getElementById('editPhoneInput');
        const licenseInput = document.getElementById('editLicenseInput');

        if (modal && form && nameInput && phoneInput && licenseInput) {
            form.action = '/operator/drivers/' + id + '/edit';
            nameInput.value = name;
            phoneInput.value = phone;
            licenseInput.value = license;
            modal.style.display = 'grid';
        }
    }

    function closeEditDriverModal() {
        const modal = document.getElementById('editDriverModal');
        if (modal) modal.style.display = 'none';
    }
