using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartTicket.Web.Models;

public sealed class OperatorRegistrationForm
{
    [Required, StringLength(180)] public string CompanyName { get; set; } = string.Empty;
    [Required, StringLength(20)] public string TaxCode { get; set; } = string.Empty;
    [Required, Phone, StringLength(24)] public string Hotline { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
    [Required] public IFormFile? BusinessLicense { get; set; }
}

public sealed class OperatorActivationForm
{
    [Required] public string Token { get; set; } = string.Empty;
    [Required, MinLength(12), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required, Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = string.Empty;
    public string? SharedKey { get; set; }
    public string? Email { get; set; }
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "Nhập mã 6 chữ số.")] public string AuthenticatorCode { get; set; } = string.Empty;
}

public sealed class DispatcherInviteForm
{
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
}

public sealed class OperatorTripForm
{
    [Required] public Guid RouteId { get; set; }
    [Required] public Guid VehicleId { get; set; }
    [Required] public Guid DriverId { get; set; }
    [Required] public DateTime DepartureTime { get; set; }
    [Required] public DateTime ArrivalTime { get; set; }
    [Range(typeof(decimal), "1", "100000000")] public decimal TicketPrice { get; set; }
}
