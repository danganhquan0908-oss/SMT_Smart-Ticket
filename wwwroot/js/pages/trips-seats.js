(() => {
    const page = JSON.parse(document.getElementById('seat-page-data').textContent);
    const { tripId, pricePerSeat, isAuthenticated } = page;
    const api = `/api/trips/${tripId}`;
    const selected = new Set();
    let seats = page.seats;

    const badgeSeats = document.getElementById('selected-seats-badge');
    const badgeCount = document.getElementById('selected-count-badge');
    const totalDisplay = document.getElementById('total-price-display');
    const btnSubmit = document.getElementById('btn-submit-hold');
    const msgBox = document.getElementById('hold-message');

    const render = () => {
        for (const deck of [1, 2]) {
            const container = document.getElementById(`deck-${deck}`);
            container.replaceChildren();

            const deckSeats = seats.filter(x => x.deck === deck);
            for (const s of deckSeats) {
                const btn = document.createElement('button');
                btn.type = 'button';
                btn.className = `btn-seat ${s.status.toLowerCase()}`;
                if (selected.has(s.code)) btn.classList.add('selected');

                if (s.status === 'Available') {
                    btn.innerHTML = selected.has(s.code) ? `${s.code} <small>✓</small>` : s.code;
                } else if (s.status === 'Holding') {
                    btn.innerHTML = `${s.code} <small>🔒</small>`;
                    btn.disabled = !selected.has(s.code);
                } else {
                    btn.innerHTML = `${s.code} <small>✕</small>`;
                    btn.disabled = true;
                }

                btn.onclick = () => {
                    if (selected.has(s.code)) {
                        selected.delete(s.code);
                    } else {
                        if (selected.size >= 8) {
                            alert("Bạn chỉ có thể chọn tối đa 8 ghế trong một lần đặt.");
                            return;
                        }
                        selected.add(s.code);
                    }
                    render();
                };
                container.appendChild(btn);
            }
        }

        // Cập nhật tổng kết
        const count = selected.size;
        const isEn = window.SMT && window.SMT.currentLang === 'en';
        if (count > 0) {
            badgeSeats.textContent = [...selected].sort().join(', ');
            badgeCount.textContent = count + (isEn ? ' tickets' : ' vé');
            const total = count * pricePerSeat;
            totalDisplay.textContent = total.toLocaleString('vi-VN') + ' ₫';
            btnSubmit.classList.remove('btn-dimmed');
            msgBox.style.display = 'none';
        } else {
            badgeSeats.textContent = window.SMT ? window.SMT.t('sum_no_seats') : 'Chưa chọn ghế (Bấm vào ghế trên sơ đồ)';
            badgeCount.textContent = '0 ' + (isEn ? 'tickets' : 'vé');
            totalDisplay.textContent = '0 ₫';
            btnSubmit.classList.add('btn-dimmed');
        }
    };

    window.addEventListener('smt:languagechange', () => {
        render();
    });

    const refreshSeats = async () => {
        try {
            const res = await fetch(`${api}/seats`);
            if (res.ok) {
                seats = await res.json();
                render();
            }
        } catch (err) {
            console.error("Refresh error:", err);
        }
    };

    render();

    // SignalR realtime sync
    if (window.signalR) {
        try {
            const hub = new signalR.HubConnectionBuilder()
                .withUrl('/hubs/trip-seats')
                .withAutomaticReconnect()
                .build();
            hub.on('SeatsChanged', async () => await refreshSeats());
            hub.start()
                .then(() => hub.invoke('JoinTrip', tripId))
                .catch(err => console.warn("SignalR warning:", err));
        } catch (err) {
            console.warn("SignalR warning:", err);
        }
    }

    // Quản lý Modal VietQR Popup In-Page
    let modalTimer = null;
    let modalPollTimer = null;
    let modalRemaining = 420;
    let currentToken = null;
    let currentBookingCode = null;
    let currentAmount = 0;

    const modalBackdrop = document.getElementById('qr-modal-backdrop');
    const modalQrImg = document.getElementById('modal-qr-image');
    const modalCountdownEl = document.getElementById('modal-countdown-badge');
    const modalAmountEl = document.getElementById('modal-amount-display');
    const modalRefEl = document.getElementById('modal-ref-display');
    const modalCheckoutLink = document.getElementById('modal-checkout-link');
    const modalExpiredOverlay = document.getElementById('modal-qr-overlay-expired');
    const modalStatusTitle = document.getElementById('modal-status-title');
    const modalStatusDesc = document.getElementById('modal-status-desc');
    const btnCloseModal = document.getElementById('btn-close-modal');


    const openQrModal = (token, bookingCode, amount, qrImage = null, expiresAtUtc = null) => {
        currentToken = token;
        currentBookingCode = bookingCode;
        currentAmount = amount;
        modalRemaining = expiresAtUtc ? Math.max(0, Math.floor((Date.parse(expiresAtUtc) - Date.now()) / 1000)) : 420;

        const amountFormatted = Number(amount).toString();
        const refText = 'STKT' + bookingCode;

        modalAmountEl.textContent = Number(amount).toLocaleString('vi-VN') + ' ₫';
        modalRefEl.textContent = refText;
        modalCheckoutLink.href = `/payments/checkout?token=${encodeURIComponent(token)}`;
        modalExpiredOverlay.style.display = 'none';
        modalStatusTitle.textContent = "Đang chờ bạn quét mã thanh toán...";
        modalStatusDesc.textContent = "Hệ thống tự động kích hoạt vé ngay khi giao dịch thành công.";
        modalStatusTitle.style.color = "var(--ink, #1a162b)";

        modalQrImg.hidden = !qrImage;
        if (qrImage) {
            modalQrImg.src = qrImage;
        } else {
            modalQrImg.removeAttribute('src');
            modalStatusTitle.textContent = "Chưa cấu hình ngân hàng thanh toán";
            modalStatusDesc.textContent = "Thanh toán trực tuyến chưa sẵn sàng. Vui lòng liên hệ nhà xe; không chuyển tiền khi chưa có thông tin ngân hàng.";
        }

        // Gán sự kiện nút sao chép trong modal
        const copyAmountBtn = document.getElementById('btn-modal-copy-amount');
        if (copyAmountBtn) {
            copyAmountBtn.onclick = () => {
                navigator.clipboard.writeText(amountFormatted);
                copyAmountBtn.textContent = '✓ Đã chép';
                setTimeout(() => copyAmountBtn.textContent = 'Sao chép', 2000);
            };
        }

        const copyRefBtn = document.getElementById('btn-modal-copy-ref');
        if (copyRefBtn) {
            copyRefBtn.onclick = () => {
                navigator.clipboard.writeText(refText);
                copyRefBtn.textContent = '✓ Đã chép';
                setTimeout(() => copyRefBtn.textContent = 'Sao chép', 2000);
            };
        }

        // Đồng hồ đếm ngược 7 phút
        const updateModalCountdown = () => {
            const mins = String(Math.floor(modalRemaining / 60)).padStart(2, '0');
            const secs = String(modalRemaining % 60).padStart(2, '0');
            modalCountdownEl.textContent = `⏱️ ${mins}:${secs}`;
            if (modalRemaining <= 0) {
                clearInterval(modalTimer);
                clearInterval(modalPollTimer);
                modalExpiredOverlay.style.display = 'flex';
                modalStatusTitle.textContent = "Hết hạn giữ chỗ 7 phút!";
                modalStatusDesc.textContent = "Ghế đã được giải phóng. Vui lòng đóng cửa sổ và chọn lại ghế.";
                return;
            }
            modalRemaining--;
        };
        clearInterval(modalTimer);
        updateModalCountdown();
        modalTimer = setInterval(updateModalCountdown, 1000);

        // Polling kiểm tra trạng thái thanh toán theo chu kỳ 2.5 giây
        clearInterval(modalPollTimer);
        modalPollTimer = setInterval(async () => {
            if (!currentToken) return;
            try {
                const res = await fetch(`/api/payments/state?token=${encodeURIComponent(currentToken)}`);
                if (res.ok) {
                    const data = await res.json();
                    if (data.status === 'Succeeded' && data.ticketLookupUrl) {
                        clearInterval(modalTimer);
                        clearInterval(modalPollTimer);
                        modalStatusTitle.textContent = "✓ Thanh toán thành công!";
                        modalStatusDesc.textContent = "Ngân hàng đã xác nhận thanh toán. Đang chuyển tới vé điện tử...";
                        modalStatusTitle.style.color = "#059669";
                        setTimeout(() => {
                            window.location.href = data.ticketLookupUrl;
                        }, 1200);
                    }
                }
            } catch (err) {
                console.warn("Lỗi kiểm tra trạng thái thanh toán:", err);
            }
        }, 2500);

        // Hiển thị modal
        modalBackdrop.style.display = 'flex';
    };

    const closeQrModal = () => {
        modalBackdrop.style.display = 'none';
        clearInterval(modalTimer);
        clearInterval(modalPollTimer);
    };

    if (btnCloseModal) {
        btnCloseModal.addEventListener('click', closeQrModal);
    }
    if (modalBackdrop) {
        modalBackdrop.addEventListener('click', (e) => {
            if (e.target === modalBackdrop) closeQrModal();
        });
    }



    // Submit form giữ chỗ
    document.getElementById('hold-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        msgBox.style.display = 'none';

        if (!selected.size) {
            msgBox.innerHTML = '<strong>⚠️ Bạn chưa chọn ghế:</strong> Vui lòng bấm chọn ít nhất một ghế (ví dụ: A01, A02...) trên sơ đồ xe trước khi tiến hành thanh toán.';
            msgBox.style.display = 'block';
            document.querySelector('.map-column')?.scrollIntoView({ behavior: 'smooth', block: 'center' });
            return;
        }

        if (!isAuthenticated) {
            msgBox.innerHTML = '<strong>🔒 Cần đăng nhập:</strong> Đang chuyển hướng bạn tới trang đăng nhập để lưu vé vào tài khoản...';
            msgBox.style.display = 'block';
            setTimeout(() => {
                window.location.href = `/account/login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
            }, 800);
            return;
        }

        btnSubmit.disabled = true;
        btnSubmit.innerHTML = '<span>Đang tạo mã giữ chỗ & sinh mã VietQR...</span>';

        const form = new FormData(e.currentTarget);
        try {
            const res = await fetch(`${api}/holds`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    passengerName: form.get('passengerName'),
                    passengerPhone: form.get('passengerPhone'),
                    passengerEmail: form.get('passengerEmail') || null,
                    expectedUnitPrice: pricePerSeat,
                    seatCodes: [...selected]
                })
            });

            const rawText = await res.text();
            let data = {};
            try {
                data = rawText ? JSON.parse(rawText) : {};
            } catch {
                data = { detail: rawText };
            }
            if (!res.ok) {
                msgBox.textContent = data.detail || data.title || 'Không thể giữ ghế. Ghế có thể vừa được người khác chọn. Vui lòng thử lại.';
                msgBox.style.display = 'block';
                msgBox.scrollIntoView({ behavior: 'smooth', block: 'center' });
                btnSubmit.disabled = false;
                btnSubmit.innerHTML = '<span>Tiến hành giữ chỗ & Thanh toán QR</span><svg width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24"><path d="M5 12h14M12 5l7 7-7 7"/></svg>';
                await refreshSeats();
                return;
            }

            const token = data.lookupToken || data.LookupToken;
            const bCode = data.bookingCode || data.BookingCode;
            const tAmount = data.totalAmount || data.TotalAmount || (selected.size * pricePerSeat);
            const qrImgData = data.qrImage || data.QrImage;

            if (token) {
                btnSubmit.disabled = false;
                btnSubmit.innerHTML = '<span>Tiến hành giữ chỗ & Thanh toán QR</span><svg width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24"><path d="M5 12h14M12 5l7 7-7 7"/></svg>';

                // MỞ MODAL VIETQR NGAY LẬP TỨC TRÊN MÀN HÌNH!
                openQrModal(token, bCode, tAmount, qrImgData, data.expiresAtUtc || data.ExpiresAtUtc);
            } else {
                msgBox.textContent = 'Lỗi hệ thống: không tìm thấy mã thanh toán.';
                msgBox.style.display = 'block';
                btnSubmit.disabled = false;
                btnSubmit.innerHTML = '<span>Tiến hành giữ chỗ & Thanh toán QR</span><svg width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24"><path d="M5 12h14M12 5l7 7-7 7"/></svg>';
            }
        } catch (err) {
            msgBox.textContent = 'Lỗi kết nối máy chủ. Vui lòng thử lại sau.';
            msgBox.style.display = 'block';
            btnSubmit.disabled = false;
            btnSubmit.innerHTML = '<span>Tiến hành giữ chỗ & Thanh toán QR</span><svg width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24"><path d="M5 12h14M12 5l7 7-7 7"/></svg>';
        }
    });
})();
