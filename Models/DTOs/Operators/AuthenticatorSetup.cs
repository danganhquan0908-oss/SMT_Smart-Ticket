namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed record AuthenticatorSetup(string Email, string SharedKey, string Issuer = "Smart Ticket");
