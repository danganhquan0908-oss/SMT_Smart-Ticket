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
        nav_my_tickets: "🎟️ Vé của tôi",
        nav_my_profile: "👤 Hồ sơ",
        nav_profile_tooltip: "Xem hồ sơ cá nhân",
        nav_login: "Đăng nhập",
        nav_logout: "Đăng xuất",
        footer_copy: "© 2026 Smart Ticket - SMT",
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
        btn_dev_simulate: "⚡ Giả lập thanh toán thành công (Môi trường Dev)",
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
        qr_help_text: "Không sửa đổi số tiền hoặc nội dung để vé được xuất tự động sau 3 giây.",

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
        login_brand_eyebrow: "SMART TICKET - SMT",
        login_brand_heading: "Vạn dặm bình an,<br /><em>chọn chỗ dễ dàng.</em>",
        login_brand_copy: "Một hành trình thuận tiện bắt đầu từ việc chọn đúng chuyến, đúng chỗ và đúng thời điểm.",
        login_brand_tagline: "ĐẶT VÉ · CHỌN GHẾ · LÊN ĐƯỜNG",
        login_card_eyebrow: "KHU VỰC QUẢN TRỊ",
        login_card_title: "Chào mừng trở lại",
        login_card_subtitle: "Đăng nhập bằng tài khoản Smart Ticket - SMT.",
        login_email_label: "Email",
        login_password_label: "Mật khẩu",
        login_password_placeholder: "Nhập mật khẩu",
        login_remember_me: "Ghi nhớ đăng nhập",
        login_submit_btn: "Đăng nhập hệ thống",
        login_new_user: "Bạn là hành khách mới?",
        login_create_acc: "Tạo tài khoản",
        login_partner_q: "Bạn là nhà xe?",
        login_partner_reg: "Gửi hồ sơ đăng ký",

        // Customer Tickets Page (Tickets.cshtml)
        tickets_hero_eyebrow: "TRUNG TÂM TÀI KHOẢN HÀNH KHÁCH",
        tickets_hero_title: "Vé xe của tôi",
        tickets_hero_subtitle: "Toàn bộ vé xe điện tử và lịch sử đặt chỗ của bạn được lưu trữ an toàn tại đây.",
        tickets_btn_edit_profile: "👤 Cập nhật hồ sơ",
        tickets_btn_find_trip: "🔍 Tìm chuyến xe mới →",
        tickets_tab_upcoming: "🚌 Sắp khởi hành",
        tickets_tab_pending: "⏱️ Chờ thanh toán",
        tickets_tab_history: "📜 Lịch sử chuyến đi",
        tickets_empty_upcoming_title: "Bạn chưa có vé xe nào sắp khởi hành",
        tickets_empty_upcoming_desc: "Hãy đặt chuyến xe mới để trải nghiệm dịch vụ xe khách chất lượng cao liên tỉnh.",
        tickets_empty_btn: "Đặt vé ngay bây giờ",
        tickets_empty_pending_title: "Không có đơn đặt chỗ nào đang chờ thanh toán",
        tickets_empty_pending_desc: "Tất cả các đơn đặt vé của bạn đều đã được hoàn tất hoặc giải phóng ghế an toàn.",
        tickets_empty_history_title: "Chưa có lịch sử chuyến đi cũ",
        tickets_empty_history_desc: "Các chuyến xe đã hoàn thành hoặc hết hạn sẽ được lưu trữ tại đây.",
        badge_paid_confirmed: "✓ ĐÃ THANH TOÁN",
        badge_calculating: "⏱️ Đang tính giờ...",
        badge_completed: "✓ ĐÃ HOÀN THÀNH",
        badge_expired: "✕ ĐÃ HẾT HẠN",
        badge_overdue_sold: "⚠️ THANH TOÁN TRỄ (GHẾ ĐÃ BÁN)",
        label_booking_code: "MÃ:",
        label_departure_time: "Khởi hành:",
        label_bus_operator: "Nhà xe:",
        label_booked_seats: "Ghế đã đặt:",
        label_held_seats: "Ghế giữ chỗ:",
        label_history_seats: "Ghế:",
        label_total_paid: "Tổng tiền:",
        label_total_due: "Tổng tiền cần CK:",
        label_transfer_note: "Nội dung:",
        btn_copy_inline: "Sao chép",
        btn_copied_inline: "✓ Đã chép",
        btn_resume_pay: "⚡ Tiếp tục thanh toán VietQR",
        btn_view_eticket_qr: "📱 Xem vé & Mã QR lên xe",
        btn_review_eticket: "Xem lại vé điện tử & In PDF",
        overdue_alert_title: "⚠️ Giao dịch chuyển tiền sau khi hết hạn giữ chỗ:",
        overdue_alert_desc: "Hệ thống đã nhận thanh toán nhưng ghế đã bị bán cho khách khác sau khi hết hạn 7 phút. Vui lòng liên hệ hotline tổng đài <strong>1900 6868</strong> để được xếp chuyến tương đương hoặc hoàn tiền 100%.",
        countdown_hold_remaining: "⏱️ GIỮ CHỖ CÒN:",
        countdown_hold_expired: "✕ ĐÃ HẾT HẠN GIỮ CHỖ",
        btn_hold_expired: "Đã hết hạn giữ chỗ",
        modal_resume_title: "Quét Mã VietQR Tiếp Tục Thanh Toán",
        modal_resume_subtitle: "Mở ứng dụng Ngân hàng bất kỳ hoặc MoMo/VNPay để quét mã thanh toán 1-chạm",
        modal_expired_title_short: "Hết hạn giữ chỗ 7 phút",
        modal_expired_desc_short: "Thời gian giữ chỗ đã hết, ghế đã được tự động giải phóng.",
        modal_bank_label: "Ngân hàng thụ hưởng:",
        modal_acc_label: "Số tài khoản:",
        modal_name_label: "Chủ tài khoản:",
        modal_amount_label: "Số tiền:",
        modal_ref_label: "Nội dung CK:",
        modal_status_wait_title: "Đang chờ bạn quét mã thanh toán...",
        modal_status_wait_desc: "Hệ thống tự động kích hoạt vé ngay khi giao dịch thành công.",
        modal_status_success_title: "✓ Thanh toán thành công!",
        modal_status_success_desc: "Hệ thống đã nhận thanh toán. Đang chuyển tới danh sách vé...",
        modal_btn_dev_simulate: "⚡ Giả lập thanh toán thành công (Môi trường Dev)",

        // Customer Profile Page (Profile.cshtml)
        profile_hero_eyebrow: "TRUNG TÂM TÀI KHOẢN HÀNH KHÁCH",
        profile_hero_title: "Hồ sơ cá nhân & Bảo mật",
        profile_hero_subtitle: "Quản lý thông tin liên hệ khi nhận vé điện tử và cập nhật mật khẩu bảo vệ tài khoản.",
        profile_btn_view_tickets: "🎟️ Xem vé của tôi →",
        profile_info_card_title: "Thông tin hành khách",
        profile_info_card_desc: "Tên và số điện thoại này sẽ tự động điền khi bạn đặt vé mới.",
        profile_label_fullname: "Họ và tên hành khách",
        profile_placeholder_fullname: "Ví dụ: Đặng Anh Quân",
        profile_label_phone: "Số điện thoại liên hệ",
        profile_placeholder_phone: "Ví dụ: 0345 588 495",
        profile_label_email: "Địa chỉ Email (Định danh tài khoản)",
        profile_email_locked: "🔒 Cố định",
        profile_email_hint: "Email dùng để nhận vé điện tử và đăng nhập, không thể thay đổi trực tiếp.",
        profile_btn_save_info: "Lưu thay đổi thông tin",
        profile_pwd_card_title: "Đổi mật khẩu bảo mật",
        profile_pwd_card_desc: "Mật khẩu cần tối thiểu 12 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.",
        profile_label_current_pwd: "Mật khẩu hiện tại",
        profile_placeholder_current_pwd: "Nhập mật khẩu đang dùng",
        profile_label_new_pwd: "Mật khẩu mới",
        profile_placeholder_new_pwd: "Tối thiểu 12 ký tự mạnh",
        profile_label_confirm_pwd: "Xác nhận mật khẩu mới",
        profile_placeholder_confirm_pwd: "Nhập lại mật khẩu mới",
        profile_btn_save_pwd: "Cập nhật mật khẩu mới",

        // Customer Registration Page (Register.cshtml)
        reg_brand_eyebrow: "SMART TICKET - SMT",
        reg_brand_heading: "Hành trình mới<br /><em>bắt đầu từ đây.</em>",
        reg_brand_copy: "Đăng ký tài khoản hành khách để bắt đầu sử dụng Smart Ticket.",
        reg_brand_tagline: "ĐẶT VÉ · CHỌN GHẾ · LÊN ĐƯỜNG",
        reg_card_eyebrow: "TÀI KHOẢN HÀNH KHÁCH",
        reg_card_title: "Tạo tài khoản",
        reg_card_subtitle: "Đăng ký miễn phí để bắt đầu đặt vé.",
        reg_label_fullname: "Họ và tên hành khách",
        reg_placeholder_fullname: "Ví dụ: Nguyễn Văn A",
        reg_label_email: "Email",
        reg_placeholder_email: "email@example.com",
        reg_label_phone: "Số điện thoại",
        reg_placeholder_phone: "09xx xxx xxx",
        reg_label_pwd: "Mật khẩu",
        reg_placeholder_pwd: "Ít nhất 12 ký tự",
        reg_label_confirm_pwd: "Xác nhận mật khẩu",
        reg_placeholder_confirm_pwd: "Nhập lại mật khẩu",
        reg_btn_submit: "Tạo tài khoản",
        reg_has_account: "Đã có tài khoản?",
        reg_link_login: "Đăng nhập",

        // Ticket Details Page (TicketDetails.cshtml)
        detail_verified_badge: "✓ ĐÃ XÁC NHẬN CHÍNH CHỦ",
        detail_page_title: "Vé Xe Khách Điện Tử (E-Ticket)",
        detail_code_label: "Mã đặt chỗ:",
        detail_count_label: "Số lượng:",
        detail_operator_label: "Nhà xe:",
        detail_tickets_suffix: "vé",
        detail_btn_back: "← Vé của tôi",
        detail_btn_print: "In vé / Lưu PDF",
        detail_btn_rebook: "Đặt thêm chuyến khác →",
        detail_empty_title: "Chưa có vé nào được phát hành",
        detail_empty_desc: "Đơn đặt chỗ này chưa hoàn tất thanh toán hoặc đang được hệ thống đối soát. Vui lòng kiểm tra lại sau ít phút.",
        detail_legal_sub: "VÉ ĐIỆN TỬ HỢP PHÁP",
        detail_ticket_num_label: "SỐ VÉ:",
        detail_route_label: "LỘ TRÌNH CHUYẾN ĐI",
        detail_dep_label: "GIỜ KHỞI HÀNH",
        detail_seat_label: "SỐ GHẾ",
        detail_fare_label: "GIÁ VÉ NIÊM YẾT",
        detail_vat_note: "(Đã gồm thuế & bảo hiểm)",
        detail_vehicle_type_label: "LOẠI XE",
        detail_stub_title: "CHECK-IN BẾN XE",
        detail_stub_valid: "Mã QR hợp lệ",
        detail_stub_hint: "Đưa mã QR cho nhân viên bến xe hoặc tài xế quét khi lên xe",
        detail_stub_no_qr: "Không có mã QR",
        detail_legal_footer: "🛡️ Vé điện tử có giá trị tương đương vé giấy truyền thống theo Thông tư 12/2020/TT-BGTVT.",

        // Shared / Filters / Search / Auth Gates / 2FA
        modal_close_title: "Đóng cửa sổ",
        login_demo_hint_title: "⚡ Tài khoản mẫu kiểm thử phân quyền đa nhà xe:",
        filter_all_vehicles: "Tất cả xe",
        filter_limousine: "Limousine",
        filter_sleeper: "Giường nằm",
        filter_seats: "Ghế ngồi",
        sort_label: "Sắp xếp:",
        sort_time_asc: "Giờ đi sớm nhất",
        sort_time_desc: "Giờ đi muộn nhất",
        sort_price_asc: "Giá vé thấp nhất",
        sort_price_desc: "Giá vé cao nhất",
        sort_seats_desc: "Nhiều chỗ trống nhất",
        fare_prefix: "Giá niêm yết",
        sold_out_btn: "Hết vé",
        swap_tooltip: "Hoán đổi bến đi và bến đến",
        auth_gate_title: "Cần đăng nhập để đặt vé",
        auth_gate_desc: "Vé điện tử sẽ tự động lưu vào Trung tâm tài khoản của bạn sau khi thanh toán.",
        auth_gate_login: "Đăng nhập",
        auth_gate_register: "Tạo tài khoản",
        auth_logged_in_prefix: "✓ Đã đăng nhập:",
        auth_saved_to: "Vé sẽ tự động lưu vào",
        twofa_eyebrow: "BẢO MẬT TÀI KHOẢN",
        twofa_title: "Nhập mã xác thực",
        twofa_subtitle: "Nhập mã 6 chữ số từ ứng dụng xác thực của bạn.",
        twofa_code_label: "Mã xác thực",
        twofa_submit_btn: "Xác minh"
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
        nav_my_tickets: "🎟️ My Tickets",
        nav_my_profile: "👤 Profile",
        nav_profile_tooltip: "View Personal Profile",
        nav_login: "Sign In",
        nav_logout: "Sign Out",
        footer_copy: "© 2026 Smart Ticket - SMT",
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
        btn_dev_simulate: "⚡ Simulate Dev Payment Success",
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
        qr_help_text: "Do not alter amount or transfer note to ensure auto ticket issuance in 3 seconds.",

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
        login_brand_eyebrow: "SMART TICKET - SMT",
        login_brand_heading: "Safe travels,<br /><em>effortless booking.</em>",
        login_brand_copy: "A smooth trip starts with picking the right journey, seat, and schedule.",
        login_brand_tagline: "BOOK · CHOOSE SEAT · HIT THE ROAD",
        login_card_eyebrow: "ADMINISTRATION PORTAL",
        login_card_title: "Welcome Back",
        login_card_subtitle: "Sign in with your Smart Ticket - SMT account.",
        login_email_label: "Email address",
        login_password_label: "Password",
        login_password_placeholder: "Enter your password",
        login_remember_me: "Remember me",
        login_submit_btn: "Sign In to Portal",
        login_new_user: "New passenger?",
        login_create_acc: "Create Account",
        login_partner_q: "Bus Operator?",
        login_partner_reg: "Submit Partner Application",

        // Customer Tickets Page (Tickets.cshtml)
        tickets_hero_eyebrow: "PASSENGER ACCOUNT PORTAL",
        tickets_hero_title: "My Bus Tickets",
        tickets_hero_subtitle: "All your electronic bus tickets and booking history are securely saved here.",
        tickets_btn_edit_profile: "👤 Update Profile",
        tickets_btn_find_trip: "🔍 Search New Trips →",
        tickets_tab_upcoming: "🚌 Upcoming Trips",
        tickets_tab_pending: "⏱️ Pending Payment",
        tickets_tab_history: "📜 Booking History",
        tickets_empty_upcoming_title: "You have no upcoming trips",
        tickets_empty_upcoming_desc: "Book a new journey to experience high-quality certified intercity coach travel.",
        tickets_empty_btn: "Book Tickets Now",
        tickets_empty_pending_title: "No bookings pending payment",
        tickets_empty_pending_desc: "All your bookings have either been completed or seats safely released.",
        tickets_empty_history_title: "No previous booking history",
        tickets_empty_history_desc: "Completed or expired journeys will be archived here.",
        badge_paid_confirmed: "✓ PAYMENT CONFIRMED",
        badge_calculating: "⏱️ Calculating time...",
        badge_completed: "✓ COMPLETED",
        badge_expired: "✕ EXPIRED",
        badge_overdue_sold: "⚠️ LATE PAYMENT (SEATS RECLAIMED)",
        label_booking_code: "CODE:",
        label_departure_time: "Departure:",
        label_bus_operator: "Operator:",
        label_booked_seats: "Booked Seats:",
        label_held_seats: "Held Seats:",
        label_history_seats: "Seats:",
        label_total_paid: "Total Fare:",
        label_total_due: "Amount Due:",
        label_transfer_note: "Note:",
        btn_copy_inline: "Copy",
        btn_copied_inline: "✓ Copied",
        btn_resume_pay: "⚡ Resume VietQR Payment",
        btn_view_eticket_qr: "📱 View Ticket & Boarding QR",
        btn_review_eticket: "Review E-Ticket & Print PDF",
        overdue_alert_title: "⚠️ Transfer received after seat hold expiration:",
        overdue_alert_desc: "System received payment but seats were reassigned after the 7-minute hold expired. Please contact hotline <strong>1900 6868</strong> for seat rebooking or a 100% refund.",
        countdown_hold_remaining: "⏱️ HOLD REMAINING:",
        countdown_hold_expired: "✕ SEAT HOLD EXPIRED",
        btn_hold_expired: "Hold expired",
        modal_resume_title: "Scan VietQR Code To Resume Payment",
        modal_resume_subtitle: "Open any Banking App or MoMo/VNPay for 1-tap instant payment",
        modal_expired_title_short: "7-Minute Seat Hold Expired",
        modal_expired_desc_short: "Seat hold has expired, seats were automatically released.",
        modal_bank_label: "Beneficiary Bank:",
        modal_acc_label: "Account Number:",
        modal_name_label: "Account Name:",
        modal_amount_label: "Total Amount:",
        modal_ref_label: "Transfer Note:",
        modal_status_wait_title: "Waiting for your payment scan...",
        modal_status_wait_desc: "System automatically issues your ticket once payment is confirmed.",
        modal_status_success_title: "✓ Payment Succeeded!",
        modal_status_success_desc: "Payment confirmed. Redirecting to your tickets list...",
        modal_btn_dev_simulate: "⚡ Simulate Dev Payment Success",

        // Customer Profile Page (Profile.cshtml)
        profile_hero_eyebrow: "PASSENGER ACCOUNT PORTAL",
        profile_hero_title: "Personal Profile & Security",
        profile_hero_subtitle: "Manage your contact info for e-ticket delivery and update account password.",
        profile_btn_view_tickets: "🎟️ View My Tickets →",
        profile_info_card_title: "Passenger Details",
        profile_info_card_desc: "Name and phone number will autofill when you book new tickets.",
        profile_label_fullname: "Passenger Full Name",
        profile_placeholder_fullname: "e.g. John Doe",
        profile_label_phone: "Contact Phone Number",
        profile_placeholder_phone: "e.g. 0912 345 678",
        profile_label_email: "Email Address (Account ID)",
        profile_email_locked: "🔒 Locked",
        profile_email_hint: "Email is used for e-tickets and login; it cannot be modified directly.",
        profile_btn_save_info: "Save Profile Changes",
        profile_pwd_card_title: "Change Security Password",
        profile_pwd_card_desc: "Password must be at least 12 characters with uppercase, lowercase, number, and symbol.",
        profile_label_current_pwd: "Current Password",
        profile_placeholder_current_pwd: "Enter current password",
        profile_label_new_pwd: "New Password",
        profile_placeholder_new_pwd: "Minimum 12 strong characters",
        profile_label_confirm_pwd: "Confirm New Password",
        profile_placeholder_confirm_pwd: "Re-enter new password",
        profile_btn_save_pwd: "Update Password",

        // Customer Registration Page (Register.cshtml)
        reg_brand_eyebrow: "SMART TICKET - SMT",
        reg_brand_heading: "A new journey<br /><em>begins right here.</em>",
        reg_brand_copy: "Create your passenger account to begin using Smart Ticket.",
        reg_brand_tagline: "BOOK · CHOOSE SEAT · HIT THE ROAD",
        reg_card_eyebrow: "PASSENGER ACCOUNT",
        reg_card_title: "Create Account",
        reg_card_subtitle: "Free registration to start booking tickets.",
        reg_label_fullname: "Passenger Full Name",
        reg_placeholder_fullname: "e.g. John Doe",
        reg_label_email: "Email",
        reg_placeholder_email: "email@example.com",
        reg_label_phone: "Phone Number",
        reg_placeholder_phone: "09xx xxx xxx",
        reg_label_pwd: "Password",
        reg_placeholder_pwd: "At least 12 characters",
        reg_label_confirm_pwd: "Confirm Password",
        reg_placeholder_confirm_pwd: "Re-enter password",
        reg_btn_submit: "Create Account",
        reg_has_account: "Already have an account?",
        reg_link_login: "Sign in",

        // Ticket Details Page (TicketDetails.cshtml)
        detail_verified_badge: "✓ VERIFIED PASSENGER",
        detail_page_title: "Electronic Bus Ticket (E-Ticket)",
        detail_code_label: "Booking Code:",
        detail_count_label: "Quantity:",
        detail_operator_label: "Operator:",
        detail_tickets_suffix: "tickets",
        detail_btn_back: "← My Tickets",
        detail_btn_print: "Print / Save PDF",
        detail_btn_rebook: "Book Another Trip →",
        detail_empty_title: "No tickets issued yet",
        detail_empty_desc: "This booking is either awaiting payment or settlement. Please check back shortly.",
        detail_legal_sub: "CERTIFIED E-TICKET",
        detail_ticket_num_label: "TICKET NO:",
        detail_route_label: "BUS ROUTE",
        detail_dep_label: "DEPARTURE TIME",
        detail_seat_label: "SEAT NUMBER",
        detail_fare_label: "OFFICIAL FARE",
        detail_vat_note: "(VAT & Insurance included)",
        detail_vehicle_type_label: "VEHICLE TYPE",
        detail_stub_title: "STATION CHECK-IN",
        detail_stub_valid: "Valid QR Code",
        detail_stub_hint: "Present this QR code to station staff or driver upon boarding",
        detail_stub_no_qr: "No QR Code",
        detail_legal_footer: "🛡️ This e-ticket has full legal boarding validity equivalent to paper tickets under Circular 12/2020/TT-BGTVT.",

        // Shared / Filters / Search / Auth Gates / 2FA
        modal_close_title: "Close modal",
        login_demo_hint_title: "⚡ Demo accounts for role-based multi-operator testing:",
        filter_all_vehicles: "All Vehicles",
        filter_limousine: "Limousine",
        filter_sleeper: "Sleeper Berth",
        filter_seats: "Seater Coach",
        sort_label: "Sort by:",
        sort_time_asc: "Earliest Departure",
        sort_time_desc: "Latest Departure",
        sort_price_asc: "Lowest Fare",
        sort_price_desc: "Highest Fare",
        sort_seats_desc: "Most Available Seats",
        fare_prefix: "Official Fare",
        sold_out_btn: "Sold Out",
        swap_tooltip: "Swap departure and destination",
        auth_gate_title: "Sign in required to book",
        auth_gate_desc: "Your e-ticket will be automatically stored in your Passenger Account Portal upon payment.",
        auth_gate_login: "Sign In",
        auth_gate_register: "Create Account",
        auth_logged_in_prefix: "✓ Signed in as:",
        auth_saved_to: "Tickets are automatically saved to",
        twofa_eyebrow: "ACCOUNT SECURITY",
        twofa_title: "Enter Verification Code",
        twofa_subtitle: "Enter the 6-digit code from your authenticator app.",
        twofa_code_label: "Verification Code",
        twofa_submit_btn: "Verify"
    }
};

// GLOBAL STATE MANAGERS
window.SMT = {
    currentLang: 'vi',
    currentTheme: 'dark',

    setLanguage: function(lang, notify = false) {
        if (!SMT_I18N[lang]) lang = 'vi';
        this.currentLang = lang;
        document.documentElement.setAttribute('data-lang', lang);
        try { localStorage.setItem('smartticket_lang', lang); } catch (e) {}

        // Update active switch button
        document.querySelectorAll('.lang-option').forEach(btn => {
            btn.classList.toggle('active', btn.getAttribute('data-lang') === lang);
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

        // Notify custom subscribers
        window.dispatchEvent(new CustomEvent('smt:languagechange', { detail: { lang: lang } }));
    },

    setTheme: function(theme) {
        if (theme !== 'light' && theme !== 'dark') theme = 'dark';
        this.currentTheme = theme;
        document.documentElement.setAttribute('data-theme', theme);
        try { localStorage.setItem('smartticket_theme', theme); } catch (e) {}

        const btn = document.getElementById('btn-theme-toggle');
        if (btn) {
            btn.setAttribute('title', theme === 'dark' ? 'Chuyển sang nền Sáng / Switch to Light mode' : 'Chuyển sang nền Tối / Switch to Dark mode');
        }

        window.dispatchEvent(new CustomEvent('smt:themechange', { detail: { theme: theme } }));
    },

    toggleTheme: function() {
        const nextTheme = this.currentTheme === 'dark' ? 'light' : 'dark';
        this.setTheme(nextTheme);
    },

    t: function(key) {
        const dict = SMT_I18N[this.currentLang] || SMT_I18N.vi;
        return dict[key] !== undefined ? dict[key] : key;
    }
};

// INITIALIZE ON DOM READY
document.addEventListener('DOMContentLoaded', function() {
    // 1. Theme initialization
    let savedTheme = 'dark';
    try { savedTheme = localStorage.getItem('smartticket_theme') || 'dark'; } catch (e) {}
    SMT.setTheme(savedTheme);

    const themeToggleBtn = document.getElementById('btn-theme-toggle');
    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', function() {
            SMT.toggleTheme();
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
