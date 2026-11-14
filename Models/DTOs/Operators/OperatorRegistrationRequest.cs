using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed class OperatorRegistrationRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên công ty hoặc doanh nghiệp vận tải.")]
    [StringLength(180, ErrorMessage = "Tên doanh nghiệp tối đa 180 ký tự.")]
    public string CompanyName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Mã số thuế doanh nghiệp.")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "Mã số thuế từ 10 đến 20 ký tự.")]
    public string TaxCode { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại hotline nhà xe.")]
    [StringLength(24, MinimumLength = 8, ErrorMessage = "Số điện thoại hotline không hợp lệ.")]
    public string Hotline { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email người đại diện pháp luật.")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
    [StringLength(256, ErrorMessage = "Email tối đa 256 ký tự.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng tải lên tệp Giấy phép kinh doanh vận tải.")]
    public string BusinessLicensePath { get; init; } = string.Empty;
}
