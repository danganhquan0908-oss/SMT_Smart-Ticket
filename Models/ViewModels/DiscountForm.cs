using System.ComponentModel.DataAnnotations;
using BusGoSmartTicket.Models.Entities;
namespace BusGoSmartTicket.Models.ViewModels;

public sealed class DiscountForm
{
    [Required, StringLength(100)] public string Name { get; set; } = "";
    public MemberTier Tier { get; set; } = MemberTier.Bronze;
    [Range(typeof(decimal), "0.01", "99")] public decimal Percent { get; set; } = 5;
    public DateTime StartsAt { get; set; } = DateTime.UtcNow.AddHours(7);
    public DateTime EndsAt { get; set; } = DateTime.UtcNow.AddHours(7).AddDays(7);
}
