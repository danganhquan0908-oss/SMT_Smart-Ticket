(() => {
    'use strict';
    const root = document.documentElement;
    let activeTransition;
    window.SMTAppearance = {
        transition(update, event) {
            activeTransition?.skipTransition();
            if (matchMedia('(prefers-reduced-motion: reduce)').matches) { update(); return; }
            if (!document.startViewTransition) {
                root.classList.add('is-theme-fallback');
                update();
                setTimeout(() => root.classList.remove('is-theme-fallback'), 320);
                return;
            }
            const bounds = document.getElementById('btn-theme-toggle')?.getBoundingClientRect();
            const x = event?.clientX || (bounds ? bounds.left + bounds.width / 2 : innerWidth - 40);
            const y = event?.clientY || (bounds ? bounds.top + bounds.height / 2 : 40);
            const radius = Math.hypot(Math.max(x, innerWidth - x), Math.max(y, innerHeight - y));
            root.classList.add('is-theme-transitioning');
            const transition = document.startViewTransition(update);
            activeTransition = transition;
            transition.ready.then(() => {
                root.animate({ clipPath:[`circle(0px at ${x}px ${y}px)`,`circle(${radius}px at ${x}px ${y}px)`] }, {
                    duration:460, easing:'cubic-bezier(0.2,0,0.25,1)', pseudoElement:'::view-transition-new(root)'
                });
            }).catch(() => {});
            transition.finished.catch(() => {}).finally(() => {
                if (activeTransition === transition) {
                    activeTransition = undefined;
                    root.classList.remove('is-theme-transitioning');
                }
            });
        }
    };
})();
