namespace PotatoAccounting.Infrastructure.Services;

public class StorageOptions
{
    /// <summary>
    /// Where uploaded files live on disk. Either an absolute path
    /// (e.g. "D:\PotatoData\uploads") or a path relative to ContentRoot
    /// (e.g. "App_Data/uploads"). Kept outside wwwroot so deployments
    /// that wipe the published bundle don't take user data with them.
    /// </summary>
    public string UploadsPath { get; set; } = "App_Data/uploads";

    /// <summary>
    /// Public URL prefix the uploads are served at. The frontend stores
    /// values like "/uploads/farmers/abc.jpg" so changing this requires
    /// a data migration.
    /// </summary>
    public string PublicUrlPrefix { get; set; } = "/uploads";
}
