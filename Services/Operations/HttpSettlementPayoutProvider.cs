using BusGoSmartTicket.Models.DTOs.Operations;
using BusGoSmartTicket.Services.Interfaces.Operations;

namespace BusGoSmartTicket.Services.Operations;

public sealed class HttpSettlementPayoutProvider(HttpClient http, IConfiguration configuration, ILogger<HttpSettlementPayoutProvider> logger) : ISettlementPayoutProvider
{
    private string? Endpoint => configuration["Payouts:Endpoint"];
    private string? ApiKey => configuration["Payouts:ApiKey"];
    public bool IsConfigured => Uri.TryCreate(Endpoint, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps && !string.IsNullOrWhiteSpace(ApiKey);

    public async Task<PayoutResult> TransferAsync(PayoutInstruction instruction, CancellationToken ct)
    {
        if (!IsConfigured)
            return new PayoutResult(false, null, "Payout gateway chưa được cấu hình.");

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
        request.Headers.Add("Idempotency-Key", instruction.SettlementId.ToString("N"));
        request.Content = JsonContent.Create(new
        {
            reference = instruction.Reference,
            amount = instruction.Amount,
            bankBin = instruction.BankBin,
            accountNumber = instruction.AccountNumber,
            accountName = instruction.AccountName
        });
        try
        {
            using var response = await http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Payout gateway returned status {StatusCode} for settlement {SettlementId}.", (int)response.StatusCode, instruction.SettlementId);
                return new PayoutResult(false, null, $"Payout gateway trả HTTP {(int)response.StatusCode}.");
            }
            var body = await response.Content.ReadFromJsonAsync<PayoutGatewayResponse>(cancellationToken: ct);
            return body is { Succeeded: true } && !string.IsNullOrWhiteSpace(body.TransactionReference)
                ? new PayoutResult(true, body.TransactionReference, null)
                : new PayoutResult(false, null, "Payout gateway không xác nhận mã giao dịch.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Payout gateway request failed for settlement {SettlementId}.", instruction.SettlementId);
            return new PayoutResult(false, null, "Không kết nối được payout gateway.");
        }
    }

    private sealed record PayoutGatewayResponse(bool Succeeded, string? TransactionReference);
}
