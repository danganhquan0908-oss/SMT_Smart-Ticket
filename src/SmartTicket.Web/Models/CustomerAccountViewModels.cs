using System.ComponentModel.DataAnnotations;
using SmartTicket.Application.Account;

namespace SmartTicket.Web.Models;

public sealed class CustomerProfileViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [Display(Name = "Họ và tên")]
    [StringLength(160, ErrorMessage = "Họ và tên không được vượt quá 160 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại chưa đúng định dạng.")]
    [StringLength(24, ErrorMessage = "Số điện thoại không được vượt quá 24 ký tự.")]
    [Display(Name = "Số điện thoại liên hệ")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Địa chỉ Email (Định danh tài khoản)")]
    public string Email { get; set; } = string.Empty;
}

public sealed class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu hiện tại")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 12, ErrorMessage = "Mật khẩu mới cần có ít nhất 12 ký tự.")]
    [Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [Display(Name = "Xác nhận mật khẩu mới")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class CustomerTicketsViewModel
{
    public IReadOnlyList<CustomerBookingSummaryDto> Upcoming { get; init; } = [];
    public IReadOnlyList<CustomerBookingSummaryDto> Pending { get; init; } = [];
    public IReadOnlyList<CustomerBookingSummaryDto> History { get; init; } = [];
}
