namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed record VehicleDto(Guid Id, string PlateNumber, string VehicleType, Guid SeatTemplateId, string TemplateName, int SeatCount, bool IsActive);
