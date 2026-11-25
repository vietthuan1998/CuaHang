using CuaHang.Api.Entities;

namespace CuaHang.Api.Dtos;

public record BaoCaoDonHangResponse(int Id, TrangThaiDon TrangThai, decimal ThanhTien, DateTime NgayTao);
