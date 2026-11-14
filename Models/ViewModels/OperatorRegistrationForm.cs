using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.ViewModels;

public sealed class OperatorRegistrationForm
{
    [Required, StringLength(180)] public string CompanyName { get; set; } = string.Empty;
    [Required, StringLength(20)] public string TaxCode { get; set; } = string.Empty;
    [Required, Phone, StringLength(24)] public string Hotline { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
    [Required]
    public IFormFile? BusinessLicense
    {
        get; set;
    }
}
