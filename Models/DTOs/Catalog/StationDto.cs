namespace BusGoSmartTicket.Models.DTOs.Catalog;

public sealed record StationDto(
    Guid Id,
    string Code,
    string Name,
    string Province,
    string Address,
    bool IsActive);
