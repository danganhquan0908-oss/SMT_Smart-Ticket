using System.Globalization;
using System.Text;

namespace BusGoSmartTicket.Services.Payments;

public static class NapasQrPayloadBuilder
{
    public static string Build(string bankBin, string accountNumber, string accountName, decimal amount, string reference)
    {
        if (string.IsNullOrWhiteSpace(bankBin) || bankBin.Length != 6 || !bankBin.All(char.IsAsciiDigit))
            throw new ArgumentException("A valid six-digit bank BIN is required.", nameof(bankBin));
        if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length > 28 || !accountNumber.All(char.IsAsciiLetterOrDigit))
            throw new ArgumentException("A valid bank account number is required.", nameof(accountNumber));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be positive.");
        var amountText = amount.ToString("0.##", CultureInfo.InvariantCulture);
        var accountInfo = Tlv("00", bankBin) + Tlv("01", accountNumber);
        var merchantAccount = Tlv("00", "A000000727") + Tlv("01", accountInfo) + Tlv("02", "QRIBFTTA");
        var refText = string.IsNullOrWhiteSpace(reference) ? "SMARTTKT" : reference;
        var extra = Tlv("05", refText) + Tlv("08", refText);
        var merchantName = Ascii(accountName ?? "").ToUpperInvariant();
        if (merchantName.Length is < 1 or > 25)
            merchantName = "SMART TICKET";
        var body = Tlv("00", "01") + Tlv("01", "12") + Tlv("38", merchantAccount) + Tlv("53", "704") +
            Tlv("54", amountText) + Tlv("58", "VN") + Tlv("59", merchantName) + Tlv("60", "VIETNAM") + Tlv("62", extra) + "6304";
        return body + Crc16CcittFalse(body).ToString("X4", CultureInfo.InvariantCulture);
    }

    private static string Tlv(string id, string value) => id + value.Length.ToString("D2", CultureInfo.InvariantCulture) + value;
    private static string Ascii(string value) => new(value.Replace('Đ', 'D').Replace('đ', 'd').Normalize(NormalizationForm.FormD).Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark && c <= 127).ToArray());

    private static ushort Crc16CcittFalse(string input)
    {
        ushort crc = 0xFFFF;
        foreach (var b in Encoding.ASCII.GetBytes(input))
        {
            crc ^= (ushort)(b << 8);
            for (var bit = 0; bit < 8; bit++)
                crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1);
        }
        return crc;
    }
}
