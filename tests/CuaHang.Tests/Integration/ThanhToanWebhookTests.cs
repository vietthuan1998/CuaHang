using System.Net;
using System.Security.Cryptography;
using System.Text;
using CuaHang.Api.Data;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CuaHang.Tests.Integration;

public sealed class ThanhToanWebhookTests
{
    private const string Secret = "test-webhook-signing-secret-value";

    [Fact]
    public async Task Webhook_ChuKyHopLe_CapNhatDonVaKhongXuLyLap()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        await ThemDonHangMoiAsync(factory);
        const string body = """{"maThanhToanNgoai":"pay-1","donHangId":1,"trangThai":"DaThanhToan"}""";

        var first = await GuiAsync(client, body);
        var second = await GuiAsync(client, body);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(TrangThaiDon.DaThanhToan,
            await context.DonHangs.Select(x => x.TrangThai).SingleAsync());
        Assert.Equal(1, await context.ThanhToanWebhooks.CountAsync());
        Assert.Equal(1, await context.HopThuDis.CountAsync());
    }

    [Fact]
    public async Task Webhook_ChuKySai_Tra401VaKhongGhiDuLieu()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        await ThemDonHangMoiAsync(factory);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/thanh-toan")
        {
            Content = new StringContent(
                """{"maThanhToanNgoai":"pay-bad","donHangId":1,"trangThai":"DaThanhToan"}""",
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.Add("X-Payment-Signature", new string('0', 64));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(await context.ThanhToanWebhooks.ToListAsync());
    }

    private static async Task<HttpResponseMessage> GuiAsync(HttpClient client, string body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/thanh-toan")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        var signature = Convert.ToHexString(
            HMACSHA256.HashData(Encoding.UTF8.GetBytes(Secret), Encoding.UTF8.GetBytes(body)));
        request.Headers.Add("X-Payment-Signature", signature);
        return await client.SendAsync(request);
    }

    private static async Task ThemDonHangMoiAsync(ApiFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        context.DanhMucs.Add(new DanhMuc { Ten = "Danh mục" });
        context.KhachHangs.Add(new KhachHang
        {
            Ten = "Khách hàng",
            SoDienThoai = "0900000000"
        });
        await context.SaveChangesAsync();
        context.DonHangs.Add(new DonHang
        {
            KhachHangId = 1,
            SoLuong = 1,
            DonGia = 100m,
            ThanhTien = 100m,
            TrangThai = TrangThaiDon.Moi
        });
        await context.SaveChangesAsync();
    }
}
