using System.ComponentModel.DataAnnotations;
using SmartTicket.Domain.Entities;

namespace SmartTicket.Application.Operators;

public sealed record OperatorApplicationDto(
    Guid Id,
    string CompanyName,
    string TaxCode,
    string Email,
    string Hotline,
    OperatorStatus Status,
    string? ReviewNote,
    DateTime SubmittedAtUtc);

public sealed record ActivationNotice(string Email, string Token);
public sealed record ActivationInfo(string Email, string CompanyName, DateTime ExpiresAtUtc);
public sealed record AuthenticatorSetup(string Email, string SharedKey, string Issuer = "Smart Ticket");
public sealed record OperatorProfileDto(Guid Id, string CompanyName, string TaxCode, string Hotline);
public sealed record SeatTemplateDto(Guid Id, string Name, string VehicleType, int DeckCount, int SeatsPerDeck);
public sealed record VehicleDto(Guid Id, string PlateNumber, string VehicleType, Guid SeatTemplateId, string TemplateName, int SeatCount, bool IsActive);
public sealed record DriverDto(Guid Id, string FullName, string Phone, string LicenseNumber, bool IsActive);
public sealed record OperatorTripDto(Guid Id, string RouteName, Guid RouteId, string PlateNumber, Guid VehicleId, string DriverName, Guid DriverId, DateTime DepartureTimeUtc, DateTime ArrivalTimeUtc, decimal TicketPrice, TripStatus Status, int SoldTicketCount, long Version);
public sealed record TripPriceAuditDto(DateTime ChangedAtUtc, string ModifiedBy, decimal OldPrice, decimal NewPrice, string Action);

public sealed class InviteDispatcherRequest
{
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
    [StringLength(256, ErrorMessage = "Email tối đa 256 ký tự.")]
    public string Email { get; init; } = string.Empty;
}

public sealed class OperatorRegistrationRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên công ty hoặc doanh nghiệp vận tải.")]
    [StringLength(180, ErrorMessage = "Tên doanh nghiệp tối đa 180 ký tự.")]
    public string CompanyName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Mã số thuế doanh nghiệp.")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "Mã số thuế từ 10 đến 20 ký tự.")]
    public string TaxCode { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại hotline nhà xe.")]
    [StringLength(24, MinimumLength = 8, ErrorMessage = "Số điện thoại hotline không hợp lệ.")]
    public string Hotline { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email người đại diện pháp luật.")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
    [StringLength(256, ErrorMessage = "Email tối đa 256 ký tự.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng tải lên tệp Giấy phép kinh doanh vận tải.")]
    public string BusinessLicensePath { get; init; } = string.Empty;
}

public sealed class SeatTemplateRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên mẫu sơ đồ ghế.")]
    [StringLength(100, ErrorMessage = "Tên mẫu tối đa 100 ký tự.")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập loại xe áp dụng.")]
    [StringLength(80, ErrorMessage = "Loại xe tối đa 80 ký tự.")]
    public string VehicleType { get; init; } = string.Empty;

    [Range(1, 2, ErrorMessage = "Số tầng xe chỉ có thể là 1 hoặc 2 tầng.")]
    public int DeckCount { get; init; } = 1;

    [Range(1, 60, ErrorMessage = "Số ghế mỗi tầng từ 1 đến 60 ghế.")]
    public int SeatsPerDeck { get; init; } = 16;
}

public sealed class VehicleRequest
{
    [Required(ErrorMessage = "Vui lòng nhập biển số xe.")]
    [RegularExpression("^[0-9A-Za-z.-]{5,16}$", ErrorMessage = "Biển số xe không hợp lệ (Ví dụ: 51B-123.45 hoặc 29B-999.88).")]
    public string PlateNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập chủng loại xe.")]
    [StringLength(80, ErrorMessage = "Chủng loại xe tối đa 80 ký tự.")]
    public string VehicleType { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn mẫu sơ đồ ghế áp dụng.")]
    public Guid SeatTemplateId { get; init; }
}

public sealed class DriverRequest
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên tài xế.")]
    [StringLength(160, ErrorMessage = "Họ tên tài xế tối đa 160 ký tự.")]
    public string FullName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại tài xế.")]
    [Phone(ErrorMessage = "Số điện thoại tài xế không hợp lệ.")]
    [StringLength(24, ErrorMessage = "Số điện thoại tối đa 24 ký tự.")]
    public string Phone { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số giấy phép lái xe.")]
    [StringLength(40, ErrorMessage = "Số giấy phép lái xe tối đa 40 ký tự.")]
    public string LicenseNumber { get; init; } = string.Empty;
}

public sealed class TripRequest
{
    [Required(ErrorMessage = "Vui lòng chọn tuyến đường.")]
    public Guid RouteId { get; init; }

    [Required(ErrorMessage = "Vui lòng chọn phương tiện vận tải.")]
    public Guid VehicleId { get; init; }

    [Required(ErrorMessage = "Vui lòng chọn tài xế.")]
    public Guid DriverId { get; init; }

    [Required(ErrorMessage = "Vui lòng chọn thời gian khởi hành.")]
    public DateTime DepartureTimeUtc { get; init; }

    [Required(ErrorMessage = "Vui lòng chọn thời gian đến dự kiến.")]
    public DateTime ArrivalTimeUtc { get; init; }

    [Range(typeof(decimal), "10000", "100000000", ErrorMessage = "Giá vé niêm yết từ 10,000 đ đến 100,000,000 đ.")]
    public decimal TicketPrice { get; init; }
}

public sealed class TripScheduleRequest
{
    [Required(ErrorMessage = "Vui lòng chọn tuyến đường.")]
    public Guid RouteId { get; init; }

    [Required(ErrorMessage = "Vui lòng chọn phương tiện.")]
    public Guid VehicleId { get; init; }

    [Required(ErrorMessage = "Vui lòng chọn tài xế.")]
    public Guid DriverId { get; init; }

    [Required(ErrorMessage = "Vui lòng chọn thời gian khởi hành.")]
    public DateTime DepartureTimeUtc { get; init; }

    [Required(ErrorMessage = "Vui lòng chọn thời gian đến dự kiến.")]
    public DateTime ArrivalTimeUtc { get; init; }
}

public sealed class PriceChangeRequest
{
    [Range(typeof(decimal), "10000", "100000000", ErrorMessage = "Giá vé mới phải từ 10,000 đ đến 100,000,000 đ.")]
    public decimal NewPrice { get; init; }
}

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

public interface IPrivateLicenseStore
{
    Task<string> SaveAsync(Stream content, string extension, CancellationToken ct);
    Task<(Stream Content, string ContentType, string DownloadName)?> OpenAsync(string storedPath, CancellationToken ct);
}

public interface IActivationEmailSender
{
    Task SendAsync(string recipient, string activationUrl, CancellationToken ct);
}

public sealed class OperatorConflictException(string message) : Exception(message);
public sealed class OperatorNotFoundException(string message) : Exception(message);
