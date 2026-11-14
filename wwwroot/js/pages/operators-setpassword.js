function togglePasswordVisibility(inputId, btn) {
        const input = document.getElementById(inputId);
        if (!input) return;
        if (input.type === 'password') {
            input.type = 'text';
            btn.textContent = '🔒';
        } else {
            input.type = 'password';
            btn.textContent = '👁';
        }
    }

    const passInput = document.getElementById('passwordInput');
    if (passInput) {
        passInput.addEventListener('input', function() {
            const val = this.value;
            toggleRule('rule-len', val.length >= 12);
            toggleRule('rule-upper', /[A-Z]/.test(val) && /[a-z]/.test(val));
            toggleRule('rule-digit', /[0-9]/.test(val));
            toggleRule('rule-special', /[^A-Za-z0-9]/.test(val));
        });
    }

    function toggleRule(id, isValid) {
        const el = document.getElementById(id);
        if (!el) return;
        if (isValid) {
            el.classList.add('valid');
            el.textContent = '✓ ' + el.textContent.substring(2);
        } else {
            el.classList.remove('valid');
            if (el.textContent.startsWith('✓ ')) {
                el.textContent = '○ ' + el.textContent.substring(2);
            }
        }
    }
