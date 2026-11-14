using BusGoSmartTicket.Services.Interfaces.Operators;

namespace BusGoSmartTicket.Services.Operators;

public sealed class PrivateLicenseStore(IHostEnvironment environment) : IPrivateLicenseStore
{
    private readonly string _root = Path.Combine(environment.ContentRootPath, "App_Data", "operator-licenses");

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken ct)
    {
        Directory.CreateDirectory(_root);
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var path = Path.Combine(_root, fileName);
        await using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await content.CopyToAsync(file, ct);
        return fileName;
    }

    public Task<(Stream Content, string ContentType, string DownloadName)?> OpenAsync(string storedPath, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var name = Path.GetFileName(storedPath);
        var fullPath = Path.GetFullPath(Path.Combine(_root, name));
        var root = Path.GetFullPath(_root) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
            return Task.FromResult<(Stream, string, string)?>(null);
        var contentType = Path.GetExtension(name).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            _ => "image/jpeg"
        };
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return Task.FromResult<(Stream, string, string)?>((stream, contentType, name));
    }
}
