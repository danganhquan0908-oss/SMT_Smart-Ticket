using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SmartTicket.Application.Operations;

public sealed record OfflineTicketClaims(Guid SeatId, Guid TripId, string SeatCode, DateTime ExpiresAtUtc);

/// <summary>Use inside a trusted POS client to verify ticket authenticity when the station has no network.</summary>
public static class OfflineTicketVerifier
{
    public static bool TryVerify(string jwt, string signingKey, DateTime scannedAtUtc, out OfflineTicketClaims? claims)
    {
        claims = null;
        if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32) return false;
        var parts = jwt.Split('.');
        if (parts.Length != 3) return false;
        var body = parts[0] + "." + parts[1];
        byte[] supplied;
        try { supplied = Decode(parts[2]); } catch (FormatException) { return false; }
        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(signingKey), Encoding.ASCII.GetBytes(body));
        if (!CryptographicOperations.FixedTimeEquals(expected, supplied)) return false;
        try
        {
            using var header = JsonDocument.Parse(Decode(parts[0]));
            if (header.RootElement.GetProperty("alg").GetString() != "HS256" || header.RootElement.GetProperty("typ").GetString() != "JWT") return false;
            using var doc = JsonDocument.Parse(Decode(parts[1]));
            var root = doc.RootElement;
            if (!Guid.TryParseExact(root.GetProperty("sub").GetString(), "N", out var ticketId) ||
                !Guid.TryParse(root.GetProperty("trip").GetString(), out var tripId) ||
                !root.TryGetProperty("seat", out var seat) ||
                !root.TryGetProperty("exp", out var exp) || exp.GetInt64() < new DateTimeOffset(DateTime.SpecifyKind(scannedAtUtc, DateTimeKind.Utc)).ToUnixTimeSeconds()) return false;
            claims = new OfflineTicketClaims(ticketId, tripId, seat.GetString() ?? "", DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()).UtcDateTime);
            return true;
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or ArgumentOutOfRangeException or FormatException) { return false; }
    }

    private static byte[] Decode(string part) => Convert.FromBase64String(part.Replace('-', '+').Replace('_', '/') + new string('=', (4 - part.Length % 4) % 4));
}
