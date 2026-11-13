using System.ComponentModel.DataAnnotations;

namespace SmartTicket.Web.Models;

public sealed class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public sealed class TwoFactorViewModel
{
    [Required, RegularExpression("^[0-9]{6}$", ErrorMessage = "Nhập mã 6 chữ số.")] public string Code { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
