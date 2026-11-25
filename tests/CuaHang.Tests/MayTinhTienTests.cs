using CuaHang.Api.Services;

namespace CuaHang.Tests;

public class MayTinhTienTests
{
    [Theory]
    [InlineData(9, 0.00)]
    [InlineData(10, 0.05)]
    [InlineData(19, 0.05)]
    [InlineData(20, 0.10)]
    public void TinhThanhTien_ApDungDungMucGiam(int soLuong, decimal mucGiam)
    {
        var ketQua = MayTinhTien.TinhThanhTien(100m, soLuong);

        Assert.Equal(100m * soLuong * (1 - mucGiam), ketQua);
    }

    [Fact]
    public void TinhThanhTien_LamTronHaiChuSoXaSoKhong()
    {
        var ketQua = MayTinhTien.TinhThanhTien(0.05m, 10);

        Assert.Equal(0.48m, ketQua);
    }
}
