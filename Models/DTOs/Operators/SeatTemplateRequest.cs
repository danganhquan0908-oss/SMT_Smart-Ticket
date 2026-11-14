using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed class SeatTemplateRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên mẫu sơ đồ ghế.")]
    [StringLength(100, ErrorMessage = "Tên mẫu tối đa 100 ký tự.")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập loại xe áp dụng.")]
    [StringLength(80, ErrorMessage = "Loại xe tối đa 80 ký tự.")]
    public string VehicleType { get; init; } = string.Empty;

    [Range(1, 2, ErrorMessage = "Số tầng xe chỉ có thể là 1 hoặc 2 tầng.")]
    public int DeckCount { get; init; } = 1;

    [Range(1, 60, ErrorMessage = "Số ghế mỗi tầng từ 1 đến 60 ghế.")]
    public int SeatsPerDeck { get; init; } = 16;
}
