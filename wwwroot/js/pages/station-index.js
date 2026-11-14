(() => {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    const ticketInput = document.getElementById('ticket-token');
    const verifyBtn = document.getElementById('verify-ticket');
    const clearBtn = document.getElementById('btn-clear');
    const soundToggle = document.getElementById('btn-sound-toggle');
    const soundStateLabel = document.getElementById('sound-state');
    const soundIcon = document.getElementById('sound-icon');
    const resCard = document.getElementById('scan-result-card');
    const resIcon = document.getElementById('res-icon');
    const resHeadline = document.getElementById('res-headline');
    const resMessage = document.getElementById('res-message');
    const resMetaGrid = document.getElementById('res-meta-grid');
    const resSeat = document.getElementById('res-seat-code');
    const resTrip = document.getElementById('res-trip-name');
    const resDep = document.getElementById('res-departure-time');
    const resTime = document.getElementById('res-scan-time');
    const liveLatency = document.getElementById('live-latency');
    const dodStatus = document.getElementById('dod-status');
    const recentBody = document.getElementById('recent-scans-body');
    const queueKey = 'smartticket-offline-scan-queue';
    const queueBadge = document.getElementById('sync-badge');
    const queueCountSpan = document.getElementById('queue-count');
    const btnSyncNow = document.getElementById('btn-sync-now');
    const netStatus = document.getElementById('net-status');

    let isSoundOn = true;
    let recentScans = [];

    // REALTIME CLOCK (GMT+7)
    function updateClock() {
        const now = new Date();
        const d = String(now.getDate()).padStart(2, '0');
        const m = String(now.getMonth() + 1).padStart(2, '0');
        const y = now.getFullYear();
        const h = String(now.getHours()).padStart(2, '0');
        const min = String(now.getMinutes()).padStart(2, '0');
        const s = String(now.getSeconds()).padStart(2, '0');
        const clockEl = document.getElementById('station-clock');
        if (clockEl) clockEl.textContent = `${h}:${min}:${s} · ${d}/${m}/${y}`;
    }
    setInterval(updateClock, 1000);
    updateClock();

    // SOUND SYNTHESIZER (WEB AUDIO API)
    function playAudioTone(type) {
        if (!isSoundOn) return;
        try {
            const ctx = new (window.AudioContext || window.webkitAudioContext)();
            const now = ctx.currentTime;

            if (type === 'success') {
                // Two-tone pleasant chord (880Hz -> 1320Hz)
                const osc1 = ctx.createOscillator();
                const gain1 = ctx.createGain();
                osc1.type = 'sine';
                osc1.frequency.setValueAtTime(880, now);
                osc1.frequency.exponentialRampToValueAtTime(1320, now + 0.15);
                gain1.gain.setValueAtTime(0.18, now);
                gain1.gain.exponentialRampToValueAtTime(0.001, now + 0.22);
                osc1.connect(gain1);
                gain1.connect(ctx.destination);
                osc1.start(now);
                osc1.stop(now + 0.22);
            }
            else if (type === 'alarm') {
                // Piercing alternating alarm siren (190Hz / 250Hz pulsing)
                for (let i = 0; i < 3; i++) {
                    const osc = ctx.createOscillator();
                    const gain = ctx.createGain();
                    osc.type = 'sawtooth';
                    const t = now + (i * 0.25);
                    osc.frequency.setValueAtTime(190, t);
                    osc.frequency.setValueAtTime(260, t + 0.12);
                    gain.gain.setValueAtTime(0.35, t);
                    gain.gain.linearRampToValueAtTime(0.01, t + 0.23);
                    osc.connect(gain);
                    gain.connect(ctx.destination);
                    osc.start(t);
                    osc.stop(t + 0.24);
                }
            }
            else {
                // Warning descending chime (440Hz -> 330Hz)
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();
                osc.type = 'triangle';
                osc.frequency.setValueAtTime(440, now);
                osc.frequency.exponentialRampToValueAtTime(330, now + 0.25);
                gain.gain.setValueAtTime(0.2, now);
                gain.gain.exponentialRampToValueAtTime(0.01, now + 0.3);
                osc.connect(gain);
                gain.connect(ctx.destination);
                osc.start(now);
                osc.stop(now + 0.3);
            }
        } catch (e) {
            console.warn('Audio tone could not play', e);
        }
    }

    // SOUND TOGGLE
    soundToggle.addEventListener('click', () => {
        isSoundOn = !isSoundOn;
        soundStateLabel.textContent = isSoundOn ? 'BẬT' : 'TẮT';
        soundIcon.textContent = isSoundOn ? '🔊' : '🔇';
        soundToggle.style.color = isSoundOn ? '#5e17eb' : '#8c8699';
    });

    // NETWORK MONITORING & QUEUE
    function updateQueueDisplay() {
        const q = JSON.parse(localStorage.getItem(queueKey) || '[]');
        queueCountSpan.textContent = q.length;
        if (q.length > 0) {
            queueBadge.style.background = 'rgba(247, 37, 133, 0.2)';
            queueBadge.style.borderColor = 'rgba(247, 37, 133, 0.4)';
            queueBadge.style.color = '#ffb3d9';
            btnSyncNow.style.display = 'inline-block';
        } else {
            queueBadge.style.background = 'rgba(255, 255, 255, 0.08)';
            queueBadge.style.borderColor = 'rgba(255, 255, 255, 0.12)';
            queueBadge.style.color = '#cbd5e1';
            btnSyncNow.style.display = 'none';
        }
    }

    function setOnlineStatus(online) {
        if (online) {
            netStatus.className = 'net-pill online';
            netStatus.innerHTML = '<i class="dot"></i><span>Trực tuyến (Online)</span>';
        } else {
            netStatus.className = 'net-pill offline';
            netStatus.innerHTML = '<i class="dot"></i><span>Ngoại tuyến (Offline Mode)</span>';
        }
    }

    window.addEventListener('online', async () => {
        setOnlineStatus(true);
        await flushOfflineQueue();
    });
    window.addEventListener('offline', () => {
        setOnlineStatus(false);
    });
    setOnlineStatus(navigator.onLine);
    updateQueueDisplay();

    // FLUSH QUEUE
    async function flushOfflineQueue() {
        const q = JSON.parse(localStorage.getItem(queueKey) || '[]');
        if (q.length === 0) return;
        localStorage.removeItem(queueKey);
        updateQueueDisplay();
        for (const item of q) {
            await executeScan(item.jwt, true, item.scannedAtUtc);
        }
    }
    btnSyncNow.addEventListener('click', flushOfflineQueue);

    function decodeJwtPayload(token) {
        try {
            const parts = token.split('.');
            if (parts.length !== 3) return null;
            const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
            const pad = base64.length % 4;
            const padded = pad ? base64 + '='.repeat(4 - pad) : base64;
            return JSON.parse(decodeURIComponent(escape(atob(padded))));
        } catch {
            return null;
        }
    }


    // CORE SCAN EXECUTION
    async function executeScan(jwt, isOffline = false, scannedAtUtc = null) {
        jwt = jwt.trim();
        if (!jwt) return;

        const startTime = performance.now();

        // Queue scans only when the device is offline.
        if (!navigator.onLine) {
            const payload = decodeJwtPayload(jwt);
            const q = JSON.parse(localStorage.getItem(queueKey) || '[]');
            const timestamp = new Date().toISOString();

            // Check if expired offline
            if (payload && payload.exp && payload.exp < Date.now() / 1000) {
                resCard.className = 'scan-result-card rejected';
                resIcon.textContent = '⛔';
                resHeadline.textContent = 'TỪ CHỐI · VÉ ĐÃ HẾT HẠN (NGOẠI TUYẾN)';
                resMessage.textContent = 'Mã vé đã hết hạn hiệu lực theo thông tin ký số trên vé.';
                resMetaGrid.style.display = 'grid';
                resSeat.textContent = payload.seat || '--';
                resTrip.textContent = payload.trip ? payload.trip.substring(0, 8) + '...' : '--';
                resDep.textContent = new Date(payload.exp * 1000).toLocaleString('vi-VN');
                resTime.textContent = '0 ms (Offline)';
                playAudioTone('warning');
                addAuditLog(new Date(), payload.seat || '--', '--', 'HẾT HẠN (OFFLINE)', 'Offline', 0);
                return;
            }

            // Check if already scanned in local offline queue
            if (q.some(x => x.jwt === jwt)) {
                resCard.className = 'scan-result-card alarm';
                resIcon.textContent = '🚨';
                resHeadline.textContent = '⚠️ CẢNH BÁO: VÉ ĐÃ QUÉT TRƯỚC ĐÓ (NGOẠI TUYẾN)!';
                resMessage.textContent = `Vé ghế ${payload?.seat || ''} đã được quét trước đó trong ca trực ngoại tuyến hiện tại. Cảnh báo quét trùng lặp!`;
                resMetaGrid.style.display = 'grid';
                resSeat.textContent = payload?.seat || '--';
                resTrip.textContent = payload?.trip ? payload.trip.substring(0, 8) + '...' : '--';
                resDep.textContent = payload?.exp ? new Date(payload.exp * 1000).toLocaleString('vi-VN') : '--';
                resTime.textContent = '0 ms (Offline)';
                playAudioTone('alarm');
                addAuditLog(new Date(), payload?.seat || '--', '--', 'TRÙNG VÉ (OFFLINE)', 'Offline', 0);
                if ('vibrate' in navigator) navigator.vibrate([300, 100, 300, 100, 300]);
                return;
            }

            q.push({ jwt, isOffline: true, scannedAtUtc: timestamp });
            localStorage.setItem(queueKey, JSON.stringify(q));
            updateQueueDisplay();

            resCard.className = 'scan-result-card pending';
            resIcon.textContent = '💾';
            resHeadline.textContent = 'VÉ HỢP LỆ · ĐÃ LƯU HÀNG ĐỢI POS (NGOẠI TUYẾN)';
            resMessage.textContent = 'Xác thực cấu trúc mã JWT thành công khi không có kết nối. Lượt quét đã lưu cục bộ và sẽ tự động đồng bộ khi có mạng.';
            resMetaGrid.style.display = 'grid';
            resSeat.textContent = payload?.seat || '--';
            resTrip.textContent = payload?.trip ? payload.trip.substring(0, 8) + '...' : '--';
            resDep.textContent = payload?.exp ? new Date(payload.exp * 1000).toLocaleString('vi-VN') : '--';
            resTime.textContent = '0 ms (Offline)';
            playAudioTone('success');
            addAuditLog(new Date(), payload?.seat || '--', '--', 'ĐÃ LƯU (OFFLINE)', 'Offline', 0);
            return;
        }

        try {
            const response = await fetch('/api/operations/scan', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ jwt, isOffline, scannedAtUtc })
            });

            const latencyMs = Math.round(performance.now() - startTime);
            liveLatency.textContent = `⚡ ${latencyMs} ms`;
            dodStatus.textContent = 'Đã nhận phản hồi từ máy chủ';
            dodStatus.className = 'kpi-sub';

            const data = await response.json();

            if (data.accepted) {
                // SUCCESS
                resCard.className = 'scan-result-card success';
                resIcon.textContent = '✅';
                resHeadline.textContent = 'VÉ HỢP LỆ · QUA CỔNG AN TOÀN';
                resMessage.textContent = `Hành khách có vé hợp lệ và đã được xác thực thành công tại cổng bến xe.`;
                resMetaGrid.style.display = 'grid';
                resSeat.textContent = data.seatCode || '--';
                resTrip.textContent = data.trip || '--';
                resDep.textContent = data.departureTimeUtc ? new Date(data.departureTimeUtc).toLocaleString('vi-VN') : '--';
                resTime.textContent = `${latencyMs} ms`;

                playAudioTone('success');
                addAuditLog(new Date(), data.seatCode || '--', data.trip || '--', 'HỢP LỆ', isOffline ? 'Offline' : 'Online', latencyMs);

                // Vibrate if supported
                if ('vibrate' in navigator) navigator.vibrate([100, 50, 100]);
            }
            else if (data.result === 'ALREADY_USED') {
                // CRITICAL ALARM: REUSE DETECTED
                resCard.className = 'scan-result-card alarm';
                resIcon.textContent = '🚨';
                resHeadline.textContent = '⚠️ CẢNH BÁO NGUY HIỂM: VÉ ĐÃ ĐƯỢC QUÉT TRƯỚC ĐÓ!';
                resMessage.textContent = `Mã vé ghế ${data.seatCode || ''} (${data.trip || ''}) đã qua cổng check-in trước đây. Nghi vấn gian lận vé, vé chụp lại màn hình hoặc nhiều người đi chung 1 vé!`;
                resMetaGrid.style.display = 'grid';
                resSeat.textContent = data.seatCode || '--';
                resTrip.textContent = data.trip || '--';
                resDep.textContent = data.departureTimeUtc ? new Date(data.departureTimeUtc).toLocaleString('vi-VN') : '--';
                resTime.textContent = `${latencyMs} ms`;

                playAudioTone('alarm');
                addAuditLog(new Date(), data.seatCode || '--', data.trip || '--', 'ĐÃ DÙNG (CẢNH BÁO)', isOffline ? 'Offline' : 'Online', latencyMs);

                if ('vibrate' in navigator) navigator.vibrate([300, 100, 300, 100, 300]);
            }
            else {
                // REJECTED
                resCard.className = 'scan-result-card rejected';
                resIcon.textContent = '⛔';
                resHeadline.textContent = 'TỪ CHỐI QUA CỔNG';
                resMessage.textContent = data.result === 'OUTSIDE_TRIP_WINDOW'
                    ? 'Vé nằm ngoài khung giờ soát vé cho phép của chuyến (chỉ mở trước giờ chạy 12h).'
                    : `Vé không hợp lệ hoặc đã bị hủy (${data.result}).`;
                resMetaGrid.style.display = 'grid';
                resSeat.textContent = data.seatCode || '--';
                resTrip.textContent = data.trip || '--';
                resDep.textContent = data.departureTimeUtc ? new Date(data.departureTimeUtc).toLocaleString('vi-VN') : '--';
                resTime.textContent = `${latencyMs} ms`;

                playAudioTone('warning');
                addAuditLog(new Date(), data.seatCode || '--', data.trip || '--', `TỪ CHỐI (${data.result})`, isOffline ? 'Offline' : 'Online', latencyMs);
            }
        }
        catch (err) {
            console.error('Scan error:', err);
            resCard.className = 'scan-result-card pending';
            resIcon.textContent = '💾';
            resHeadline.textContent = 'MẤT KẾT NỐI MÁY CHỦ · ĐÃ LƯU HÀNG ĐỢI';
            resMessage.textContent = 'Không gửi được yêu cầu lên máy chủ; lượt quét đã được lưu lại để đồng bộ sau.';
            resMetaGrid.style.display = 'none';

            const q = JSON.parse(localStorage.getItem(queueKey) || '[]');
            q.push({ jwt, isOffline: true, scannedAtUtc: new Date().toISOString() });
            localStorage.setItem(queueKey, JSON.stringify(q));
            updateQueueDisplay();
            playAudioTone('warning');
            addAuditLog(new Date(), 'OFFLINE', '--', 'Đã lưu hàng đợi', 'Offline', 0);
        }
    }

    // AUDIT LOG IN-MEMORY
    function addAuditLog(time, seat, trip, status, mode, latency) {
        const item = {
            time: time.toLocaleTimeString('vi-VN'),
            seat,
            trip,
            status,
            mode,
            latency
        };
        recentScans.unshift(item);
        if (recentScans.length > 15) recentScans.pop();
        renderAuditLog();
    }

    function renderAuditLog() {
        if (recentScans.length === 0) {
            recentBody.innerHTML = '<tr class="empty-row"><td colspan="6">Chưa có lượt quét nào trong phiên làm việc này.</td></tr>';
            return;
        }
        recentBody.innerHTML = recentScans.map(s => {
            const statusClass = s.status === 'HỢP LỆ' ? 'success' : (s.status.includes('CẢNH BÁO') ? 'alarm' : 'rejected');
            return `<tr>
                <td>${s.time}</td>
                <td><strong>${s.seat}</strong></td>
                <td>${s.trip}</td>
                <td><span class="status-badge-mini ${statusClass}">${s.status}</span></td>
                <td>${s.mode}</td>
                <td>${s.latency ? s.latency + ' ms' : '--'}</td>
            </tr>`;
        }).join('');
    }

    document.getElementById('btn-clear-log').addEventListener('click', () => {
        recentScans = [];
        renderAuditLog();
    });

    // INPUT EVENTS
    verifyBtn.addEventListener('click', () => {
        const val = ticketInput.value.trim();
        if (val) {
            executeScan(val);
            ticketInput.value = '';
            ticketInput.focus();
        }
    });

    ticketInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            verifyBtn.click();
        }
    });

    clearBtn.addEventListener('click', () => {
        ticketInput.value = '';
        ticketInput.focus();
    });

    // CAMERA SCANNER
    const startCamBtn = document.getElementById('start-camera');
    const camWrapper = document.getElementById('camera-wrapper');
    const video = document.getElementById('camera');
    const closeCamBtn = document.getElementById('btn-cam-close');
    const switchCamBtn = document.getElementById('btn-cam-switch');
    let camStream = null;
    let currentFacingMode = 'environment';
    let camScanning = false;

    startCamBtn.addEventListener('click', async () => {
        await startCamera(currentFacingMode);
    });

    closeCamBtn.addEventListener('click', stopCamera);

    switchCamBtn.addEventListener('click', async () => {
        currentFacingMode = currentFacingMode === 'environment' ? 'user' : 'environment';
        await startCamera(currentFacingMode);
    });

    async function startCamera(facingMode) {
        if (camStream) stopCamera();
        try {
            camStream = await navigator.mediaDevices.getUserMedia({
                video: { facingMode: { ideal: facingMode } }
            });
            video.srcObject = camStream;
            await video.play();
            camWrapper.style.display = 'block';
            camScanning = true;
            scanCameraLoop();
        } catch (err) {
            alert('Không mở được camera. Vui lòng cấp quyền truy cập camera cho trình duyệt.');
            console.error('Camera error', err);
        }
    }

    function stopCamera() {
        camScanning = false;
        if (camStream) {
            camStream.getTracks().forEach(t => t.stop());
            camStream = null;
        }
        camWrapper.style.display = 'none';
    }

    async function scanCameraLoop() {
        if (!camScanning) return;
        if ('BarcodeDetector' in window) {
            try {
                const detector = new BarcodeDetector({ formats: ['qr_code'] });
                const barcodes = await detector.detect(video);
                if (barcodes.length > 0) {
                    const rawVal = barcodes[0].rawValue;
                    if (rawVal) {
                        stopCamera();
                        ticketInput.value = rawVal;
                        await executeScan(rawVal);
                        return;
                    }
                }
            } catch (err) {
                // continue loop
            }
        }
        if (camScanning) requestAnimationFrame(scanCameraLoop);
    }

    // FILTER TRIPS
    const filterButtons = document.querySelectorAll('.filter-chip');
    const tripCards = document.querySelectorAll('.trip-manifest-card');

    filterButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            filterButtons.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            const cat = btn.dataset.filter;

            tripCards.forEach(card => {
                if (cat === 'all') {
                    card.style.display = 'flex';
                } else if (cat === 'scheduled') {
                    card.style.display = card.classList.contains('cat-scheduled') ? 'flex' : 'none';
                } else if (cat === 'approved') {
                    card.style.display = card.classList.contains('cat-approved') ? 'flex' : 'none';
                } else if (cat === 'running') {
                    card.style.display = card.classList.contains('cat-running') ? 'flex' : 'none';
                }
            });
        });
    });

    // GLOBAL EXPORTS FOR MODALS
    window.openManifestModal = async (tripId, dep, arr, time, plate, driver) => {
        const modal = document.getElementById('manifest-modal');
        document.getElementById('mf-trip-title').textContent = `${dep} → ${arr}`;
        document.getElementById('mf-trip-sub').textContent = `Xe: ${plate} · Lái xe: ${driver} · Giờ chạy: ${time}`;
        const tbody = document.getElementById('manifest-tbody');
        tbody.innerHTML = '<tr><td colspan="6" class="loading-cell">Đang tải danh sách vé...</td></tr>';
        modal.style.display = 'grid';

        try {
            const res = await fetch(`/api/operations/trips/${tripId}/manifest`);
            const data = await res.json();
            window.currentManifestData = data;

            document.getElementById('mf-sold-count').textContent = data.soldTicketCount;
            document.getElementById('mf-checked-count').textContent = data.checkedInCount;
            document.getElementById('mf-pending-count').textContent = data.soldTicketCount - data.checkedInCount;

            renderManifestTable(data.passengers, tripId);
        } catch (e) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-danger">Lỗi khi tải dữ liệu manifest.</td></tr>';
        }
    };

    window.closeManifestModal = () => {
        document.getElementById('manifest-modal').style.display = 'none';
    };

    function renderManifestTable(passengers, tripId) {
        const tbody = document.getElementById('manifest-tbody');
        if (!passengers || passengers.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" style="text-align:center; padding: 20px;">Chưa có vé nào được bán cho chuyến này.</td></tr>';
            return;
        }

        tbody.innerHTML = passengers.map(p => {
            const isChecked = p.status === 'CheckedIn';
            return `<tr>
                <td><strong style="color: #c81d4a; font-size: 14px;">${p.seatCode}</strong></td>
                <td><strong>${p.passengerName}</strong></td>
                <td>${p.passengerPhone}</td>
                <td>${Number(p.price).toLocaleString('vi-VN')} ₫</td>
                <td>
                    ${isChecked
                        ? `<span class="checkedin-stamp">✓ Đã soát vé (${p.checkedInAtUtc ? new Date(p.checkedInAtUtc).toLocaleTimeString('vi-VN') : ''})</span>`
                        : '<span style="color:#71697f;">Chưa check-in</span>'}
                </td>
                <td style="text-align:right;">
                    ${!isChecked
                        ? `<button type="button" class="btn-manual-checkin" onclick="manualCheckInSeat('${tripId}', '${p.seatCode}')">Soát thủ công</button>`
                        : '<span style="color:#10b981; font-weight:700;">Đã lên xe</span>'}
                </td>
            </tr>`;
        }).join('');
    }

    window.filterManifestList = () => {
        const q = document.getElementById('manifest-search').value.toLowerCase().trim();
        if (!window.currentManifestData) return;
        const filtered = window.currentManifestData.passengers.filter(p =>
            p.seatCode.toLowerCase().includes(q) ||
            p.passengerName.toLowerCase().includes(q) ||
            p.passengerPhone.includes(q)
        );
        renderManifestTable(filtered, window.currentManifestData.tripId);
    };

    window.manualCheckInSeat = async (tripId, seatCode) => {
        if (!confirm(`Xác nhận soát vé thủ công cho ghế ${seatCode} (Hành khách đã xuất trình giấy tờ)?`)) return;
        try {
            const res = await fetch(`/api/operations/trips/${tripId}/manual-checkin`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ seatCode })
            });
            const data = await res.json();
            if (data.accepted) {
                alert(`Soát vé thành công cho ghế ${seatCode}!`);
                playAudioTone('success');
                // Refresh manifest
                const mfRes = await fetch(`/api/operations/trips/${tripId}/manifest`);
                const mfData = await mfRes.json();
                window.currentManifestData = mfData;
                document.getElementById('mf-checked-count').textContent = mfData.checkedInCount;
                document.getElementById('mf-pending-count').textContent = mfData.soldTicketCount - mfData.checkedInCount;
                renderManifestTable(mfData.passengers, tripId);
            } else {
                alert(`Không thể soát vé: ${data.result}`);
                playAudioTone('alarm');
            }
        } catch (e) {
            alert('Lỗi hệ thống khi soát vé thủ công.');
        }
    };

    // DISPATCH MODAL
    window.openDispatchModal = (tripId, route, plate, driver, time, count, cap) => {
        document.getElementById('dp-trip-id').value = tripId;
        document.getElementById('dp-trip-info').textContent = `Chuyến: ${route} · Xe: ${plate} · Lái xe: ${driver} · Giờ chạy: ${time}`;
        document.getElementById('dp-capacity-note').textContent = `Đã bán ${count} vé / Tối đa ${cap} chỗ`;

        document.getElementById('chk-vehicle').checked = false;
        document.getElementById('chk-license').checked = false;
        document.getElementById('chk-health').checked = false;
        document.getElementById('chk-capacity').checked = false;
        document.getElementById('dp-notes').value = '';
        validateDispatchForm();

        document.getElementById('dispatch-modal').style.display = 'grid';
    };

    window.closeDispatchModal = () => {
        document.getElementById('dispatch-modal').style.display = 'none';
    };

    window.validateDispatchForm = () => {
        const v = document.getElementById('chk-vehicle').checked;
        const l = document.getElementById('chk-license').checked;
        const h = document.getElementById('chk-health').checked;
        const c = document.getElementById('chk-capacity').checked;
        document.getElementById('btn-submit-dispatch').disabled = !(v && l && h && c);
    };

    document.getElementById('dispatch-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const tripId = document.getElementById('dp-trip-id').value;
        const v = document.getElementById('chk-vehicle').checked;
        const l = document.getElementById('chk-license').checked;
        const h = document.getElementById('chk-health').checked;
        const c = document.getElementById('chk-capacity').checked;
        const notes = document.getElementById('dp-notes').value;

        try {
            const res = await fetch('/api/operations/dispatch', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({
                    tripId,
                    vehicleDocumentsValid: v,
                    driverLicenseValid: l,
                    driverFitForDuty: h,
                    passengerCapacityValid: c,
                    notes
                })
            });
            const data = await res.json();
            if (res.ok && data.approved) {
                alert('Lệnh xuất bến đã được ký số điện tử thành công! Chuyến xe được phép xuất bến.');
                location.reload();
            } else if (res.ok) {
                alert(data.message || 'Không đạt đủ 4 tiêu chí an toàn; chưa thể cấp lệnh xuất bến.');
            } else {
                alert(data.message || data.detail || 'Không thể cấp lệnh xuất bến: kiểm tra lại quyền hạn tại bến đi của chuyến.');
            }
        } catch (err) {
            alert('Lỗi hệ thống khi gửi lệnh xuất bến.');
        }
    });

    // PRINT DISPATCH ORDER
    window.printDispatchOrder = (tripId, dep, arr, op, plate, driver, time, count, sig, sigTime) => {
        document.getElementById('p-sign-date').textContent = `Ngày cấp lệnh: ${new Date().toLocaleDateString('vi-VN')}`;
        document.getElementById('p-operator').textContent = op;
        document.getElementById('p-plate').textContent = plate;
        document.getElementById('p-route').textContent = `${dep} → ${arr}`;
        document.getElementById('p-driver').textContent = driver;
        document.getElementById('p-depart').textContent = time;
        document.getElementById('p-passengers').textContent = `${count} hành khách`;
        document.getElementById('p-sig-hash').textContent = `HMAC-SHA256: ${sig ? sig.substring(0, 24) : 'VALID'}`;
        document.getElementById('p-sig-time').textContent = sigTime ? `KÝ LÚC: ${sigTime}` : 'SMT SECURE CLEARANCE';

        window.print();
    };

})();
