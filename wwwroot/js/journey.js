(() => {
    const from = document.getElementById('journey-from'), to = document.getElementById('journey-to');
    const date = document.getElementById('journey-date'), swap = document.getElementById('journey-swap');
    const clearValidation = () => to?.setCustomValidity('');
    swap?.addEventListener('click', () => {
        [from.value, to.value] = [to.value, from.value];
        clearValidation();
        swap.classList.remove('is-swapping');
        requestAnimationFrame(() => swap.classList.add('is-swapping'));
        setTimeout(() => swap.classList.remove('is-swapping'), 300);
    });
    const updateDates = () => document.querySelectorAll('[data-travel-date]').forEach(button => {
        button.setAttribute('aria-pressed', String(button.dataset.travelDate === date?.value));
    });
    document.querySelectorAll('[data-travel-date]').forEach(button => button.addEventListener('click', () => {
        date.value = button.dataset.travelDate;
        updateDates();
    }));
    date?.addEventListener('change', updateDates);
    document.getElementById('journey-form')?.addEventListener('submit', event => {
        clearValidation();
        if (from.value && from.value === to.value) {
            event.preventDefault();
            to.setCustomValidity(SMT.text('Bến đi và bến đến phải khác nhau.'));
            to.reportValidity();
        }
    });
    to?.addEventListener('change', clearValidation);
    from?.addEventListener('change', clearValidation);
    window.addEventListener('smt:languagechange', clearValidation);
    updateDates();
})();
