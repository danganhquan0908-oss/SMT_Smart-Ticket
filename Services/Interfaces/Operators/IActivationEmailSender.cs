namespace BusGoSmartTicket.Services.Interfaces.Operators;

public interface IActivationEmailSender
{
    Task SendAsync(string recipient, string activationUrl, CancellationToken ct);
}
