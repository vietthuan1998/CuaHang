using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace CuaHang.Tests.Integration;

public sealed class IntegrationAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.ContainsKey("X-Test-Unauthenticated"))
            return Task.FromResult(AuthenticateResult.NoResult());

        var role = Request.Headers["X-Test-Role"].FirstOrDefault() ?? "QuanTri";
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "integration-admin"),
            new Claim(ClaimTypes.Name, role == "QuanTri" ? "integration-admin" : "integration-staff"),
            new Claim(ClaimTypes.Role, role)
        };
        var userClaims = role == "QuanTri"
            ? claims.Append(new Claim("quyen", "sua_san_pham"))
            : claims;
        var identity = new ClaimsIdentity(userClaims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(principal, Scheme.Name)));
    }
}
