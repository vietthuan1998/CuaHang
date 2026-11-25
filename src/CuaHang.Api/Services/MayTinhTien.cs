namespace CuaHang.Api.Services;

public static class MayTinhTien
{
    public static decimal TinhThanhTien(decimal donGia, int soLuong)
    {
        var mucGiam = soLuong switch
        {
            >= 20 => 0.10m,
            >= 10 => 0.05m,
            _ => 0m
        };
        var thanhTien = donGia * soLuong * (1 - mucGiam);
        return decimal.Round(thanhTien, 2, MidpointRounding.AwayFromZero);
    }
}
