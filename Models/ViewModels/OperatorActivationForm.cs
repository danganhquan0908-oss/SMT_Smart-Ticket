using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.ViewModels;

public sealed class OperatorActivationForm
{
    [Required] public string Token { get; set; } = string.Empty;
    [Required, MinLength(12), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required, Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = string.Empty;
    public string? SharedKey
    {
        get; set;
    }
    public string? Email
    {
        get; set;
    }
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "Nhập mã 6 chữ số.")] public string AuthenticatorCode { get; set; } = string.Empty;
}
