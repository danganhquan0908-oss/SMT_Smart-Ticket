using BusGoSmartTicket.Models.DTOs.Operators;
using BusGoSmartTicket.Models.Entities;

namespace BusGoSmartTicket.Services.Interfaces.Operators;

public interface IOperatorRepository
{
    Task<bool> OperatorTaxCodeExistsAsync(string taxCode, CancellationToken ct);
    Task<bool> UserEmailExistsAsync(string email, CancellationToken ct);
    Task RegisterAsync(OperatorRegistrationRequest request, CancellationToken ct);
    Task<IReadOnlyList<OperatorApplicationDto>> GetApplicationsAsync(CancellationToken ct);
    Task<ActivationNotice?> ReviewAsync(Guid operatorId, bool approve, string? note, string tokenHash, DateTime expiresAtUtc, CancellationToken ct);
    Task<ActivationNotice> InviteDispatcherAsync(Guid operatorId, string email, string tokenHash, DateTime expiresAtUtc, CancellationToken ct);
    Task<ActivationNotice> ResendActivationAsync(Guid operatorId, string tokenHash, DateTime expiresAtUtc, CancellationToken ct);
    Task<string> GetLicensePathAsync(Guid operatorId, CancellationToken ct);
    Task<ActivationInfo?> GetActivationAsync(string tokenHash, CancellationToken ct);
    Task<AuthenticatorSetup> BeginActivationAsync(string tokenHash, string password, CancellationToken ct);
    Task CompleteActivationAsync(string tokenHash, string authenticatorCode, CancellationToken ct);
    Task<Guid?> GetActiveOperatorIdAsync(Guid userId, CancellationToken ct);
    Task<OperatorProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<SeatTemplate>> GetSeatTemplatesAsync(Guid operatorId, CancellationToken ct);
    Task<SeatTemplate> CreateSeatTemplateAsync(Guid operatorId, SeatTemplateRequest request, CancellationToken ct);
    Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(Guid operatorId, CancellationToken ct);
    Task<Vehicle> CreateVehicleAsync(Guid operatorId, VehicleRequest request, CancellationToken ct);
    Task UpdateVehicleAsync(Guid operatorId, Guid vehicleId, VehicleRequest request, CancellationToken ct);
    Task DeactivateVehicleAsync(Guid operatorId, Guid vehicleId, CancellationToken ct);
    Task<IReadOnlyList<Driver>> GetDriversAsync(Guid operatorId, CancellationToken ct);
    Task<Driver> CreateDriverAsync(Guid operatorId, DriverRequest request, CancellationToken ct);
    Task UpdateDriverAsync(Guid operatorId, Guid driverId, DriverRequest request, CancellationToken ct);
    Task DeactivateDriverAsync(Guid operatorId, Guid driverId, CancellationToken ct);
    Task<IReadOnlyList<Trip>> GetTripsAsync(Guid operatorId, CancellationToken ct);
    Task<Trip> CreateTripAsync(Guid operatorId, Guid userId, TripRequest request, CancellationToken ct);
    Task UpdateTripScheduleAsync(Guid operatorId, Guid userId, Guid tripId, TripScheduleRequest request, CancellationToken ct);
    Task ChangeTripPriceAsync(Guid operatorId, Guid userId, Guid tripId, decimal newPrice, CancellationToken ct);
    Task<IReadOnlyList<TripPriceAuditDto>> GetPriceHistoryAsync(Guid operatorId, Guid tripId, CancellationToken ct);
}
