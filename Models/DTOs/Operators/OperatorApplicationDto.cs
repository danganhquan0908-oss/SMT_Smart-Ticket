using BusGoSmartTicket.Models.Entities;

namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed record OperatorApplicationDto(
    Guid Id,
    string CompanyName,
    string TaxCode,
    string Email,
    string Hotline,
    OperatorStatus Status,
    string? ReviewNote,
    DateTime SubmittedAtUtc);
