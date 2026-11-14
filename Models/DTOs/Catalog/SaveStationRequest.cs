using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.DTOs.Catalog;

public sealed class SaveStationRequest
{
    [Required(ErrorMessage = "Vui lòng nhập mã bến xe.")]
    [StringLength(12, MinimumLength = 2, ErrorMessage = "Mã bến xe phải từ 2 đến 12 ký tự.")]
    public string Code { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên bến xe.")]
    [StringLength(160, ErrorMessage = "Tên bến xe không được vượt quá 160 ký tự.")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Tỉnh / Thành phố.")]
    [StringLength(100, ErrorMessage = "Tên Tỉnh/Thành phố không được vượt quá 100 ký tự.")]
    public string Province { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ chi tiết của bến xe.")]
    [StringLength(300, ErrorMessage = "Địa chỉ không được vượt quá 300 ký tự.")]
    public string Address { get; init; } = string.Empty;
}
