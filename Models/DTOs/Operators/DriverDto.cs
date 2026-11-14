namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed record DriverDto(Guid Id, string FullName, string Phone, string LicenseNumber, bool IsActive);
