using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed class PriceChangeRequest
{
    [Range(typeof(decimal), "10000", "100000000", ErrorMessage = "Giá vé mới phải từ 10,000 đ đến 100,000,000 đ.")]
    public decimal NewPrice
    {
        get; init;
    }
}
