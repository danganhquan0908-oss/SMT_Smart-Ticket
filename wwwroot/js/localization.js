(() => {
    'use strict';
    const originals = new WeakMap();
    const attributes = new WeakMap();
    let language = 'vi';
    const normalize = value => value.replace(/\s+/g, ' ').trim();
    const patterns = [
        [/^(\d+) chuyến ngày (.+)$/, (_, n, day) => `${n} trips on ${day}`],
        [/^Còn (\d+) \/ (\d+) ghế$/, (_, free, total) => `${free} / ${total} seats available`],
        [/^(\d+) giờ (\d+) phút$/, (_, h, m) => `${h}h ${m}m`],
        [/^(\d+) ghế$/, (_, n) => `${n} seats`],
        [/^(\d+) vé$/, (_, n) => `${n} tickets`],
        [/^(\d+) chuyến chờ kiểm định$/, (_, n) => `${n} trips awaiting inspection`],
        [/^(\d+)% đã qua cổng bến$/, (_, n) => `${n}% checked in`],
        [/^Tầng (\d+)$/, (_, n) => `Deck ${n}`],
        [/^Giảm ([\d.,]+)%$/, (_, n) => `${n}% off`],
        [/^Còn ([\d.,]+) ₫ để lên hạng\.$/, (_, n) => `VND ${n} to the next tier.`],
        [/^(\d+) tầng$/, (_, n) => `${n} deck${n === '1' ? '' : 's'}`],
        [/^(\d+) chỗ$/, (_, n) => `${n} seats`],
        [/^\((\d+) ghế \/ tầng\)$/, (_, n) => `(${n} seats / deck)`],
        [/^\((\d+) (mẫu sơ đồ|tài xế|chuyến xe|phiên đối soát|hồ sơ đăng ký)\)$/, (_, n, unit) => `(${n} ${{'mẫu sơ đồ':'layouts','tài xế':'drivers','chuyến xe':'trips','phiên đối soát':'settlements','hồ sơ đăng ký':'applications'}[unit]})`],
        [/^(\d+) vé đã bán$/, (_, n) => `${n} tickets sold`],
        [/^(\d+) \/ (\d+) vé$/, (_, n, total) => `${n} / ${total} tickets`],
        [/^(Tất cả|Chờ duyệt|Đã duyệt|Đang chạy|Hoạt động|Từ chối) \((\d+)\)$/, (_, label, n) => `${{'Tất cả':'All','Chờ duyệt':'Pending','Đã duyệt':'Approved','Đang chạy':'Running','Hoạt động':'Active','Từ chối':'Declined'}[label]} (${n})`],
        [/^Tài xế: (.+)$/, (_, name) => `Driver: ${name}`],
        [/^Nộp lúc: (.+)$/, (_, date) => `Submitted: ${date}`],
        [/^KHU VỰC ĐỐI TÁC VẬN TẢI · (.+)$/, (_, company) => `OPERATOR WORKSPACE · ${company}`],
        [/^Trang (\d+)$/, (_, n) => `Page ${n}`],
        [/^(.+?) · (\d+) ghế$/, (_, label, n) => `${text(label)} · ${n} seats`]
    ];
    function text(value) {
        if (language !== 'en' || typeof value !== 'string') return value;
        const key = normalize(value);
        const catalog = window.SMT_EN || {};
        let translated = catalog[key];
        if (translated === undefined) {
            for (const [pattern, format] of patterns) {
                if (pattern.test(key)) { translated = key.replace(pattern, format); break; }
            }
        }
        if (translated === undefined && key.includes(' · ')) {
            translated = key.split(' · ').map(part => catalog[part] || part).join(' · ');
        }
        if (translated === undefined) {
            const decorated = key.match(/^([\p{Extended_Pictographic}\uFE0F\s+✓⚠!←→↗🔒]+)(.+?)([\s:…↗→]+)?$/u);
            if (decorated && catalog[decorated[2]]) translated = decorated[1] + catalog[decorated[2]] + (decorated[3] || '');
        }
        return translated === undefined ? value : value.replace(value.trim(), translated);
    }
    function renderNode(node) {
        const parent = node.parentElement;
        if (!parent || parent.closest('script,style,textarea,code,pre,[translate="no"]')) return;
        const keyed = parent.closest('[data-i18n]');
        if (keyed && window.SMT?.isStaticTranslation(keyed.dataset.i18n, keyed.textContent)) return;
        // A status changed by a page script must retain its current state on language changes.
        if (keyed && window.SMT?.t(keyed.dataset.i18n) !== keyed.dataset.i18n) keyed.removeAttribute('data-i18n');
        const current = node.nodeValue;
        let record = originals.get(node);
        if (!record || current !== record.rendered) record = {source:current};
        const rendered = text(record.source);
        record.rendered = rendered;
        originals.set(node, record);
        if (current !== rendered) node.nodeValue = rendered;
    }
    function renderElement(element) {
        if (element.closest('[translate="no"],script,style')) return;
        let records = attributes.get(element);
        if (!records) { records = {}; attributes.set(element, records); }
        for (const name of ['placeholder','title','aria-label','alt','data-val-required','data-val-length','data-val-regex','data-val-range','data-val-equalto']) {
            if (!element.hasAttribute(name)) continue;
            const key = element.getAttribute(`data-i18n-${name}`);
            if (key && window.SMT?.t(key) !== key) continue;
            const current = element.getAttribute(name);
            let record = records[name];
            if (!record || current !== record.rendered) record = {source:current};
            record.rendered = text(record.source);
            records[name] = record;
            if (current !== record.rendered) element.setAttribute(name, record.rendered);
        }
    }
    function render(scope = document.documentElement) {
        if (scope.nodeType === Node.TEXT_NODE) { renderNode(scope); return; }
        if (scope.nodeType !== Node.ELEMENT_NODE) return;
        renderElement(scope);
        const walker = document.createTreeWalker(scope, NodeFilter.SHOW_ELEMENT | NodeFilter.SHOW_TEXT);
        while (walker.nextNode()) {
            if (walker.currentNode.nodeType === Node.TEXT_NODE) renderNode(walker.currentNode);
            else renderElement(walker.currentNode);
        }
    }
    const observer = new MutationObserver(records => {
        observer.disconnect();
        for (const record of records) {
            if (record.type === 'childList') record.addedNodes.forEach(node => render(node));
            else if (record.type === 'characterData') renderNode(record.target);
            else renderElement(record.target);
        }
        observe();
    });
    function observe() { observer.observe(document.documentElement, {subtree:true,childList:true,characterData:true,attributes:true,attributeFilter:['placeholder','title','aria-label','alt']}); }
    window.SMTLocale = {
        text,
        apply(lang) {
            language = lang === 'en' ? 'en' : 'vi';
            document.documentElement.lang = language;
            observer.disconnect();
            render();
            observe();
        }
    };
    for (const name of ['alert','confirm']) {
        const original = window[name].bind(window);
        window[name] = message => original(text(String(message)));
    }
})();
