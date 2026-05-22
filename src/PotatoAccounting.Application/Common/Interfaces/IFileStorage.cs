namespace PotatoAccounting.Application.Common.Interfaces;

public interface IFileStorage
{
    /// <summary>
    /// Saves a file under the given subfolder and returns a public-relative URL (e.g. "/uploads/farmers/abc.jpg").
    /// </summary>
    Task<string> SaveAsync(string subfolder, string originalFileName, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a previously saved file by its public-relative URL. Silent if file doesn't exist.
    /// </summary>
    void Delete(string? publicRelativeUrl);
}
