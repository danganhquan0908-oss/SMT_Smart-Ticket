using System.Security.Cryptography;
using System.Text;
using SmartTicket.Domain.Entities;

namespace SmartTicket.Application.Operators;

public sealed class OperatorService(IOperatorRepository repository) : IOperatorService
{
    public async Task RegisterAsync(OperatorRegistrationRequest request, CancellationToken ct)
    {
        if (await repository.OperatorTaxCodeExistsAsync(request.TaxCode.Trim(), ct)) throw new OperatorConflictException("Mã số thuế này đã được đăng ký.");
        if (await repository.UserEmailExistsAsync(request.Email.Trim(), ct)) throw new OperatorConflictException("Email này đã có tài khoản.");
        await repository.RegisterAsync(request, ct);
    }

    public Task<IReadOnlyList<OperatorApplicationDto>> GetApplicationsAsync(CancellationToken ct) => repository.GetApplicationsAsync(ct);

    public async Task<ActivationNotice?> ReviewAsync(Guid operatorId, bool approve, string? note, CancellationToken ct)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var result = await repository.ReviewAsync(operatorId, approve, note, HashToken(rawToken), DateTime.UtcNow.AddHours(48), ct);
        return result is null ? null : result with { Token = rawToken };
    }

    public async Task<ActivationNotice> InviteDispatcherAsync(Guid userId, string email, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var notice = await repository.InviteDispatcherAsync(operatorId, email, HashToken(rawToken), DateTime.UtcNow.AddHours(48), ct);
        return notice with { Token = rawToken };
    }

    public async Task<ActivationNotice> ResendActivationAsync(Guid operatorId, CancellationToken ct)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var notice = await repository.ResendActivationAsync(operatorId, HashToken(rawToken), DateTime.UtcNow.AddHours(48), ct);
        return notice with { Token = rawToken };
    }

    public async Task<string> GetLicensePathAsync(Guid operatorId, CancellationToken ct) => await repository.GetLicensePathAsync(operatorId, ct);

    public async Task<ActivationInfo?> GetActivationAsync(string token, CancellationToken ct) =>
        await repository.GetActivationAsync(HashToken(token), ct);

    public async Task<AuthenticatorSetup> BeginActivationAsync(string token, string password, CancellationToken ct)
    {
        if (await GetActivationAsync(token, ct) is null) throw new OperatorConflictException("Liên kết kích hoạt không hợp lệ, đã dùng hoặc đã hết hạn.");
        return await repository.BeginActivationAsync(HashToken(token), password, ct);
    }

    public Task CompleteActivationAsync(string token, string authenticatorCode, CancellationToken ct) =>
        repository.CompleteActivationAsync(HashToken(token), authenticatorCode, ct);

    public async Task<IReadOnlyList<SeatTemplateDto>> GetSeatTemplatesAsync(Guid userId, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        return (await repository.GetSeatTemplatesAsync(operatorId, ct)).Select(Map).ToList();
    }

    public async Task<SeatTemplateDto> CreateSeatTemplateAsync(Guid userId, SeatTemplateRequest request, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        return Map(await repository.CreateSeatTemplateAsync(operatorId, request, ct));
    }

    public async Task<IReadOnlyList<VehicleDto>> GetVehiclesAsync(Guid userId, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        return (await repository.GetVehiclesAsync(operatorId, ct)).Select(Map).ToList();
    }

    public async Task<VehicleDto> CreateVehicleAsync(Guid userId, VehicleRequest request, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        return Map(await repository.CreateVehicleAsync(operatorId, request, ct));
    }

    public async Task UpdateVehicleAsync(Guid userId, Guid vehicleId, VehicleRequest request, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        await repository.UpdateVehicleAsync(operatorId, vehicleId, request, ct);
    }

    public async Task DeactivateVehicleAsync(Guid userId, Guid vehicleId, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        await repository.DeactivateVehicleAsync(operatorId, vehicleId, ct);
    }

    public async Task<IReadOnlyList<DriverDto>> GetDriversAsync(Guid userId, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        return (await repository.GetDriversAsync(operatorId, ct)).Select(Map).ToList();
    }

    public async Task<DriverDto> CreateDriverAsync(Guid userId, DriverRequest request, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        return Map(await repository.CreateDriverAsync(operatorId, request, ct));
    }

    public async Task UpdateDriverAsync(Guid userId, Guid driverId, DriverRequest request, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        await repository.UpdateDriverAsync(operatorId, driverId, request, ct);
    }

    public async Task DeactivateDriverAsync(Guid userId, Guid driverId, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        await repository.DeactivateDriverAsync(operatorId, driverId, ct);
    }

    public async Task<IReadOnlyList<OperatorTripDto>> GetTripsAsync(Guid userId, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        return (await repository.GetTripsAsync(operatorId, ct)).Select(Map).ToList();
    }

    public async Task<OperatorTripDto> CreateTripAsync(Guid userId, TripRequest request, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        return Map(await repository.CreateTripAsync(operatorId, userId, request, ct));
    }

    public async Task UpdateTripScheduleAsync(Guid userId, Guid tripId, TripScheduleRequest request, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        await repository.UpdateTripScheduleAsync(operatorId, userId, tripId, request, ct);
    }

    public async Task ChangeTripPriceAsync(Guid userId, Guid tripId, decimal newPrice, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        await repository.ChangeTripPriceAsync(operatorId, userId, tripId, newPrice, ct);
    }

    public async Task<IReadOnlyList<TripPriceAuditDto>> GetPriceHistoryAsync(Guid userId, Guid tripId, CancellationToken ct)
    {
        var operatorId = await RequireOperatorAsync(userId, ct);
        return await repository.GetPriceHistoryAsync(operatorId, tripId, ct);
    }

    public Task<OperatorProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct) =>
        repository.GetProfileAsync(userId, ct);

    private async Task<Guid> RequireOperatorAsync(Guid userId, CancellationToken ct) =>
        await repository.GetActiveOperatorIdAsync(userId, ct) ?? throw new OperatorNotFoundException("Tài khoản chưa được gán vào nhà xe đang hoạt động.");

    public static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    private static SeatTemplateDto Map(SeatTemplate x) => new(x.Id, x.Name, x.VehicleType, x.DeckCount, x.SeatsPerDeck);
    private static VehicleDto Map(Vehicle x) => new(x.Id, x.PlateNumber, x.VehicleType, x.SeatTemplateId, x.SeatTemplate?.Name ?? string.Empty, x.SeatTemplate?.Seats.Count ?? 0, x.IsActive);
    private static DriverDto Map(Driver x) => new(x.Id, x.FullName, x.Phone, x.LicenseNumber, x.IsActive);
    private static OperatorTripDto Map(Trip x) => new(x.Id,
        $"{x.Route?.DepartureStation?.Name ?? "?"} → {x.Route?.ArrivalStation?.Name ?? "?"}", x.RouteId,
        x.Vehicle?.PlateNumber ?? string.Empty, x.VehicleId, x.Driver?.FullName ?? string.Empty, x.DriverId,
        x.DepartureTimeUtc, x.ArrivalTimeUtc, x.TicketPrice, x.Status,
        x.Tickets.Count(t => t.Status == TicketLifecycleStatus.Sold), x.Version);
}
