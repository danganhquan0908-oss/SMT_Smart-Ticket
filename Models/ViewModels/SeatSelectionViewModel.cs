using BusGoSmartTicket.Models.DTOs.Bookings;

namespace BusGoSmartTicket.Models.ViewModels;

/// <summary>Only presentation data needed by the seat-selection view.</summary>
public sealed class SeatSelectionViewModel
{
    public Guid TripId
    {
        get; init;
    }
    public decimal TicketPrice
    {
        get; init;
    }
    public string OperatorName { get; init; } = "";
    public string RouteName { get; init; } = "";
    public string VehicleType { get; init; } = "";
    public DateTime DepartureTimeUtc
    {
        get; init;
    }
    public string? DiscountName
    {
        get; init;
    }
    public IReadOnlyList<SeatDto> Seats { get; init; } = [];
    public string CustomerName { get; init; } = "";
    public string CustomerPhone { get; init; } = "";
    public string CustomerEmail { get; init; } = "";
    public PaymentBankViewModel Bank { get; init; } = new();
}
