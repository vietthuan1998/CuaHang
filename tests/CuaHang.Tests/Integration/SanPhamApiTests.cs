using System.Net;
using System.Net.Http.Json;
using System.Text;
using CuaHang.Api.Data;
using CuaHang.Api.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace CuaHang.Tests.Integration;

public sealed class SanPhamApiTests
{
    [Fact]
    public async Task TaoSanPham_HopLe_Tra201VaLocation()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        await ThemDanhMucAsync(factory);

        var response = await client.PostAsJsonAsync("/api/san-pham", new
        {
            ten = "Cà phê",
            donGia = 25000m,
            tonKho = 10,
            danhMucId = 1,
            ghiChu = "Lon"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith("/api/san-pham/1", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task TaoSanPham_DanhMucKhongTonTai_Tra400()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/san-pham", new
        {
            ten = "Cà phê",
            donGia = 25000m,
            tonKho = 10,
            danhMucId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TaoSanPham_JsonHong_Tra400()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        using var content = new StringContent("{", Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/san-pham", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DanhSach_PageSizeQuaGioiHan_Tra400()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/san-pham?pageSize=201");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ApiSanPham_KhongCoDanhTinh_Tra401()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Unauthenticated", "true");

        var response = await client.GetAsync("/api/san-pham");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ApiSanPham_NhanVienKhongCoQuyenSua_Tra403()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "NhanVien");

        var response = await client.PostAsJsonAsync("/api/san-pham", new
        {
            ten = "Cà phê",
            donGia = 25000m,
            tonKho = 10,
            danhMucId = 1
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CORS_OriginDuocChoPhep_CoHeaderNguon()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/san-pham");
        request.Headers.Add("Origin", "http://localhost:5158");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        Assert.Equal("http://localhost:5158",
            response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task CORS_OriginKhongDuocChoPhep_KhongCoHeaderNguon()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/san-pham");
        request.Headers.Add("Origin", "https://not-allowed.example");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task NenPhanHoi_AcceptGzip_KichThuocNhoHon()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        await ThemDanhMucAsync(factory);
        await ThemNhieuSanPhamAsync(factory);

        var plainResponse = await client.GetAsync("/api/san-pham");
        var plainBody = await plainResponse.Content.ReadAsByteArrayAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/san-pham");
        request.Headers.AcceptEncoding.ParseAdd("gzip");

        var compressedResponse = await client.SendAsync(request);
        var compressedBody = await compressedResponse.Content.ReadAsByteArrayAsync();

        Assert.Contains("gzip", compressedResponse.Content.Headers.ContentEncoding);
        Assert.True(compressedBody.Length < plainBody.Length);
    }

    private static async Task ThemDanhMucAsync(ApiFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        context.DanhMucs.Add(new DanhMuc { Ten = "Đồ uống" });
        await context.SaveChangesAsync();
    }

    private static async Task ThemNhieuSanPhamAsync(ApiFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.SanPhams.AddRange(Enumerable.Range(1, 20).Select(i => new SanPham
        {
            DanhMucId = 1,
            Ten = $"Sản phẩm {i}",
            DonGia = 1000m,
            TonKho = i,
            GhiChu = new string('a', 300)
        }));
        await context.SaveChangesAsync();
    }
}
