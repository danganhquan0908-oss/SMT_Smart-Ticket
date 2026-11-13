using SmartTicket.Application.Bookings;
using SmartTicket.Application.Catalog;
namespace SmartTicket.Web.Models;
public sealed class TripSearchViewModel
{
    public IReadOnlyList<StationDto> Stations { get; init; } = [];
    public IReadOnlyList<TripSearchResult> Results { get; init; } = [];
    public Guid? DepartureStationId { get; init; }
    public Guid? ArrivalStationId { get; init; }
    public DateOnly TravelDate { get; init; }
    public string? Error { get; init; }
}
