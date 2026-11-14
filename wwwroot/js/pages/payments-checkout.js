(() => {
    const page = document.getElementById('checkout-page');
    let remainingSeconds = Math.max(0, Math.floor((Date.parse(page.dataset.expiresAt) - Date.now()) / 1000));
    const token = page.dataset.token;
    const qrBox = document.getElementById('qr-wrap');
    const bankAppBtn = document.getElementById('bank-app');
    const saveQrBtn = document.getElementById('save-qr');
    const countdownEl = document.getElementById('countdown');
    const statusTitle = document.getElementById('status-title');
    const statusDesc = document.getElementById('status-desc');
    let isTerminated = false;
    let timer;



    // Countdown logic
    const updateCountdown = () => {
        const mins = String(Math.floor(remainingSeconds / 60)).padStart(2, '0');
        const secs = String(remainingSeconds % 60).padStart(2, '0');
        countdownEl.textContent = `${mins}:${secs}`;

        if (remainingSeconds < 60) {
            countdownEl.style.color = '#c81d4a';
        }

        if (remainingSeconds <= 0) {
            clearInterval(timer);
            isTerminated = true;
            qrBox.classList.add('expired');
            bankAppBtn.classList.add('disabled');
            bankAppBtn.removeAttribute('href');
            saveQrBtn.removeAttribute('href');
            statusTitle.textContent = "Hết hạn giữ chỗ!";
            statusDesc.textContent = "Thời gian thanh toán 7 phút đã kết thúc. Vui lòng đặt lại ghế.";
            return;
        }
        remainingSeconds--;
    };
    updateCountdown();
    if (!isTerminated) timer = setInterval(updateCountdown, 1000);

    // Polling payment state
    const pollPaymentStatus = async () => {
        if (isTerminated) return;
        try {
            const res = await fetch(`/api/payments/state?token=${encodeURIComponent(token)}`);
            if (res.ok) {
                const data = await res.json();
                if (data.status === 'Succeeded' && data.ticketLookupUrl) {
                    clearInterval(timer);
                    isTerminated = true;
                    statusTitle.textContent = "Thanh toán thành công!";
                    statusDesc.textContent = "Ngân hàng đã xác nhận giao dịch. Đang chuyển tới vé điện tử...";
                    setTimeout(() => {
                        window.location.href = data.ticketLookupUrl;
                    }, 1200);
                } else if (data.status === 'OverdueUnassigned') {
                    isTerminated = true;
                    statusTitle.textContent = "Giao dịch thanh toán muộn";
                    statusDesc.textContent = "Ghế đã được cấp lại do hết hạn 7 phút. Tổng đài viên Smart Ticket sẽ liên hệ hỗ trợ đổi chuyến hoặc hoàn tiền 100%.";
                } else if (data.status === 'Failed') {
                    isTerminated = true;
                    statusTitle.textContent = "Thanh toán chưa thành công";
                    statusDesc.textContent = "Ngân hàng báo lỗi giao dịch. Vui lòng kiểm tra lại.";
                }
            }
        } catch (err) {
            console.error("Polling error:", err);
        }
    };
    setInterval(pollPaymentStatus, 2500);
})();
