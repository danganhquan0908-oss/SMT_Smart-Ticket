using System.ComponentModel.DataAnnotations;

namespace SmartTicket.Application.Operations;

public sealed record OperationsTripDto(
    Guid Id,
    string DepartureStation,
    string ArrivalStation,
    string Operator,
    string VehiclePlate,
    string Driver,
    DateTime DepartureTimeUtc,
    string Status,
    int TicketCount,
    bool HasApprovedDispatch,
    int CheckedInCount = 0,
    int TotalCapacity = 0,
    string? DriverPhone = null,
    string? DispatchSignature = null,
    DateTime? DispatchedAtUtc = null,
    string? DispatchNotes = null
);

public sealed record TripPassengerManifestItemDto(
    Guid TicketId,
    string SeatCode,
    string PassengerName,
    string PassengerPhone,
    string Status,
    decimal Price,
    DateTime? CheckedInAtUtc,
    string? CheckInToken
);

public sealed record TripManifestDto(
    Guid TripId,
    string RouteName,
    string DepartureStation,
    string ArrivalStation,
    string OperatorName,
    string VehiclePlate,
    string DriverName,
    string DriverPhone,
    DateTime DepartureTimeUtc,
    string Status,
    int TotalCapacity,
    int SoldTicketCount,
    int CheckedInCount,
    bool HasApprovedDispatch,
    string? DispatchSignature,
    DateTime? DispatchedAtUtc,
    string? DispatchNotes,
    IReadOnlyList<TripPassengerManifestItemDto> Passengers
);
public sealed class DispatchRequest
{
    [Required] public Guid TripId { get; init; }
    public bool VehicleDocumentsValid { get; init; }
    public bool DriverLicenseValid { get; init; }
    public bool DriverFitForDuty { get; init; }
    public bool PassengerCapacityValid { get; init; }
    [StringLength(500)] public string? Notes { get; init; }
}
public sealed record DispatchResult(bool Approved, string Status, string Signature, DateTime SignedAtUtc);
public sealed record TicketScanRequest(string Jwt, bool IsOffline = false, DateTime? ScannedAtUtc = null);
public sealed record TicketScanResult(string Result, bool Accepted, bool Alarm, string? SeatCode, string? Trip, DateTime? DepartureTimeUtc);
public sealed class StationMemberRequest { [Required, EmailAddress] public string Email { get; init; } = ""; }
public sealed class PayoutAccountRequest
{
    [Required, RegularExpression("^[0-9]{6}$")] public string BankBin { get; init; } = "";
    [Required, StringLength(28, MinimumLength = 5)] public string AccountNumber { get; init; } = "";
    [Required, StringLength(100, MinimumLength = 2)] public string AccountName { get; init; } = "";
}
public sealed record SettlementDto(Guid Id, Guid TripId, string Operator, decimal GrossAmount, decimal CommissionAmount, decimal NetAmount, string Status, string? ProviderReference, DateTime CreatedAtUtc, DateTime? PaidAtUtc);
public sealed record PayoutInstruction(Guid SettlementId, string Reference, decimal Amount, string BankBin, string AccountNumber, string AccountName);
public sealed record PayoutResult(bool Succeeded, string? ProviderReference, string? Error);
public interface ISettlementPayoutProvider
{
    bool IsConfigured { get; }
    Task<PayoutResult> TransferAsync(PayoutInstruction instruction, CancellationToken ct);
}
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
public sealed class OperationsNotFoundException(string message) : Exception(message);
public sealed class OperationsConflictException(string message) : Exception(message);
