using BusGoSmartTicket.Services.Interfaces.Operators;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BusGoSmartTicket.Services.Operators;

public sealed class SmtpActivationEmailSender(IConfiguration configuration, IHostEnvironment environment, ILogger<SmtpActivationEmailSender> logger) : IActivationEmailSender
{
    public async Task SendAsync(string recipient, string activationUrl, CancellationToken ct)
    {
        var host = configuration["Smtp:Host"];
        var from = configuration["Smtp:FromAddress"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            if (environment.IsDevelopment())
            {
                logger.LogWarning("SMTP chưa cấu hình. Link kích hoạt phát triển cho {Recipient}: {ActivationUrl}", recipient, activationUrl);
                return;
            }
            throw new InvalidOperationException("Chưa cấu hình SMTP để gửi email kích hoạt nhà xe.");
        }

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = "Kích hoạt tài khoản Smart Ticket";
        message.Body = new BodyBuilder
        {
            HtmlBody = $"<p>Hồ sơ nhà xe của bạn đã được phê duyệt.</p><p><a href=\"{System.Net.WebUtility.HtmlEncode(activationUrl)}\">Kích hoạt tài khoản</a></p><p>Liên kết có hiệu lực trong 48 giờ. Khi kích hoạt, bạn sẽ tạo mật khẩu và bật xác thực hai lớp.</p>",
            TextBody = $"Hồ sơ nhà xe đã được duyệt. Kích hoạt tài khoản trong 48 giờ: {activationUrl}"
        }.ToMessageBody();

        using var client = new SmtpClient();
        var port = configuration.GetValue("Smtp:Port", 587);
        var secure = configuration.GetValue("Smtp:UseSsl", true) ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
        await client.ConnectAsync(host, port, secure, ct);
        var username = configuration["Smtp:Username"];
        var password = configuration["Smtp:Password"];
        if (!string.IsNullOrWhiteSpace(username))
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Thiếu Smtp:Password khi cấu hình SMTP authentication.");
            await client.AuthenticateAsync(username, password, ct);
        }
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
