using Microsoft.AspNetCore.Hosting;
using PotatoAccounting.Application.Common.Interfaces;

namespace PotatoAccounting.Infrastructure.Services;

public class LocalFileStorage : IFileStorage
{
    private const string PublicRoot = "/uploads";
    private readonly IWebHostEnvironment _env;

    public LocalFileStorage(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveAsync(string subfolder, string originalFileName, Stream content, CancellationToken cancellationToken = default)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var safeSubfolder = subfolder.Trim('/').Replace("..", "");
        var folderPath = Path.Combine(webRoot, "uploads", safeSubfolder);
        Directory.CreateDirectory(folderPath);

        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrEmpty(ext)) ext = ".bin";

        var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var fullPath = Path.Combine(folderPath, fileName);

        await using var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
        await content.CopyToAsync(fs, cancellationToken);

        return $"{PublicRoot}/{safeSubfolder}/{fileName}";
    }

    public void Delete(string? publicRelativeUrl)
    {
        if (string.IsNullOrWhiteSpace(publicRelativeUrl)) return;
        if (!publicRelativeUrl.StartsWith(PublicRoot, StringComparison.OrdinalIgnoreCase)) return;

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var relative = publicRelativeUrl.TrimStart('/').Replace("..", "");
        var fullPath = Path.Combine(webRoot, relative);

        try
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
        catch
        {
            // best-effort delete; ignore IO errors
        }
    }
}
