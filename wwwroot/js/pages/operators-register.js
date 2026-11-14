const fileInput = document.getElementById('licenseFileInput');
    const dropzoneLabel = document.getElementById('dropzoneLabel');
    if (fileInput && dropzoneLabel) {
        fileInput.addEventListener('change', function(e) {
            if (this.files && this.files.length > 0) {
                const file = this.files[0];
                const sizeMb = (file.size / (1024 * 1024)).toFixed(2);
                dropzoneLabel.innerHTML = '✓ Đã chọn: <strong style="color:#5e17eb;">' + file.name + '</strong> (' + sizeMb + ' MB)';
            }
        });
    }
