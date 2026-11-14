using BusGoSmartTicket.Models.DTOs.Operators;

namespace BusGoSmartTicket.Services.Interfaces.Operators;

public interface IOperatorService
{
    Task RegisterAsync(OperatorRegistrationRequest request, CancellationToken ct);
    Task<IReadOnlyList<OperatorApplicationDto>> GetApplicationsAsync(CancellationToken ct);
    Task<ActivationNotice?> ReviewAsync(Guid operatorId, bool approve, string? note, CancellationToken ct);
    Task<ActivationNotice> InviteDispatcherAsync(Guid userId, string email, CancellationToken ct);
    Task<ActivationNotice> ResendActivationAsync(Guid operatorId, CancellationToken ct);
    Task<string> GetLicensePathAsync(Guid operatorId, CancellationToken ct);
    Task<ActivationInfo?> GetActivationAsync(string token, CancellationToken ct);
    Task<AuthenticatorSetup> BeginActivationAsync(string token, string password, CancellationToken ct);
    Task CompleteActivationAsync(string token, string authenticatorCode, CancellationToken ct);

    Task<IReadOnlyList<SeatTemplateDto>> GetSeatTemplatesAsync(Guid userId, CancellationToken ct);
    Task<SeatTemplateDto> CreateSeatTemplateAsync(Guid userId, SeatTemplateRequest request, CancellationToken ct);

    Task<IReadOnlyList<VehicleDto>> GetVehiclesAsync(Guid userId, CancellationToken ct);
    Task<VehicleDto> CreateVehicleAsync(Guid userId, VehicleRequest request, CancellationToken ct);
    Task UpdateVehicleAsync(Guid userId, Guid vehicleId, VehicleRequest request, CancellationToken ct);
    Task DeactivateVehicleAsync(Guid userId, Guid vehicleId, CancellationToken ct);

    Task<IReadOnlyList<DriverDto>> GetDriversAsync(Guid userId, CancellationToken ct);
    Task<DriverDto> CreateDriverAsync(Guid userId, DriverRequest request, CancellationToken ct);
    Task UpdateDriverAsync(Guid userId, Guid driverId, DriverRequest request, CancellationToken ct);
    Task DeactivateDriverAsync(Guid userId, Guid driverId, CancellationToken ct);

    Task<IReadOnlyList<OperatorTripDto>> GetTripsAsync(Guid userId, CancellationToken ct);
    Task<OperatorTripDto> CreateTripAsync(Guid userId, TripRequest request, CancellationToken ct);
    Task UpdateTripScheduleAsync(Guid userId, Guid tripId, TripScheduleRequest request, CancellationToken ct);
    Task ChangeTripPriceAsync(Guid userId, Guid tripId, decimal newPrice, CancellationToken ct);
    Task<IReadOnlyList<TripPriceAuditDto>> GetPriceHistoryAsync(Guid userId, Guid tripId, CancellationToken ct);
    Task<OperatorProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct);
}
