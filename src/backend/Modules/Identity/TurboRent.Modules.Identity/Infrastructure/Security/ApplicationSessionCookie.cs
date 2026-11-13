using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace TurboRent.Modules.Identity.Infrastructure.Security;

public sealed class ApplicationSessionCookie(IDataProtectionProvider protection)
{
    public const string Name = "__Host-TurboRent.Session";
    private readonly IDataProtector protector = protection.CreateProtector("TurboRent.Identity.Session.v1");

    public string Protect(Guid sessionId) => protector.Protect(sessionId.ToString("N"));

    public bool TryUnprotect(string? value, out Guid sessionId)
    {
        sessionId = Guid.Empty;
        if (string.IsNullOrEmpty(value)) return false;
        try { return Guid.TryParseExact(protector.Unprotect(value), "N", out sessionId); }
        catch (CryptographicException) { return false; }
    }

    public void Set(HttpContext http, Guid sessionId, DateTimeOffset expires)
    {
        http.Response.Cookies.Append(Name, Protect(sessionId), new CookieOptions
        {
            HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict,
            Path = "/", IsEssential = true, Expires = expires
        });
    }

    public void Clear(HttpContext http) => http.Response.Cookies.Delete(Name, new CookieOptions
    {
        HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict,
        Path = "/", IsEssential = true
    });
}
