using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed class DriverRequest
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên tài xế.")]
    [StringLength(160, ErrorMessage = "Họ tên tài xế tối đa 160 ký tự.")]
    public string FullName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại tài xế.")]
    [Phone(ErrorMessage = "Số điện thoại tài xế không hợp lệ.")]
    [StringLength(24, ErrorMessage = "Số điện thoại tối đa 24 ký tự.")]
    public string Phone { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số giấy phép lái xe.")]
    [StringLength(40, ErrorMessage = "Số giấy phép lái xe tối đa 40 ký tự.")]
    public string LicenseNumber { get; init; } = string.Empty;
}
