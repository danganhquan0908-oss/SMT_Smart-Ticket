(() => {
    'use strict';
    const page = document.getElementById('account-page');
    if (!page) return;
    const panels = [...page.querySelectorAll('[data-account-panel]')];
    const links = [...page.querySelectorAll('[data-account-section]')];
    const localize = value => window.SMT?.text(value) || value;
    const validSection = value => panels.some(panel => panel.dataset.accountPanel === value);
    function sectionFromLocation() {
        const hashPanel = panels.find(panel => `#${panel.id}` === location.hash);
        const requested = new URL(location.href).searchParams.get('section');
        return hashPanel?.dataset.accountPanel || (validSection(requested) ? requested : null);
    }
    function selectSection(section, focus = false) {
        if (!validSection(section)) section = 'profile';
        page.dataset.activeSection = section;
        for (const panel of panels) panel.hidden = panel.dataset.accountPanel !== section;
        for (const link of links) {
            if (link.dataset.accountSection === section) link.setAttribute('aria-current', 'location');
            else link.removeAttribute('aria-current');
        }
        if (focus) {
            const panel = panels.find(panel => !panel.hidden);
            panel?.focus({preventScroll:true});
            if (matchMedia('(max-width:720px)').matches) panel?.scrollIntoView({
                behavior:matchMedia('(prefers-reduced-motion: reduce)').matches ? 'auto' : 'smooth', block:'start'
            });
        }
    }
    const initialSection = page.dataset.hasErrors === 'true'
        ? page.dataset.activeSection : sectionFromLocation() || page.dataset.activeSection;
    selectSection(initialSection);
    for (const link of links) link.addEventListener('click', event => {
        if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
        event.preventDefault();
        selectSection(link.dataset.accountSection, true);
        history.pushState(null, '', link.href);
    });
    window.addEventListener('popstate', () => selectSection(sectionFromLocation() || 'profile'));
    window.addEventListener('hashchange', () => selectSection(sectionFromLocation() || 'profile'));

    const toggles = [...page.querySelectorAll('[data-password-toggle]')];
    function updatePasswordLabels() {
        for (const button of toggles) {
            const field = document.getElementById(button.dataset.passwordToggle);
            button.setAttribute('aria-label', localize(field.type === 'password' ? 'Hiện mật khẩu' : 'Ẩn mật khẩu'));
        }
    }
    for (const button of toggles) button.addEventListener('click', () => {
        const field = document.getElementById(button.dataset.passwordToggle);
        const reveal = field.type === 'password';
        field.type = reveal ? 'text' : 'password';
        button.setAttribute('aria-pressed', String(reveal));
        updatePasswordLabels();
    });

    const passwordForm = document.getElementById('account-password-form');
    const password = document.getElementById('NewPassword');
    const confirmation = document.getElementById('ConfirmPassword');
    const rules = {
        length: value => value.length >= 12 && value.length <= 100,
        case: value => /[A-Z]/.test(value) && /[a-z]/.test(value),
        number: value => /[0-9]/.test(value),
        symbol: value => /[^A-Za-z0-9]/.test(value)
    };
    function updatePasswordRules() {
        const value = password?.value || '';
        page.querySelectorAll('[data-password-rule]').forEach(rule => {
            rule.classList.toggle('is-met', rules[rule.dataset.passwordRule](value));
        });
        if (confirmation) confirmation.setCustomValidity(confirmation.value && confirmation.value !== value
            ? localize('Mật khẩu xác nhận không khớp.') : '');
    }
    password?.addEventListener('input', updatePasswordRules);
    confirmation?.addEventListener('input', updatePasswordRules);
    passwordForm?.addEventListener('submit', event => {
        updatePasswordRules();
        if (!passwordForm.checkValidity()) { event.preventDefault(); passwordForm.reportValidity(); }
    });
    window.addEventListener('pagehide', () => {
        passwordForm?.querySelectorAll('input[type="password"],input[autocomplete="current-password"],input[autocomplete="new-password"]').forEach(field => { field.value = ''; field.type = 'password'; });
        for (const button of toggles) button.setAttribute('aria-pressed', 'false');
    });
    window.addEventListener('pageshow', () => { updatePasswordLabels(); updatePasswordRules(); });

    const preferences = [...page.querySelectorAll('[data-preference-control]')];
    const status = document.getElementById('account-preference-status');
    function syncPreferences() {
        if (!window.SMT) return;
        for (const group of preferences) group.disabled = false;
        page.querySelectorAll('[name="account-language"]').forEach(input => { input.checked = input.value === SMT.currentLang; });
        page.querySelectorAll('[name="account-theme"]').forEach(input => { input.checked = input.value === SMT.currentTheme; });
    }
    page.querySelectorAll('[name="account-language"]').forEach(input => input.addEventListener('change', () => {
        if (!input.checked || !window.SMT) return;
        SMT.setLanguage(input.value);
        status.textContent = localize('Đã lưu tùy chọn trên trình duyệt này.');
    }));
    page.querySelectorAll('[name="account-theme"]').forEach(input => input.addEventListener('change', () => {
        if (!input.checked || !window.SMT) return;
        const theme = input.value;
        if (theme === SMT.currentTheme) return;
        const bounds = input.closest('label').getBoundingClientRect();
        const update = () => {
            SMT.setTheme(theme);
            status.textContent = localize('Đã lưu tùy chọn trên trình duyệt này.');
        };
        SMT.pendingTheme = theme;
        if (window.SMTAppearance) SMTAppearance.transition(update, {clientX:bounds.left + bounds.width / 2,clientY:bounds.top + bounds.height / 2});
        else update();
    }));
    window.addEventListener('smt:languagechange', () => { syncPreferences(); updatePasswordLabels(); updatePasswordRules(); });
    window.addEventListener('smt:themechange', syncPreferences);
    function initialize() { syncPreferences(); updatePasswordLabels(); updatePasswordRules(); }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', initialize, {once:true});
    else initialize();
})();
