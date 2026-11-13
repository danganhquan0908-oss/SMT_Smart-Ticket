using System.Globalization;
using System.Text;
using SmartTicket.Infrastructure.Payments;

namespace SmartTicket.Tests;

public sealed class NapasQrPayloadBuilderTests
{
    [Fact]
    public void Build_GeneratesValidEmvCoVietQrWithReferenceAmountAndCrc16()
    {
        var payload = NapasQrPayloadBuilder.Build(
            bankBin: "970415",
            accountNumber: "0000000000",
            accountName: "Sample Account",
            amount: 250000m,
            reference: "STKTAB12CD34");

        Assert.StartsWith("000201010212", payload, StringComparison.Ordinal);
        Assert.Contains("970415", payload, StringComparison.Ordinal);
        Assert.Contains("0000000000", payload, StringComparison.Ordinal);
        Assert.Contains("5303704", payload, StringComparison.Ordinal);
        Assert.Contains("5406250000", payload, StringComparison.Ordinal);
        Assert.Contains("5914SAMPLE ACCOUNT", payload, StringComparison.Ordinal);
        Assert.Contains("STKTAB12CD34", payload, StringComparison.Ordinal);

        var crcPrefixIndex = payload.Length - 8;
        Assert.True(crcPrefixIndex > 0);
        Assert.Equal("6304", payload.Substring(crcPrefixIndex, 4));

        var dataWithoutCrcValue = payload[..(payload.Length - 4)];
        var providedCrc = payload[(payload.Length - 4)..];
        var expectedCrc = ComputeCrc16CcittFalse(dataWithoutCrcValue).ToString("X4", CultureInfo.InvariantCulture);

        Assert.Equal(expectedCrc, providedCrc);
    }

    [Fact]
    public void Build_WithInvalidBankOrAccount_ThrowsInsteadOfUsingPersonalDefaults()
    {
        Assert.Throws<ArgumentException>(() => NapasQrPayloadBuilder.Build("invalid", "0000000000", "Sample Account", 10000m, "STKT1"));
        Assert.Throws<ArgumentException>(() => NapasQrPayloadBuilder.Build("970415", "", "Sample Account", 10000m, "STKT1"));
    }

    private static ushort ComputeCrc16CcittFalse(string input)
    {
        ushort crc = 0xFFFF;
        foreach (var b in Encoding.ASCII.GetBytes(input))
        {
            crc ^= (ushort)(b << 8);
            for (var bit = 0; bit < 8; bit++)
            {
                crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1);
            }
        }
        return crc;
    }
}
