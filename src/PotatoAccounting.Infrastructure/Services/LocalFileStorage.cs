using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using PotatoAccounting.Application.Common.Interfaces;

namespace PotatoAccounting.Infrastructure.Services;

public class LocalFileStorage : IFileStorage
{
    private readonly string _uploadsRoot;
    private readonly string _publicPrefix;

    public LocalFileStorage(IWebHostEnvironment env, IOptions<StorageOptions> options)
    {
        var opts = options.Value;
        _uploadsRoot = ResolveUploadsRoot(env.ContentRootPath, opts.UploadsPath);
        _publicPrefix = NormalizePrefix(opts.PublicUrlPrefix);
        Directory.CreateDirectory(_uploadsRoot);
    }

    public async Task<string> SaveAsync(string subfolder, string originalFileName, Stream content, CancellationToken cancellationToken = default)
    {
        var safeSubfolder = subfolder.Trim('/').Replace("..", "");
        var folderPath = Path.Combine(_uploadsRoot, safeSubfolder);
        Directory.CreateDirectory(folderPath);

        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrEmpty(ext)) ext = ".bin";

        var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var fullPath = Path.Combine(folderPath, fileName);

        await using var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
        await content.CopyToAsync(fs, cancellationToken);

        return $"{_publicPrefix}/{safeSubfolder}/{fileName}";
    }

    public void Delete(string? publicRelativeUrl)
    {
        if (string.IsNullOrWhiteSpace(publicRelativeUrl)) return;
        if (!publicRelativeUrl.StartsWith(_publicPrefix, StringComparison.OrdinalIgnoreCase)) return;

        var relativeWithinUploads = publicRelativeUrl
            .Substring(_publicPrefix.Length)
            .TrimStart('/')
            .Replace("..", "");
        var fullPath = Path.Combine(_uploadsRoot, relativeWithinUploads);

        try
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
        catch
        {
            // best-effort delete; ignore IO errors
        }
    }

    public static string ResolveUploadsRoot(string contentRootPath, string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            configuredPath = "App_Data/uploads";
        return Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(Path.Combine(contentRootPath, configuredPath));
    }

    private static string NormalizePrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix)) return "/uploads";
        var p = prefix.Trim();
        if (!p.StartsWith('/')) p = "/" + p;
        return p.TrimEnd('/');
    }
}
