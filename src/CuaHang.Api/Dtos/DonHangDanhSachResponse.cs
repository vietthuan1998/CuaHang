using CuaHang.Api.Entities;

namespace CuaHang.Api.Dtos;

public sealed record DonHangDanhSachResponse(
    int Id,
    int KhachHangId,
    string? TenKhachHang,
    int? SanPhamId,
    string? TenSanPham,
    int SoLuong,
    decimal ThanhTien,
    TrangThaiDon TrangThai,
    DateTime NgayTao);
