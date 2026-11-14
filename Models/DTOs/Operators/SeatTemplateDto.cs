namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed record SeatTemplateDto(Guid Id, string Name, string VehicleType, int DeckCount, int SeatsPerDeck);
