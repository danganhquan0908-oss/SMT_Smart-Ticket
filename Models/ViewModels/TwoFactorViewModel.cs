using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.ViewModels;

public sealed class TwoFactorViewModel
{
    [Required, RegularExpression("^[0-9]{6}$", ErrorMessage = "Nhập mã 6 chữ số.")] public string Code { get; set; } = string.Empty;
    public bool RememberMe
    {
        get; set;
    }
    public string? ReturnUrl
    {
        get; set;
    }
}
