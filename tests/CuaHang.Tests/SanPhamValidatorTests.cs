using CuaHang.Api.Dtos;
using CuaHang.Api.Validation;

namespace CuaHang.Tests;

public class SanPhamValidatorTests
{
    [Fact]
    public void DuLieuHopLe_KhongCoLoi()
    {
        var loi = SanPhamValidator.Kiem(new SanPhamRequest("Cà phê", 25000m, 10, 1, "Lon"));
        Assert.Empty(loi);
    }

    [Fact]
    public void TonKhoBangKhong_DuocChapNhan()
    {
        var loi = SanPhamValidator.Kiem(new SanPhamRequest("Cà phê", 0m, 0, 1, null));
        Assert.Empty(loi);
    }

    [Fact]
    public void TenVaGhiChuQuaGioiHan_BiTuChoi()
    {
        var loi = SanPhamValidator.Kiem(new SanPhamRequest(new string('a', 201), 1m, 1, 1, new string('b', 501)));
        Assert.Contains(loi, x => x.Truong == "ten");
        Assert.Contains(loi, x => x.Truong == "ghiChu");
    }

    [Fact]
    public void ThieuTruongVaGiaTriAm_BiTuChoi()
    {
        var loi = SanPhamValidator.Kiem(new SanPhamRequest(" ", -1m, -1, null, null));
        Assert.Contains(loi, x => x.Truong == "ten");
        Assert.Contains(loi, x => x.Truong == "donGia");
        Assert.Contains(loi, x => x.Truong == "tonKho");
        Assert.Contains(loi, x => x.Truong == "danhMucId");
    }

    [Fact]
    public void GiaCoHonHaiSoLe_BiTuChoi()
    {
        var loi = SanPhamValidator.Kiem(new SanPhamRequest("Cà phê", 1.001m, 1, 1, null));
        Assert.Contains(loi, x => x.Truong == "donGia");
    }

    [Fact]
    public void TenDaiDungHaiTramKyTu_DuocChapNhan()
    {
        var loi = SanPhamValidator.Kiem(new SanPhamRequest(new string('a', 200), 1m, 1, 1, null));

        Assert.DoesNotContain(loi, x => x.Truong == "ten");
    }

    [Fact]
    public void ThieuTungTruongBatBuoc_BiTuChoiTheoTruong()
    {
        var loi = SanPhamValidator.Kiem(new SanPhamRequest(null, null, null, null, null));

        Assert.Contains(loi, x => x.Truong == "ten");
        Assert.Contains(loi, x => x.Truong == "donGia");
        Assert.Contains(loi, x => x.Truong == "tonKho");
        Assert.Contains(loi, x => x.Truong == "danhMucId");
    }

    [Fact]
    public void GiaToiDaCuaDecimal18_2_DuocChapNhan()
    {
        var loi = SanPhamValidator.Kiem(
            new SanPhamRequest("Sản phẩm", 9999999999999999.99m, 1, 1, null));

        Assert.DoesNotContain(loi, x => x.Truong == "donGia");
    }

    [Fact]
    public void GiaVuotDecimal18_2_BiTuChoi()
    {
        var loi = SanPhamValidator.Kiem(
            new SanPhamRequest("Sản phẩm", 10000000000000000m, 1, 1, null));

        Assert.Contains(loi, x => x.Truong == "donGia");
    }
}
