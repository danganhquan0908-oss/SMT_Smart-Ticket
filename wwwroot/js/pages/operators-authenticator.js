function copySecretKey() {
        const text = document.getElementById('sharedKeyText')?.textContent?.trim() || '';
        if (!text) return;
        navigator.clipboard.writeText(text).then(() => {
            const btn = document.getElementById('copyBtn');
            if (btn) {
                btn.textContent = '✓ Đã sao chép!';
                btn.style.background = '#16825d';
                btn.style.color = '#fff';
                btn.style.borderColor = '#16825d';
                setTimeout(() => {
                    btn.textContent = '📋 Sao chép';
                    btn.style.background = '#fff';
                    btn.style.color = '#5e17eb';
                    btn.style.borderColor = '#b7a2e6';
                }, 2500);
            }
        });
    }
