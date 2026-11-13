using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmartTicket.Application.Operations;

namespace SmartTicket.Tests;

public sealed class OfflineTicketVerifierTests
{
    private const string SigningKey = "unit-test-ticket-signing-key-32-bytes-minimum";
    private static readonly DateTime ScannedAt = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void TryVerify_ValidTicket_ReturnsClaims()
    {
        var seatId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var token = CreateToken(seatId, tripId, "A01", ScannedAt.AddHours(1));

        var valid = OfflineTicketVerifier.TryVerify(token, SigningKey, ScannedAt, out var claims);

        Assert.True(valid);
        Assert.Equal(seatId, claims!.SeatId);
        Assert.Equal(tripId, claims.TripId);
        Assert.Equal("A01", claims.SeatCode);
    }

    [Fact]
    public void TryVerify_TamperedSignature_IsRejected()
    {
        var token = CreateToken(Guid.NewGuid(), Guid.NewGuid(), "A01", ScannedAt.AddHours(1));
        var parts = token.Split('.');
        parts[2] = (parts[2][0] == 'A' ? "B" : "A") + parts[2][1..];
        var tampered = string.Join('.', parts);

        Assert.False(OfflineTicketVerifier.TryVerify(tampered, SigningKey, ScannedAt, out var claims));
        Assert.Null(claims);
    }

    [Fact]
    public void TryVerify_ExpiredTicket_IsRejected()
    {
        var token = CreateToken(Guid.NewGuid(), Guid.NewGuid(), "A01", ScannedAt.AddSeconds(-1));

        Assert.False(OfflineTicketVerifier.TryVerify(token, SigningKey, ScannedAt, out var claims));
        Assert.Null(claims);
    }

    [Fact]
    public void TryVerify_SigningKeyShorterThan32Bytes_IsRejected()
    {
        var token = CreateToken(Guid.NewGuid(), Guid.NewGuid(), "A01", ScannedAt.AddHours(1));

        Assert.False(OfflineTicketVerifier.TryVerify(token, "short", ScannedAt, out var claims));
        Assert.Null(claims);
    }

    private static string CreateToken(Guid seatId, Guid tripId, string seatCode, DateTime expiresAtUtc)
    {
        static string Encode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var header = Encode(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var payload = Encode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = seatId.ToString("N"),
            trip = tripId,
            seat = seatCode,
            exp = new DateTimeOffset(expiresAtUtc).ToUnixTimeSeconds()
        }));
        var body = header + "." + payload;
        var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(SigningKey), Encoding.ASCII.GetBytes(body));
        return body + "." + Encode(signature);
    }
}
