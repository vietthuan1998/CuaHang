namespace CuaHang.Api.Dtos;

public sealed record BaoCaoSanPhamThangResponse(
    int SanPhamId,
    string Ten,
    DateTime? Thang,
    decimal DoanhSo,
    int SoDonHang);
