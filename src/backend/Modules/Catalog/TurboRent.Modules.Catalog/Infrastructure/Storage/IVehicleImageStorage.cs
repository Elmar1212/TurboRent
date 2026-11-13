namespace TurboRent.Modules.Catalog.Infrastructure.Storage;

public interface IVehicleImageStorage
{
    Task PutAsync(string key, Stream content, string contentType, CancellationToken cancellationToken);

    /// <summary>
    /// Opens the stored object for streaming, or returns null when it does not exist.
    /// The caller owns the stream and must dispose it.
    /// </summary>
    Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken);

    Task DeleteAsync(string key, CancellationToken cancellationToken);
}
