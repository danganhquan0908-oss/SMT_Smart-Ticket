namespace BusGoSmartTicket.Services.Interfaces.Operators;

public interface IPrivateLicenseStore
{
    Task<string> SaveAsync(Stream content, string extension, CancellationToken ct);
    Task<(Stream Content, string ContentType, string DownloadName)?> OpenAsync(string storedPath, CancellationToken ct);
}
