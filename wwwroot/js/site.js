// ==========================================================================
// SMART TICKET (SMT) - THEME & BILINGUAL ENGINE
// Phong cách: Warm Amber Gold & Metallic Bronze (Theo Portfolio Dang Anh Quan)
// ==========================================================================

const SMT_I18N = {
    vi: {
        // Navigation & Brand
        brand_slogan: "SMT · VẠN DẶM BÌNH AN",
        brand_operator_slogan: "SMT · ĐỐI TÁC NHÀ XE",
        brand_station_slogan: "SMT · VẬN HÀNH BẾN",
        nav_home: "Tìm chuyến",
        nav_overview: "Tổng quan",
        nav_seat_templates: "Sơ đồ xe",
        nav_vehicles: "Phương tiện",
        nav_drivers: "Tài xế",
        nav_trips: "Chuyến xe",
        nav_station: "Cổng bến",
        nav_finance: "Đối soát",
        nav_gate: "Soát vé & xuất bến",
        nav_stations_admin: "Bến xe",
        nav_routes_admin: "Tuyến đường",
        nav_operators_admin: "Nhà xe",
        nav_login: "Đăng nhập",
        nav_logout: "Đăng xuất",
        footer_copy: "© 2026 BusGo SmartTicket",
        footer_slogan: "Vạn dặm bình an, chọn chỗ dễ dàng.",

        // Home Page
        hero_eyebrow: "VẠN DẶM BÌNH AN · CHỌN CHỖ DỄ DÀNG",
        hero_title: "Một chuyến xe tốt<br />bắt đầu từ <em>một lựa chọn dễ.</em>",
        hero_desc: "Nền tảng đặt vé xe khách số hóa thông minh: So sánh giá vé minh bạch của hàng chục nhà xe, chọn vị trí giường nằm 2 tầng trực quan và thanh toán VietQR Pro chuẩn xác.",
        hero_point_1: "<b>01</b> So sánh đa nhà xe",
        hero_point_2: "<b>02</b> Khóa ghế 7 phút Realtime",
        hero_point_3: "<b>03</b> Vé điện tử chuẩn xuất bến",
        ticket_kicker: "SMT XÁC THỰC",
        ticket_ready: "Đi thôi.",
        ticket_meta: "100% Chuyến chính hãng",
        label_start: "BẾN ĐI",
        label_end: "BẾN ĐẾN",
        search_eyebrow: "TÌM KIẾM HÀNH TRÌNH",
        search_heading: "Bạn muốn xuất phát từ đâu?",
        search_step: "Lựa chọn chuyến xe <b>01 / 03</b>",
        popular_routes: "Tuyến phổ biến:",
        dep_station_label: "ĐIỂM KHỞI HÀNH (BẾN ĐI)",
        dep_station_placeholder: "-- Chọn bến đi --",
        arr_station_label: "ĐIỂM ĐẾN (BẾN ĐẾN)",
        arr_station_placeholder: "-- Chọn bến đến --",
        travel_date_label: "NGÀY KHỞI HÀNH",
        search_btn: "Tìm chuyến",
        results_eyebrow: "KẾT QUẢ TÌM KIẾM",
        results_title: "Chuyến xe khởi hành",
        dep_time_label: "Khởi hành",
        arr_time_label: "Dự kiến đến",
        fare_label: "Giá vé niêm yết",
        per_seat: "₫ / ghế",
        seats_available: "chỗ trống",
        btn_pick_seats: "Chọn ghế",
        no_trips_title: "Không tìm thấy chuyến xe nào phù hợp trong ngày đã chọn.",
        no_trips_sub: "Vui lòng chọn ngày khác hoặc đổi lộ trình tìm kiếm.",
        trust_item_1_title: "Minh bạch giá & Tuyến đường",
        trust_item_1_desc: "Công khai giá niêm yết từ nhà xe chính hãng, không phụ phí ẩn.",
        trust_item_2_title: "Sơ đồ 2 tầng & Giữ chỗ 7p",
        trust_item_2_desc: "Chủ động chọn giường nằm, khóa chỗ realtime chống trùng lặp.",
        trust_item_3_title: "VietQR Pro & Vé tức thì",
        trust_item_3_desc: "Quét mã 1-chạm khóa số tiền, xuất vé điện tử check-in chuẩn sau 3 giây.",
        trust_assurance_title: "Cam kết Vận tải An toàn theo Nghị định 10/2020/NĐ-CP",
        trust_assurance_desc: "100% chuyến xe trên Smart Ticket xuất phát từ các Bến xe khách liên tỉnh hợp pháp, có Lệnh xuất bến điện tử.",

        // iPhone Notification Center Showcase
        ios_center_title: "Trung tâm thông báo",
        ios_media_track: "SMT Express · Limousine 29B",
        ios_media_route: "Sài Gòn ⇄ Đà Lạt · 08:00",
        ios_app_name: "SMART TICKET · VÉ ĐIỆN TỬ",
        ios_timestamp: "bây giờ",
        ios_card_headline: "QUÀ TẶNG HÀNH TRÌNH KHÔNG LO VỀ GIÁ",
        ios_bullet_1: "🧡 Tặng mã SMT50K – Giảm ngay vé chuyến đầu tiên",
        ios_bullet_2: "💛 Tự do chọn giường nằm 2 tầng trực quan, giữ 7 phút",
        ios_bullet_3: "💚 VietQR Pro thanh toán 1-chạm xác nhận vé tức thì",
        ios_bullet_4: "👉 Đặt vé ngay – Vạn dặm bình an cùng Smart Ticket!",
        ios_cleared_msg: "Đã xóa các thông báo",
        ios_restore_btn: "Khôi phục",

        // Seats Page
        step1_eyebrow: "BƯỚC 1 · CHỌN VỊ TRÍ GHẾ",
        price_label: "Giá vé niêm yết công khai",
        lock_note: "⏱️ Giữ chỗ 7 phút khi xác nhận",
        legend_available: "Ghế trống",
        legend_selected: "Đang chọn",
        legend_holding: "Đang giữ (7p)",
        legend_booked: "Đã bán",
        deck_1_name: "TẦNG DƯỚI (Dãy A)",
        deck_2_name: "TẦNG TRÊN (Dãy B)",
        steering_text: "Vô lăng",
        panoramic_view: "Góc nhìn thoáng",
        passenger_info_title: "Thông tin hành khách",
        passenger_info_sub: "Vui lòng điền đúng số điện thoại để nhận mã vé và liên hệ khi lên xe",
        label_passenger_name: "Họ và tên hành khách",
        placeholder_passenger_name: "Ví dụ: Nguyễn Văn A",
        label_passenger_phone: "Số điện thoại liên hệ",
        placeholder_passenger_phone: "Ví dụ: 0912 345 678",
        label_passenger_email: "Email (Nhận vé điện tử)",
        placeholder_passenger_email: "ten@email.com (không bắt buộc)",
        sum_selected_seats: "Ghế đã chọn:",
        sum_no_seats: "Chưa chọn ghế",
        sum_ticket_count: "Số lượng vé:",
        sum_total_price: "Tổng tiền:",
        btn_hold_submit: "Tiến hành giữ chỗ & Thanh toán QR",
        btn_holding_state: "Đang tạo mã giữ chỗ & sinh mã VietQR...",

        // Modal VietQR
        modal_brand_tag: "⚡ VietQR · Napas 247 · VietinBank",
        modal_title: "Quét Mã VietQR Nhận Vé Ngay",
        modal_subtitle: "Mở ứng dụng Ngân hàng bất kỳ hoặc MoMo/VNPay để quét mã thanh toán 1-chạm",
        modal_lock_tag: "Số tiền đã khóa cứng (EMVCo Tag 54)",
        modal_bank_label: "Ngân hàng thụ hưởng:",
        modal_account_num_label: "Số tài khoản:",
        modal_account_name_label: "Chủ tài khoản:",
        modal_amount_label: "Số tiền:",
        modal_ref_label: "Nội dung CK:",
        btn_copy: "Sao chép",
        btn_copied: "✓ Đã chép",
        modal_waiting_title: "Đang chờ bạn quét mã thanh toán...",
        modal_waiting_desc: "Hệ thống tự động kích hoạt vé ngay khi giao dịch thành công.",
        btn_open_checkout: "Mở trang thanh toán riêng biệt ↗",
        modal_expired_title: "Hết hạn giữ chỗ 7 phút!",
        modal_expired_desc: "Ghế đã được giải phóng. Vui lòng đóng cửa sổ và chọn lại ghế.",
        payment_success_title: "✓ Thanh toán thành công!",
        payment_success_desc: "Ngân hàng đã xác nhận thanh toán. Đang chuyển tới vé điện tử...",

        // Checkout Page
        checkout_eyebrow: "BƯỚC 2 · THANH TOÁN TỰ ĐỘNG",
        checkout_title: "Quét Mã VietQR Để Nhận Vé",
        checkout_subtitle: "Mở ứng dụng Ngân hàng bất kỳ hoặc MoMo/VNPay để quét mã thanh toán 1-chạm",
        hold_temp: "Giữ chỗ tạm thời:",
        btn_bank_app: "Mở App Ngân Hàng (DeepLink)",
        btn_save_qr: "Lưu mã QR về máy",
        qr_help_text: "Giữ nguyên số tiền và nội dung chuyển khoản. Vé được phát hành sau khi ngân hàng xác nhận thanh toán.",

        // Tickets Page
        ticket_paid_badge: "✓ ĐÃ THANH TOÁN THÀNH CÔNG",
        ticket_page_title: "Vé Xe Khách Điện Tử (E-Ticket)",
        ticket_code_label: "Mã đặt chỗ:",
        ticket_count_label: "Số lượng:",
        btn_print_pdf: "In vé / Lưu PDF",
        btn_book_more: "Đặt thêm chuyến khác →",
        ticket_route_label: "LỘ TRÌNH CHUYẾN ĐI",
        ticket_passenger_label: "HÀNH KHÁCH",
        ticket_dep_label: "GIỜ KHỞI HÀNH",
        ticket_seat_label: "SỐ GHẾ",
        ticket_price_label: "GIÁ VÉ NIÊM YẾT",
        ticket_vat_note: "(Đã gồm thuế & bảo hiểm)",
        ticket_legal_note: "🛡️ Vé điện tử có giá trị tương đương vé giấy truyền thống theo Thông tư 12/2020/TT-BGTVT.",

        // Login Page
        login_brand_eyebrow: "BusGo SmartTicket",
        login_brand_heading: "Vạn dặm bình an,<br /><em>chọn chỗ dễ dàng.</em>",
        login_brand_copy: "Một hành trình thuận tiện bắt đầu từ việc chọn đúng chuyến, đúng chỗ và đúng thời điểm.",
        login_brand_tagline: "ĐẶT VÉ · CHỌN GHẾ · LÊN ĐƯỜNG",
        login_card_eyebrow: "TÀI KHOẢN CỦA BẠN",
        login_card_title: "Chào mừng trở lại",
        login_card_subtitle: "Đăng nhập bằng tài khoản BusGo SmartTicket.",
        login_email_label: "Email",
        login_password_label: "Mật khẩu",
        login_password_placeholder: "Nhập mật khẩu",
        login_remember_me: "Ghi nhớ đăng nhập",
        login_submit_btn: "Đăng nhập hệ thống",
        login_new_user: "Bạn là hành khách mới?",
        login_create_acc: "Tạo tài khoản",
        login_partner_q: "Bạn là nhà xe?",
        login_partner_reg: "Gửi hồ sơ đăng ký"
    },
    en: {
        // Navigation & Brand
        brand_slogan: "SMT · SAFE TRAVELS",
        brand_operator_slogan: "SMT · BUS PARTNER",
        brand_station_slogan: "SMT · STATION GATE",
        nav_home: "Search Trips",
        nav_overview: "Overview",
        nav_seat_templates: "Seat Maps",
        nav_vehicles: "Vehicles",
        nav_drivers: "Drivers",
        nav_trips: "Trips",
        nav_station: "Station Portal",
        nav_finance: "Finance",
        nav_gate: "Boarding & Gate",
        nav_stations_admin: "Stations",
        nav_routes_admin: "Routes",
        nav_operators_admin: "Operators",
        nav_login: "Sign In",
        nav_logout: "Sign Out",
        footer_copy: "© 2026 BusGo SmartTicket",
        footer_slogan: "Safe travels, effortless booking.",

        // Home Page
        hero_eyebrow: "SAFE TRAVELS · EFFORTLESS BOOKING",
        hero_title: "A great journey begins<br />with an <em>effortless choice.</em>",
        hero_desc: "Smart digital coach ticketing platform: Compare transparent fares across top operators, pick your interactive 2-deck berth, and pay instantly via VietQR Pro.",
        hero_point_1: "<b>01</b> Multi-operator fares",
        hero_point_2: "<b>02</b> 7-minute realtime seat hold",
        hero_point_3: "<b>03</b> Digital boarding pass",
        ticket_kicker: "SMT VERIFIED",
        ticket_ready: "Let's Go.",
        ticket_meta: "100% Verified Trips",
        label_start: "DEPARTURE",
        label_end: "DESTINATION",
        search_eyebrow: "SEARCH YOUR JOURNEY",
        search_heading: "Where would you like to travel?",
        search_step: "Select your trip <b>01 / 03</b>",
        popular_routes: "Popular routes:",
        dep_station_label: "DEPARTURE STATION",
        dep_station_placeholder: "-- Select departure station --",
        arr_station_label: "DESTINATION STATION",
        arr_station_placeholder: "-- Select arrival station --",
        travel_date_label: "TRAVEL DATE",
        search_btn: "Search Trips",
        results_eyebrow: "SEARCH RESULTS",
        results_title: "Available Departures",
        dep_time_label: "Departure",
        arr_time_label: "Est. Arrival",
        fare_label: "Official Fare",
        per_seat: "VND / seat",
        seats_available: "seats left",
        btn_pick_seats: "Select Seats",
        no_trips_title: "No matching trips found for the selected date.",
        no_trips_sub: "Please select another date or change your stations.",
        trust_item_1_title: "Transparent Fares & Routes",
        trust_item_1_desc: "Official published fares directly from certified operators, zero hidden fees.",
        trust_item_2_title: "2-Deck Map & 7-Min Hold",
        trust_item_2_desc: "Choose your berth visually, secured by realtime lock preventing duplicate bookings.",
        trust_item_3_title: "VietQR Pro & Instant Pass",
        trust_item_3_desc: "1-tap QR payment with locked amount; instant boarding pass issued in 3 seconds.",
        trust_assurance_title: "Certified Transport Safety (Decree 10/2020/ND-CP)",
        trust_assurance_desc: "100% coach departures from legally licensed intercity stations with digital dispatch orders.",

        // iPhone Notification Center Showcase
        ios_center_title: "Notification Center",
        ios_media_track: "SMT Express · Limousine 29B",
        ios_media_route: "Saigon ⇄ Da Lat · 08:00 AM",
        ios_app_name: "SMART TICKET · E-TICKET",
        ios_timestamp: "now",
        ios_card_headline: "JOURNEY GIFT · ZERO WORRY TRAVEL",
        ios_bullet_1: "🧡 50K SMT Voucher – Instant discount on first trip",
        ios_bullet_2: "💛 Choose 2-deck berth visually, 7-min realtime hold",
        ios_bullet_3: "💚 1-tap VietQR Pro payment, instant ticket issuance",
        ios_bullet_4: "👉 Book coach tickets now on Smart Ticket!",
        ios_cleared_msg: "Notifications cleared",
        ios_restore_btn: "Restore",

        // Seats Page
        step1_eyebrow: "STEP 1 · SELECT SEATS",
        price_label: "Official Public Fare",
        lock_note: "⏱️ 7-minute temporary seat hold",
        legend_available: "Available",
        legend_selected: "Selected",
        legend_holding: "Holding (7m)",
        legend_booked: "Booked",
        deck_1_name: "LOWER DECK (Deck A)",
        deck_2_name: "UPPER DECK (Deck B)",
        steering_text: "Steering",
        panoramic_view: "Panoramic View",
        passenger_info_title: "Passenger Information",
        passenger_info_sub: "Please provide an accurate phone number to receive your ticket and for boarding contact",
        label_passenger_name: "Passenger Full Name",
        placeholder_passenger_name: "e.g. John Doe",
        label_passenger_phone: "Contact Phone Number",
        placeholder_passenger_phone: "e.g. 0912 345 678",
        label_passenger_email: "Email (E-Ticket Delivery)",
        placeholder_passenger_email: "name@email.com (optional)",
        sum_selected_seats: "Selected Seats:",
        sum_no_seats: "No seats selected",
        sum_ticket_count: "Ticket Count:",
        sum_total_price: "Total Fare:",
        btn_hold_submit: "Proceed to Hold & VietQR Pay",
        btn_holding_state: "Holding seats & creating VietQR...",

        // Modal VietQR
        modal_brand_tag: "⚡ VietQR · Napas 247 · VietinBank",
        modal_title: "Scan VietQR Code To Pay",
        modal_subtitle: "Open any Banking App or MoMo/VNPay for 1-tap instant payment",
        modal_lock_tag: "Exact amount auto-locked (EMVCo Tag 54)",
        modal_bank_label: "Beneficiary Bank:",
        modal_account_num_label: "Account Number:",
        modal_account_name_label: "Account Name:",
        modal_amount_label: "Total Amount:",
        modal_ref_label: "Transfer Note:",
        btn_copy: "Copy",
        btn_copied: "✓ Copied",
        modal_waiting_title: "Waiting for your payment scan...",
        modal_waiting_desc: "System automatically issues your ticket upon confirmation.",
        btn_open_checkout: "Open Full Checkout Page ↗",
        modal_expired_title: "7-Minute Seat Hold Expired!",
        modal_expired_desc: "Seat hold has been released. Please close and pick seats again.",
        payment_success_title: "✓ Payment Succeeded!",
        payment_success_desc: "Payment confirmed by bank. Redirecting to your e-ticket...",

        // Checkout Page
        checkout_eyebrow: "STEP 2 · AUTOMATED PAYMENT",
        checkout_title: "Scan VietQR Code For Ticket",
        checkout_subtitle: "Open any Banking App or MoMo/VNPay for 1-tap payment",
        hold_temp: "Temporary hold:",
        btn_bank_app: "Open Bank App (DeepLink)",
        btn_save_qr: "Download QR Code",
        qr_help_text: "Keep the transfer amount and reference unchanged. Tickets are issued after bank confirmation.",

        // Tickets Page
        ticket_paid_badge: "✓ PAYMENT CONFIRMED",
        ticket_page_title: "Electronic Bus Ticket (E-Ticket)",
        ticket_code_label: "Booking Code:",
        ticket_count_label: "Quantity:",
        btn_print_pdf: "Print / Save PDF",
        btn_book_more: "Book Another Trip →",
        ticket_route_label: "BUS ROUTE",
        ticket_passenger_label: "PASSENGER",
        ticket_dep_label: "DEPARTURE TIME",
        ticket_seat_label: "SEAT NUMBER",
        ticket_price_label: "OFFICIAL FARE",
        ticket_vat_note: "(VAT & Insurance included)",
        ticket_legal_note: "🛡️ This e-ticket holds official legal boarding validity under Circular 12/2020/TT-BGTVT.",

        // Login Page
        login_brand_eyebrow: "BusGo SmartTicket",
        login_brand_heading: "Safe travels,<br /><em>effortless booking.</em>",
        login_brand_copy: "A smooth trip starts with picking the right journey, seat, and schedule.",
        login_brand_tagline: "BOOK · CHOOSE SEAT · HIT THE ROAD",
        login_card_eyebrow: "ADMINISTRATION PORTAL",
        login_card_title: "Welcome Back",
        login_card_subtitle: "Sign in with your BusGo SmartTicket account.",
        login_email_label: "Email address",
        login_password_label: "Password",
        login_password_placeholder: "Enter your password",
        login_remember_me: "Remember me",
        login_submit_btn: "Sign In to Portal",
        login_new_user: "New passenger?",
        login_create_acc: "Create Account",
        login_partner_q: "Bus Operator?",
        login_partner_reg: "Submit Partner Application"
    }
};

// GLOBAL STATE MANAGERS
window.SMT = {
    currentLang: 'vi',
    currentTheme: 'light',

    setLanguage: function(lang, notify = false) {
        if (!SMT_I18N[lang]) lang = 'vi';
        this.currentLang = lang;
        document.documentElement.setAttribute('data-lang', lang);
        document.documentElement.lang = lang;
        try { localStorage.setItem('smartticket_lang', lang); } catch (e) {}

        // Update active switch button
        document.querySelectorAll('.lang-option').forEach(btn => {
            const selected = btn.getAttribute('data-lang') === lang;
            btn.classList.toggle('active', selected);
            btn.setAttribute('aria-pressed', String(selected));
        });

        // Translate text contents
        const dict = SMT_I18N[lang];
        document.querySelectorAll('[data-i18n]').forEach(el => {
            const key = el.getAttribute('data-i18n');
            if (dict[key] !== undefined) {
                if (dict[key].includes('<') || el.hasAttribute('data-i18n-html')) {
                    el.innerHTML = dict[key];
                } else {
                    el.textContent = dict[key];
                }
            }
        });

        // Translate placeholders
        document.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
            const key = el.getAttribute('data-i18n-placeholder');
            if (dict[key] !== undefined) {
                el.placeholder = dict[key];
            }
        });

        // Translate titles
        document.querySelectorAll('[data-i18n-title]').forEach(el => {
            const key = el.getAttribute('data-i18n-title');
            if (dict[key] !== undefined) {
                el.title = dict[key];
            }
        });

        window.SMTLocale?.apply(lang);
        this.updateThemeLabel();

        // Notify custom subscribers
        window.dispatchEvent(new CustomEvent('smt:languagechange', { detail: { lang: lang } }));
    },

    setTheme: function(theme) {
        if (theme !== 'light' && theme !== 'dark') theme = 'dark';
        this.currentTheme = theme;
        document.documentElement.setAttribute('data-theme', theme);
        try { localStorage.setItem('smartticket_theme', theme); } catch (e) {}

        document.querySelector('meta[name="theme-color"]')?.setAttribute('content', theme === 'dark' ? '#10211b' : '#f4f8f5');
        this.updateThemeLabel();

        window.dispatchEvent(new CustomEvent('smt:themechange', { detail: { theme: theme } }));
    },

    updateThemeLabel: function() {
        const btn = document.getElementById('btn-theme-toggle');
        const label = this.currentTheme === 'dark' ? 'Chuyển sang giao diện sáng' : 'Chuyển sang giao diện tối';
        const translated = window.SMTLocale?.text(label) || label;
        btn?.setAttribute('title', translated);
        btn?.setAttribute('aria-label', translated);
        btn?.setAttribute('aria-pressed', String(this.currentTheme === 'dark'));
    },

    toggleTheme: function(event) {
        const nextTheme = (this.pendingTheme || this.currentTheme) === 'dark' ? 'light' : 'dark';
        this.pendingTheme = nextTheme;
        window.SMTAppearance.transition(() => { this.setTheme(nextTheme); }, event);
    },

    text: function(value) { return window.SMTLocale?.text(value) || value; },

    isStaticTranslation: function(key, content) {
        const normalize = value => value.replace(/\s+/g, ' ').trim();
        return ['vi','en'].some(lang => {
            const value = SMT_I18N[lang][key];
            if (value === undefined) return false;
            const holder = document.createElement('span');
            holder.innerHTML = value;
            return normalize(holder.textContent) === normalize(content);
        });
    },

    t: function(key) {
        const dict = SMT_I18N[this.currentLang] || SMT_I18N.vi;
        return dict[key] !== undefined ? dict[key] : key;
    }
};

// INITIALIZE ON DOM READY
document.addEventListener('DOMContentLoaded', function() {
    // 1. Theme initialization
    let savedTheme = 'light';
    try { savedTheme = localStorage.getItem('smartticket_theme') || 'light'; } catch (e) {}
    SMT.setTheme(savedTheme);

    const themeToggleBtn = document.getElementById('btn-theme-toggle');
    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', function(event) {
            SMT.toggleTheme(event);
        });
    }

    // 2. Language initialization
    let savedLang = 'vi';
    try { savedLang = localStorage.getItem('smartticket_lang') || 'vi'; } catch (e) {}
    SMT.setLanguage(savedLang);

    document.querySelectorAll('.lang-option').forEach(btn => {
        btn.addEventListener('click', function() {
            const lang = this.getAttribute('data-lang');
            SMT.setLanguage(lang);
        });
    });
});

document.addEventListener('DOMContentLoaded', () => {
    const path = location.pathname.toLowerCase().replace(/\/$/, '') || '/';
    document.querySelectorAll('.main-nav a').forEach(link => {
        const target = new URL(link.href, location.origin).pathname.toLowerCase().replace(/\/$/, '') || '/';
        if (target === path) { link.classList.add('active'); link.setAttribute('aria-current', 'page'); }
    });
});
