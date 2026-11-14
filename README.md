# BusGo SmartTicket

Ứng dụng đặt vé xe khách ASP.NET Core MVC (.NET 10): tìm chuyến, chọn ghế, giữ chỗ 7 phút, thanh toán VietQR, vé điện tử, hội viên, vận hành nhà xe, soát vé tại bến và đối soát.

## Chạy ứng dụng

Mở `BusGoSmartTicket.sln` trong Visual Studio, chọn profile **BusGoSmartTicket**, nhấn **F5**. Debug trên Windows tự chuẩn bị PostgreSQL local và dừng đúng bản web chạy bằng script trước build để tránh trùng cổng 5088. PostgreSQL vẫn chạy. Nếu IDE đang dừng ở phiên Debug cũ, dùng Shift+F5 rồi chạy lại.

Hoặc nhấp đúp `start-local.cmd`. Truy cập http://localhost:5088. Chỉ chạy một phiên ứng dụng trên cổng này. Dừng phiên chạy bằng script với `scripts/stop-local.ps1`.

Script tạo PostgreSQL riêng tại `.local/pgdata`, cổng 55432. Khóa và cấu hình local được tạo ngẫu nhiên trong `.local/development.json` và `appsettings.Local.json`. Không đưa hai file này lên kho mã hoặc máy chủ triển khai.

## Tổ chức mã nguồn MVC

- `Controllers/`: điều phối yêu cầu HTTP; API trong `Controllers/Api/`.
- `Models/Entities/`, `Models/DTOs/`, `Models/ViewModels/`: dữ liệu nghiệp vụ, trao đổi và trình bày.
- `Views/`: Razor theo chức năng, layout trong `Views/Shared/`.
- `Services/`: nghiệp vụ; hợp đồng trong `Services/Interfaces/`.
- `Data/`: DbContext, migration và repository.
- `wwwroot/`: CSS, JavaScript, phông chữ và hình ảnh.

Ngôn ngữ và giao diện được lưu trên trình duyệt. `localization.js` xử lý văn bản và thuộc tính hỗ trợ truy cập; danh mục tiếng Anh nằm trong `translations.en.js`. `appearance.js` cung cấp chuyển màu dạng vòng tròn và tôn trọng tùy chọn giảm chuyển động. Kích thước nút dùng chung ở cả hai giao diện trong `interaction.css`.

## Cấu hình vận hành

Build bằng `dotnet build BusGoSmartTicket.sln`. Cơ sở dữ liệu mới tạo bảng và quản trị viên theo `Admin:Email` / `Admin:Password`. Hành khách tự đăng ký; nhà xe gửi hồ sơ để quản trị viên phê duyệt. Thông tin đăng nhập không được lưu trong kho mã. Khi chạy local, xem cấu hình quản trị riêng tại `.local/development.json`.

Thanh toán thật cần `Payments:BankBin`, `Payments:AccountNumber`, `Payments:AccountName` và khóa xác thực webhook `Payments:SePayApiKey` hoặc `Payments:WebhookSecret` (ít nhất 32 byte). Chỉ hiển thị QR khi đủ thông tin ngân hàng. Webhook luôn yêu cầu xác thực; không có API kích hoạt thanh toán giả. Payout dùng endpoint HTTPS và API key của nhà cung cấp; không có giải ngân mô phỏng. Email kích hoạt cần cấu hình SMTP.

Khi triển khai nhiều tiến trình, cấu hình Redis để khóa giữ ghế dùng chung. Cập nhật `.env.example` cho Docker hoặc dùng biến môi trường trên máy chủ. Đổi/hủy và hoàn tiền được xử lý với nhà xe; chưa có quy trình hoàn tiền tự động.

Mã nguồn bàn giao không chứa project kiểm thử, nút điền tài khoản mẫu, API vé mẫu, công cụ giả lập mất mạng hay bộ dữ liệu tự tạo khi khởi động.
