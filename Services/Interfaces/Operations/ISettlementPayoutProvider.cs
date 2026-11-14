using BusGoSmartTicket.Models.DTOs.Operations;

namespace BusGoSmartTicket.Services.Interfaces.Operations;

public interface ISettlementPayoutProvider
{
    bool IsConfigured
    {
        get;
    }
    Task<PayoutResult> TransferAsync(PayoutInstruction instruction, CancellationToken ct);
}
