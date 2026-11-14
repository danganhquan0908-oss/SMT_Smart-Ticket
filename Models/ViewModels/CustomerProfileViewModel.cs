using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.ViewModels;

public sealed class CustomerProfileViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [Display(Name = "Họ và tên")]
    [StringLength(160, ErrorMessage = "Họ và tên không được vượt quá 160 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại chưa đúng định dạng.")]
    [StringLength(24, ErrorMessage = "Số điện thoại không được vượt quá 24 ký tự.")]
    [Display(Name = "Số điện thoại liên hệ")]
    public string? PhoneNumber
    {
        get; set;
    }

    [Display(Name = "Địa chỉ Email (Định danh tài khoản)")]
    public string Email { get; set; } = string.Empty;
}
