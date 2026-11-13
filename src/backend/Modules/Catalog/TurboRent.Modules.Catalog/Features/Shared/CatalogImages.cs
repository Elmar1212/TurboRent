using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using TurboRent.Modules.Catalog.Infrastructure.Storage;

namespace TurboRent.Modules.Catalog.Features.Shared;

public static class CatalogImages
{
    public static string PublicUrl(Guid listingId, Guid imageId) =>
        $"/api/v1/catalog/listings/{listingId}/images/{imageId}";

    public static string OwnerUrl(Guid listingId, Guid imageId) =>
        $"/api/v1/owner/listings/{listingId}/images/{imageId}";

    public static string AdminUrl(Guid listingId, Guid imageId) =>
        $"/api/v1/admin/listings/{listingId}/images/{imageId}";

    /// <summary>
    /// Streams a stored image without buffering it in memory. Image objects never change
    /// (edits create new ids), so the id is a strong ETag. Public copies are cached for a day,
    /// which bounds how long an image stays visible after its listing is hidden.
    /// </summary>
    public static async Task<IResult> StreamAsync(HttpContext http, IVehicleImageStorage storage,
        Guid imageId, string objectKey, string contentType, bool isPublic, CancellationToken cancellationToken)
    {
        var etag = new EntityTagHeaderValue($"\"{imageId:N}\"");
        http.Response.Headers.CacheControl = isPublic
            ? "public, max-age=86400"
            : "private, max-age=600";
        http.Response.Headers["X-Content-Type-Options"] = "nosniff";
        http.Response.Headers["Content-Security-Policy"] = "default-src 'none'; sandbox";
        var ifNoneMatch = http.Request.Headers.IfNoneMatch.ToString();
        if (ifNoneMatch.Contains(etag.Tag.ToString(), StringComparison.Ordinal))
        {
            http.Response.Headers.ETag = etag.ToString();
            return Results.StatusCode(StatusCodes.Status304NotModified);
        }
        var stream = await storage.OpenReadAsync(objectKey, cancellationToken);
        if (stream is null) return Results.NotFound();
        return Results.Stream(stream, contentType, entityTag: etag);
    }
}
