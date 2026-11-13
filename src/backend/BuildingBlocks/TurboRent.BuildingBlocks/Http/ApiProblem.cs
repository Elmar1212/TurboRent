using Microsoft.AspNetCore.Http;

namespace TurboRent.BuildingBlocks.Http;

/// <summary>Problem Details responses that always carry a stable machine-readable <c>code</c>.</summary>
public static class ApiProblem
{
    public static IResult Error(int status, string code) => Results.Problem(
        statusCode: status,
        title: code,
        extensions: new Dictionary<string, object?> { ["code"] = code });

    /// <summary>Marks a response as private, uncacheable and without a referrer.</summary>
    public static void NoStore(HttpContext http)
    {
        http.Response.Headers.CacheControl = "no-store";
        http.Response.Headers["Referrer-Policy"] = "no-referrer";
    }
}
