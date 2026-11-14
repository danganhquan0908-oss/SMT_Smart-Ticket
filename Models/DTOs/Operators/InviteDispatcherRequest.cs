using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed class InviteDispatcherRequest
{
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
    [StringLength(256, ErrorMessage = "Email tối đa 256 ký tự.")]
    public string Email { get; init; } = string.Empty;
}
