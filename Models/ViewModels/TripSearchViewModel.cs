using BusGoSmartTicket.Models.DTOs.Bookings;
using BusGoSmartTicket.Models.DTOs.Catalog;
namespace BusGoSmartTicket.Models.ViewModels;

public sealed class TripSearchViewModel
{
    public IReadOnlyList<StationDto> Stations { get; init; } = [];
    public IReadOnlyList<TripSearchResult> Results { get; init; } = [];
    public Guid? DepartureStationId
    {
        get; init;
    }
    public Guid? ArrivalStationId
    {
        get; init;
    }
    public DateOnly TravelDate
    {
        get; init;
    }
    public string? Error
    {
        get; init;
    }
    public string? Vehicle
    {
        get; init;
    }
    public string? Company
    {
        get; init;
    }
    public string? Period
    {
        get; init;
    }
    public string? Sort
    {
        get; init;
    }
    public bool AvailableOnly
    {
        get; init;
    }
    public int Page
    {
        get; init;
    }
    public int TotalResults
    {
        get; init;
    }
    public IReadOnlyList<string> Companies { get; init; } = [];
}
