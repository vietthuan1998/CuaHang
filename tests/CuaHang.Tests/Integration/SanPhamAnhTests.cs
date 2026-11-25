using System.Net;
using System.Net.Http.Headers;
using CuaHang.Api.Data;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CuaHang.Tests.Integration;

public sealed class SanPhamAnhTests
{
    [Fact]
    public async Task TaiAnh_PngHopLe_LuuNgoaiWebRootVaTaiLaiDuoc()
    {
        using var factory = new ApiFactory();
        try
        {
            await TaoSanPhamAsync(factory);
            using var client = factory.CreateClient();
            using var form = new MultipartFormDataContent();
            var png = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 1, 2 };
            using var content = new ByteArrayContent(png);
            content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(content, "tep", "anh.png");

            var upload = await client.PostAsync("/api/san-pham/1/anh", form);
            var download = await client.GetAsync("/api/san-pham/1/anh");

            Assert.Equal(HttpStatusCode.NoContent, upload.StatusCode);
            Assert.Equal(HttpStatusCode.OK, download.StatusCode);
            Assert.Equal("image/png", download.Content.Headers.ContentType?.MediaType);
            Assert.Equal(png, await download.Content.ReadAsByteArrayAsync());
        }
        finally
        {
            if (Directory.Exists(factory.ImagePath))
                Directory.Delete(factory.ImagePath, recursive: true);
        }
    }

    [Fact]
    public async Task TaiAnh_KhongKhopChuKy_Tra400()
    {
        using var factory = new ApiFactory();
        try
        {
            await TaoSanPhamAsync(factory);
            using var client = factory.CreateClient();
            using var form = new MultipartFormDataContent();
            using var content = new ByteArrayContent([1, 2, 3, 4]);
            content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(content, "tep", "khong-phai-anh.png");

            var response = await client.PostAsync("/api/san-pham/1/anh", form);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.False(Directory.Exists(factory.ImagePath));
        }
        finally
        {
            if (Directory.Exists(factory.ImagePath))
                Directory.Delete(factory.ImagePath, recursive: true);
        }
    }

    private static async Task TaoSanPhamAsync(ApiFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        context.DanhMucs.Add(new DanhMuc { Ten = "Danh mục" });
        await context.SaveChangesAsync();
        context.SanPhams.Add(new SanPham
        {
            DanhMucId = 1,
            Ten = "Sản phẩm",
            DonGia = 100,
            TonKho = 2
        });
        await context.SaveChangesAsync();
    }
}
