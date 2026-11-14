using BusGoSmartTicket.Models.DTOs.Operations;

namespace BusGoSmartTicket.Services.Interfaces.Operations;

public interface IStationOperationsService
{
    Task<IReadOnlyList<OperationsTripDto>> GetTripsAsync(Guid userId, CancellationToken ct);
    Task<TripManifestDto> GetTripManifestAsync(Guid userId, Guid tripId, CancellationToken ct);
    Task<DispatchResult> ClearDepartureAsync(Guid userId, DispatchRequest request, CancellationToken ct);
    Task<TicketScanResult> ScanTicketAsync(Guid userId, TicketScanRequest request, CancellationToken ct);
    Task<TicketScanResult> ManualCheckInSeatAsync(Guid userId, Guid tripId, string seatCode, CancellationToken ct);
    Task AssignStationMemberAsync(Guid stationId, string email, CancellationToken ct);
    Task DeactivateStationMemberAsync(Guid stationId, Guid userId, CancellationToken ct);
    Task CompleteTripAsync(Guid userId, Guid tripId, CancellationToken ct);
    Task SetPayoutAccountAsync(Guid userId, PayoutAccountRequest request, CancellationToken ct);
    Task<IReadOnlyList<SettlementDto>> GetSettlementsAsync(Guid userId, CancellationToken ct);
    Task TriggerSettlementPayoutAsync(Guid userId, Guid settlementId, CancellationToken ct);
}
