using System.Net;
using System.Net.Http.Json;

namespace CuaHang.Tests.Integration;

public sealed class ThuNghiemApiTests
{
    [Fact]
    public async Task HealthVaThuNghiem_ChayKhongCanDangNhap()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Unauthenticated", "true");

        var health = await client.GetAsync("/api/health");
        var chao = await client.GetAsync("/api/thu-nghiem/chao");
        var lapLai = await client.GetAsync("/api/thu-nghiem/lap-lai?noiDung=hello");

        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        Assert.Equal("xin chao", await chao.Content.ReadAsStringAsync());
        Assert.Equal("hello", await lapLai.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task DangNhap_VuotHanMuc_Tra429()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        HttpResponseMessage? last = null;
        for (var i = 0; i < 6; i++)
        {
            last?.Dispose();
            last = await client.PostAsJsonAsync("/api/xac-thuc/dang-nhap", new
            {
                tenDangNhap = "khong-co",
                matKhau = "mat-khau-thu-nghiem"
            });
        }

        Assert.NotNull(last);
        Assert.Equal(HttpStatusCode.TooManyRequests, last.StatusCode);
        last.Dispose();
    }
}
