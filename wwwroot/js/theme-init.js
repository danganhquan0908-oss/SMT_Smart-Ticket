(() => {
    let theme = 'light', lang = 'vi';
    try {
        theme = localStorage.getItem('smartticket_theme') || theme;
        lang = localStorage.getItem('smartticket_lang') || lang;
    } catch {}
    document.documentElement.dataset.theme = theme === 'dark' ? 'dark' : 'light';
    document.documentElement.dataset.lang = lang === 'en' ? 'en' : 'vi';
    document.documentElement.lang = lang === 'en' ? 'en' : 'vi';
})();
