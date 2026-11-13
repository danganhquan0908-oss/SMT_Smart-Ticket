using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using SmartTicket.Application.Payments;
using SmartTicket.Infrastructure.Persistence;
using SmartTicket.Web.Controllers.Api;
using SmartTicket.Web.Hubs;

namespace SmartTicket.Tests;

public sealed class PaymentsWebhookControllerTests
{
    [Fact]
    public void WebhookHealth_ReturnsOkWithReadyStatus()
    {
        var controller = CreateController(new FakePaymentService(), new Dictionary<string, string?>(), Environments.Production);

        var result = controller.WebhookHealth();

        var ok = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(ok.Value);
        Assert.Contains("\"status\":\"ready\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Webhook_WithSePayPayloadAndValidApiKey_ProcessesNormalizedReferenceAndBroadcastsSeats()
    {
        var tripId = Guid.NewGuid();
        var payments = new FakePaymentService
        {
            WebhookResultToReturn = new WebhookProcessResult(true, "paid", tripId, ["A01", "A02"])
        };
        var hub = new FakeHubContext();
        var controller = CreateController(
            payments,
            new Dictionary<string, string?>
            {
                ["Payments:SePayApiKey"] = "sepay-live-api-key",
                ["Payments:WebhookSecret"] = "smartticket-secret-key-at-least-32-chars-long!"
            },
            Environments.Production,
            hub);

        var sePayJson = """
        {
          "id": 92704,
          "gateway": "VietinBank",
          "transactionDate": "2026-11-12 14:00:00",
          "accountNumber": "0000000000",
          "code": null,
          "content": "Thanh toan ve xe stkt Ab12Cd34 qua QR",
          "transferType": "in",
          "transferAmount": 250000,
          "referenceCode": "MBVCB.92704"
        }
        """;

        SetRequest(controller, sePayJson, ("Authorization", "Apikey sepay-live-api-key"));

        var actionResult = await controller.Webhook(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(actionResult);
        var responseJson = JsonSerializer.Serialize(ok.Value);
        Assert.Contains("\"Processed\":true", responseJson, StringComparison.Ordinal);
        Assert.Contains("\"reference\":\"STKTAB12CD34\"", responseJson, StringComparison.Ordinal);

        Assert.NotNull(payments.LastPayload);
        Assert.Equal("STKTAB12CD34", payments.LastPayload!.Reference);
        Assert.Equal("92704", payments.LastPayload.TransactionId);
        Assert.Equal(250000m, payments.LastPayload.Amount);
        Assert.Equal("SUCCESS", payments.LastPayload.Status);

        Assert.Single(hub.SentMessages);
        Assert.Equal(TripSeatsHub.Group(tripId), hub.SentMessages[0].GroupName);
        Assert.Equal("SeatsChanged", hub.SentMessages[0].Method);
    }

    [Fact]
    public async Task Webhook_WithSePayPayloadWithoutStktReference_Returns200IgnoredAndDoesNotProcess()
    {
        var payments = new FakePaymentService();
        var controller = CreateController(
            payments,
            new Dictionary<string, string?>
            {
                ["Payments:SePayApiKey"] = "sepay-live-api-key"
            },
            Environments.Production);

        var sePayUnrelatedJson = """
        {
          "id": 10001,
          "gateway": "VietinBank",
          "accountNumber": "0000000000",
          "content": "Chuyen tien an trua",
          "transferType": "in",
          "transferAmount": 50000
        }
        """;

        SetRequest(controller, sePayUnrelatedJson, ("Authorization", "Apikey sepay-live-api-key"));

        var actionResult = await controller.Webhook(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(actionResult);
        var json = JsonSerializer.Serialize(ok.Value);
        Assert.Contains("ignored_no_reference", json, StringComparison.Ordinal);
        Assert.Null(payments.LastPayload);
    }

    [Fact]
    public async Task Webhook_InProductionWithInvalidApiKey_Returns401Unauthorized()
    {
        var payments = new FakePaymentService();
        var controller = CreateController(
            payments,
            new Dictionary<string, string?>
            {
                ["Payments:SePayApiKey"] = "expected-sepay-key"
            },
            Environments.Production);

        SetRequest(controller, """{"transferType":"in","transferAmount":10000,"content":"STKT1234"}""", ("Authorization", "Apikey wrong-key"));

        var actionResult = await controller.Webhook(CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(actionResult);
        Assert.Null(payments.LastPayload);
    }

    [Fact]
    public async Task Webhook_InProductionWithoutAuthenticationConfiguration_Returns503()
    {
        var controller = CreateController(new FakePaymentService(), new Dictionary<string, string?>(), Environments.Production);
        SetRequest(controller, "{}" );

        var actionResult = await controller.Webhook(CancellationToken.None);

        Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, ((ObjectResult)actionResult).StatusCode);
    }

    [Fact]
    public async Task Webhook_WithValidHmacSignature_AuthenticatesAndProcesses()
    {
        const string secret = "smartticket-secret-key-at-least-32-chars-long!";
        var payments = new FakePaymentService();
        var controller = CreateController(
            payments,
            new Dictionary<string, string?>
            {
                ["Payments:WebhookSecret"] = secret
            },
            Environments.Production);

        var body = """{"Reference":"STKT998877","TransactionId":"TX-1","Amount":150000,"Status":"SUCCESS"}""";
        var hmac = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
        SetRequest(controller, body, ("X-Signature", $"sha256={hmac}"));

        var actionResult = await controller.Webhook(CancellationToken.None);

        Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(payments.LastPayload);
        Assert.Equal("STKT998877", payments.LastPayload!.Reference);
    }

    private static PaymentsWebhookController CreateController(
        FakePaymentService payments,
        Dictionary<string, string?> settings,
        string environmentName,
        FakeHubContext? hub = null)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        var db = new SmartTicketDbContext(new DbContextOptionsBuilder<SmartTicketDbContext>().Options);
        var env = new FakeHostEnvironment { EnvironmentName = environmentName };

        return new PaymentsWebhookController(
            payments,
            config,
            hub ?? new FakeHubContext(),
            env,
            db,
            NullLogger<PaymentsWebhookController>.Instance);
    }

    private static void SetRequest(ControllerBase controller, string body, params (string Name, string Value)[] headers)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/payments/webhook";
        httpContext.Request.Method = "POST";
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        foreach (var (name, value) in headers)
        {
            httpContext.Request.Headers[name] = value;
        }
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    private sealed class FakePaymentService : IPaymentService
    {
        public PaymentWebhookPayload? LastPayload { get; private set; }
        public WebhookProcessResult WebhookResultToReturn { get; set; } = new(true, "paid");

        public Task<PaymentCheckoutDto> CreateCheckoutAsync(string lookupToken, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<PaymentStateDto?> GetPaymentStateAsync(string lookupToken, CancellationToken ct) =>
            Task.FromResult<PaymentStateDto?>(null);

        public Task<WebhookProcessResult> ProcessWebhookAsync(PaymentWebhookPayload payload, string rawPayload, CancellationToken ct)
        {
            LastPayload = payload;
            return Task.FromResult(WebhookResultToReturn);
        }

        public Task<TicketLookupDto?> GetTicketsAsync(string lookupToken, CancellationToken ct) =>
            Task.FromResult<TicketLookupDto?>(null);
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "SmartTicket.Web";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
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
