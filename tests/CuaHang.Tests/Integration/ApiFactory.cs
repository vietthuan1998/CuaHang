using CuaHang.Api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CuaHang.Tests.Integration;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"test-{Guid.NewGuid():N}";
    public string ImagePath { get; } = Path.Combine(
        Path.GetTempPath(), $"cuahang-api-test-{Guid.NewGuid():N}");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Issuer", "test-issuer");
        builder.UseSetting("Jwt:Audience", "test-audience");
        builder.UseSetting("Jwt:SigningKey", "test-signing-key-that-is-at-least-32-bytes");
        builder.UseSetting("Jwt:ThoiHanPhut", "15");
        builder.UseSetting("Webhook:SigningSecret", "test-webhook-signing-secret-value");
        builder.UseSetting("Storage:ProductImagePath", ImagePath);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Integration";
                options.DefaultChallengeScheme = "Integration";
                options.DefaultScheme = "Integration";
            }).AddScheme<AuthenticationSchemeOptions, IntegrationAuthHandler>(
                "Integration", _ => { });
        });
    }
}
