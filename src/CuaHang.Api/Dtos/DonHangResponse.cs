using CuaHang.Api.Entities;

namespace CuaHang.Api.Dtos;

public record DonHangResponse(
    int Id,
    int KhachHangId,
    int? SanPhamId,
    int SoLuong,
    decimal DonGia,
    decimal ThanhTien,
    TrangThaiDon TrangThai,
    DateTime NgayTao,
    DateTime? NgayCapNhat);
