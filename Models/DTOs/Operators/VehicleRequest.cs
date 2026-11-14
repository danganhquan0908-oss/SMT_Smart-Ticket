using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed class VehicleRequest
{
    [Required(ErrorMessage = "Vui lòng nhập biển số xe.")]
    [RegularExpression("^[0-9A-Za-z.-]{5,16}$", ErrorMessage = "Biển số xe không hợp lệ (Ví dụ: 51B-123.45 hoặc 29B-999.88).")]
    public string PlateNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập chủng loại xe.")]
    [StringLength(80, ErrorMessage = "Chủng loại xe tối đa 80 ký tự.")]
    public string VehicleType { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn mẫu sơ đồ ghế áp dụng.")]
    public Guid SeatTemplateId
    {
        get; init;
    }
}
