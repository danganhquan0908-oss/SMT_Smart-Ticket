using System.Globalization;
using System.Text;
using BusGoSmartTicket.Models.DTOs.Bookings;
namespace BusGoSmartTicket.Services.Bookings;

public static class TripSearchFiltering
{
    public static string Fold(string value) => new string(value.Normalize(NormalizationForm.FormD)
        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()).ToLowerInvariant().Replace('đ', 'd');
    public static IEnumerable<TripSearchResult> Apply(IEnumerable<TripSearchResult> trips, string? vehicle, string? company,
        string? period, string? sort, bool availableOnly)
    {
        var q = trips;
        if (!string.IsNullOrWhiteSpace(vehicle))
            q = q.Where(t => Fold(t.VehicleType).Contains(Fold(vehicle)));
        if (!string.IsNullOrWhiteSpace(company))
            q = q.Where(t => t.Operator == company);
        if (availableOnly)
            q = q.Where(t => t.AvailableSeats > 0);
        q = q.Where(t => period switch
        {
            "morning" => t.DepartureTimeUtc.AddHours(7).Hour is >= 6 and < 12,
            "afternoon" => t.DepartureTimeUtc.AddHours(7).Hour is >= 12 and < 18,
            "night" => t.DepartureTimeUtc.AddHours(7).Hour is >= 18 or < 6,
            _ => true
        });
        return sort switch
        {
            "price" => q.OrderBy(t => t.Price).ThenBy(t => t.DepartureTimeUtc),
            "seats" => q.OrderByDescending(t => t.AvailableSeats).ThenBy(t => t.DepartureTimeUtc),
            "late" => q.OrderByDescending(t => t.DepartureTimeUtc),
            _ => q.OrderBy(t => t.DepartureTimeUtc)
        };
    }
}
