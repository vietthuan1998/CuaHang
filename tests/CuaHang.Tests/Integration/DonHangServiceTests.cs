using CuaHang.Api.Common;
using CuaHang.Api.Data;
using CuaHang.Api.Dtos;
using CuaHang.Api.Entities;
using CuaHang.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CuaHang.Tests.Integration;

public sealed class DonHangServiceTests
{
    [Fact]
    public async Task DatHang_GiamGiaVaTruTonTrongCungLuu()
    {
        using var factory = new ApiFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        await ThemDuLieuAsync(context, donGia: 100m, tonKho: 10);

        var service = scope.ServiceProvider.GetRequiredService<DonHangService>();
        var donHang = await service.DatHangAsync(new DatHangRequest(1, 1, 10));

        Assert.Equal(950m, donHang.ThanhTien);
        Assert.Equal(100m, donHang.DonGia);
        Assert.Equal(TrangThaiDon.Moi, donHang.TrangThai);
        Assert.Equal(0, await context.SanPhams.Select(x => x.TonKho).SingleAsync());
        var chiTiet = await context.ChiTietDonHangs.SingleAsync();
        Assert.Equal(donHang.Id, chiTiet.DonHangId);
        Assert.Equal(1, chiTiet.SanPhamId);
        Assert.Equal(950m, chiTiet.ThanhTien);

        var pdf = await scope.ServiceProvider
            .GetRequiredService<HoaDonService>()
            .TaoPdfAsync(donHang.Id);
        Assert.NotNull(pdf);
        Assert.StartsWith("%PDF-1.4", System.Text.Encoding.ASCII.GetString(pdf!));
        Assert.Contains("HOA DON DON HANG", System.Text.Encoding.ASCII.GetString(pdf));
    }

    [Fact]
    public async Task DatHang_VuotTonKho_BiTuChoiVaKhongTaoDon()
    {
        using var factory = new ApiFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        await ThemDuLieuAsync(context, donGia: 100m, tonKho: 2);

        var service = scope.ServiceProvider.GetRequiredService<DonHangService>();

        await Assert.ThrowsAsync<HetHangException>(
            () => service.DatHangAsync(new DatHangRequest(1, 1, 3)));
        Assert.Equal(0, await context.DonHangs.CountAsync());
        Assert.Equal(2, await context.SanPhams.Select(x => x.TonKho).SingleAsync());
    }

    [Fact]
    public async Task HuyDon_HoanTonVaChanHuyLapLai()
    {
        using var factory = new ApiFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        await ThemDuLieuAsync(context, donGia: 100m, tonKho: 5);
        var service = scope.ServiceProvider.GetRequiredService<DonHangService>();
        var donHang = await service.DatHangAsync(new DatHangRequest(1, 1, 3));

        await service.HuyAsync(donHang.Id);

        Assert.Equal(5, await context.SanPhams.Select(x => x.TonKho).SingleAsync());
        Assert.Equal(TrangThaiDon.DaHuy,
            await context.DonHangs.Select(x => x.TrangThai).SingleAsync());
        await Assert.ThrowsAsync<XungDotException>(() => service.HuyAsync(donHang.Id));
    }

    private static async Task ThemDuLieuAsync(
        AppDbContext context,
        decimal donGia,
        int tonKho)
    {
        context.DanhMucs.Add(new DanhMuc { Ten = "Danh mục" });
        context.KhachHangs.Add(new KhachHang
        {
            Ten = "Khách hàng",
            SoDienThoai = "0900000000"
        });
        await context.SaveChangesAsync();

        context.SanPhams.Add(new SanPham
        {
            DanhMucId = 1,
            Ten = "Sản phẩm",
            DonGia = donGia,
            TonKho = tonKho
        });
        await context.SaveChangesAsync();
    }
}
