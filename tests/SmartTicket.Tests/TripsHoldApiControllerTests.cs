using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartTicket.Application.Bookings;
using SmartTicket.Application.Payments;
using SmartTicket.Infrastructure.Payments;
using SmartTicket.Web.Controllers.Api;
using SmartTicket.Web.Hubs;

namespace SmartTicket.Tests;

public sealed class TripsHoldApiControllerTests
{
    [Fact]
    public async Task Hold_ReturnsCreatedWithBase64PngQrImageAndBroadcastsHoldingStatus()
    {
        var tripId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(10);
        var holdResult = new BookingHoldResult(bookingId, "AB12CD34", expiresAt, 250000m, ["A01"], "tok-123");
        var qrPayload = NapasQrPayloadBuilder.Build("970415", "0000000000", "Sample Account", 250000m, "STKTAB12CD34");

        var bookings = new FakeBookingService(holdResult);
        var payments = new FakePaymentService(new PaymentCheckoutDto(bookingId, "AB12CD34", 250000m, expiresAt, qrPayload, "tok-123"));
        var hub = new FakeHubContext();
        var controller = new TripsController(bookings, payments, hub);

        var actionResult = await controller.Hold(
            tripId,
            new HoldSeatsRequest("Đặng Anh Quân", "0345588495", "test@example.com", ["A01"]),
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal("/booking/AB12CD34", created.Location);

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(created.Value));
        var root = doc.RootElement;
        Assert.Equal("AB12CD34", root.GetProperty("BookingCode").GetString());
        Assert.Equal(qrPayload, root.GetProperty("qrPayload").GetString());
        Assert.Equal("/payments/checkout?token=tok-123", root.GetProperty("checkoutUrl").GetString());

        var qrImage = root.GetProperty("qrImage").GetString();
        Assert.NotNull(qrImage);
        Assert.StartsWith("data:image/png;base64,", qrImage!, StringComparison.Ordinal);
        var pngBytes = Convert.FromBase64String(qrImage!["data:image/png;base64,".Length..]);
        Assert.True(pngBytes.Length > 100);
        Assert.Equal([0x89, 0x50, 0x4E, 0x47], pngBytes[..4]);

        Assert.Single(hub.SentMessages);
        Assert.Equal(TripSeatsHub.Group(tripId), hub.SentMessages[0].GroupName);
        Assert.Equal("SeatsChanged", hub.SentMessages[0].Method);
    }

    [Fact]
    public async Task Hold_WhenCheckoutGenerationFails_StillReturnsCreatedWithNullQrFallback()
    {
        var tripId = Guid.NewGuid();
        var holdResult = new BookingHoldResult(Guid.NewGuid(), "XY99ZZ11", DateTime.UtcNow.AddMinutes(10), 180000m, ["B02"], "tok-fallback");
        var controller = new TripsController(
            new FakeBookingService(holdResult),
            new ThrowingCheckoutPaymentService(),
            new FakeHubContext());

        var actionResult = await controller.Hold(
            tripId,
            new HoldSeatsRequest("Nguyen Van A", "0901234567", null, ["B02"]),
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(actionResult);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(created.Value));
        var root = doc.RootElement;
        Assert.Equal("XY99ZZ11", root.GetProperty("BookingCode").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("qrImage").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("qrPayload").ValueKind);
    }

    private sealed class FakeBookingService(BookingHoldResult holdResult) : IBookingService
    {
        public Task<IReadOnlyList<TripSearchResult>> SearchAsync(TripSearchQuery query, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<TripSearchResult>>([]);

        public Task<IReadOnlyList<SeatDto>> GetSeatsAsync(Guid tripId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<SeatDto>>([]);

        public Task<BookingHoldResult> HoldSeatsAsync(Guid tripId, HoldSeatsRequest request, CancellationToken ct) =>
            Task.FromResult(holdResult);
    }

    private sealed class FakePaymentService(PaymentCheckoutDto checkout) : IPaymentService
    {
        public Task<PaymentCheckoutDto> CreateCheckoutAsync(string lookupToken, CancellationToken ct) =>
            Task.FromResult(checkout);

        public Task<PaymentStateDto?> GetPaymentStateAsync(string lookupToken, CancellationToken ct) =>
            Task.FromResult<PaymentStateDto?>(null);

        public Task<WebhookProcessResult> ProcessWebhookAsync(PaymentWebhookPayload payload, string rawPayload, CancellationToken ct) =>
            Task.FromResult(new WebhookProcessResult(false, "noop"));

        public Task<TicketLookupDto?> GetTicketsAsync(string lookupToken, CancellationToken ct) =>
            Task.FromResult<TicketLookupDto?>(null);
    }

    private sealed class ThrowingCheckoutPaymentService : IPaymentService
    {
        public Task<PaymentCheckoutDto> CreateCheckoutAsync(string lookupToken, CancellationToken ct) =>
            Task.FromException<PaymentCheckoutDto>(new InvalidOperationException("Simulated checkout failure"));

        public Task<PaymentStateDto?> GetPaymentStateAsync(string lookupToken, CancellationToken ct) =>
            Task.FromResult<PaymentStateDto?>(null);

        public Task<WebhookProcessResult> ProcessWebhookAsync(PaymentWebhookPayload payload, string rawPayload, CancellationToken ct) =>
            Task.FromResult(new WebhookProcessResult(false, "noop"));

        public Task<TicketLookupDto?> GetTicketsAsync(string lookupToken, CancellationToken ct) =>
            Task.FromResult<TicketLookupDto?>(null);
    }

    private sealed record SentSignalRMessage(string GroupName, string Method, object?[] Args);

    private sealed class FakeHubContext : IHubContext<TripSeatsHub>
    {
        private readonly FakeHubClients _clients = new();
        public List<SentSignalRMessage> SentMessages => _clients.Messages;
        public IHubClients Clients => _clients;
        public IGroupManager Groups { get; } = new NoopGroupManager();
    }

    private sealed class FakeHubClients : IHubClients
    {
        public List<SentSignalRMessage> Messages { get; } = [];
        public IClientProxy All => new RecordingClientProxy("*", Messages);
        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => All;
        public IClientProxy Client(string connectionId) => All;
        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => All;
        public IClientProxy Group(string groupName) => new RecordingClientProxy(groupName, Messages);
        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => Group(groupName);
        public IClientProxy Groups(IReadOnlyList<string> groupNames) => All;
        public IClientProxy User(string userId) => All;
        public IClientProxy Users(IReadOnlyList<string> userIds) => All;
    }

    private sealed class RecordingClientProxy(string groupName, List<SentSignalRMessage> messages) : IClientProxy
    {
        public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
        {
            messages.Add(new SentSignalRMessage(groupName, method, args));
            return Task.CompletedTask;
        }
    }

    private sealed class NoopGroupManager : IGroupManager
    {
        public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
