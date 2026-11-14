namespace BusGoSmartTicket.Models.DTOs.Account;

public sealed record DiscountQuote(decimal BaseSeatPrice, decimal NetSeatPrice, Guid? CampaignId, string? Name, decimal Percent);
