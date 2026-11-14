function updateLivePreview() {
        const deckCountSelect = document.getElementById('deckCountSelect');
        const seatsPerDeckInput = document.getElementById('seatsPerDeckInput');
        const summary = document.getElementById('previewSummary');
        const deck2Panel = document.getElementById('deck2Panel');
        const deck1Pills = document.getElementById('deck1Pills');
        const deck2Pills = document.getElementById('deck2Pills');

        if (!deckCountSelect || !seatsPerDeckInput || !summary) return;

        const decks = parseInt(deckCountSelect.value) || 1;
        const seats = Math.max(1, Math.min(60, parseInt(seatsPerDeckInput.value) || 1));
        const total = decks * seats;

        summary.textContent = `Tổng sức chứa: ${total} chỗ (${decks} tầng)`;

        // Deck 1
        let d1Html = '';
        if (seats <= 4) {
            for (let i = 1; i <= seats; i++) {
                const num = i < 10 ? '0' + i : i;
                d1Html += `<span class="code-pill">A${num}</span> `;
            }
        } else {
            const last = seats < 10 ? '0' + seats : seats;
            d1Html = `<span class="code-pill">A01</span> <span class="code-pill">A02</span> <span class="code-pill">...</span> <span class="code-pill">A${last}</span>`;
        }
        if (deck1Pills) deck1Pills.innerHTML = d1Html;

        // Deck 2
        if (decks > 1) {
            if (deck2Panel) deck2Panel.style.display = 'block';
            let d2Html = '';
            if (seats <= 4) {
                for (let i = 1; i <= seats; i++) {
                    const num = i < 10 ? '0' + i : i;
                    d2Html += `<span class="code-pill" style="background:#fff0f3; color:#c81d4a; border-color:#f7aabf;">B${num}</span> `;
                }
            } else {
                const last = seats < 10 ? '0' + seats : seats;
                d2Html = `<span class="code-pill" style="background:#fff0f3; color:#c81d4a; border-color:#f7aabf;">B01</span> <span class="code-pill" style="background:#fff0f3; color:#c81d4a; border-color:#f7aabf;">B02</span> <span class="code-pill" style="background:#fff0f3; color:#c81d4a; border-color:#f7aabf;">...</span> <span class="code-pill" style="background:#fff0f3; color:#c81d4a; border-color:#f7aabf;">B${last}</span>`;
            }
            if (deck2Pills) deck2Pills.innerHTML = d2Html;
        } else {
            if (deck2Panel) deck2Panel.style.display = 'none';
        }
    }

    document.addEventListener('DOMContentLoaded', updateLivePreview);
