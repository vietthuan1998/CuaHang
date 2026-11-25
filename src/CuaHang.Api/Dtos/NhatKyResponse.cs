namespace CuaHang.Api.Dtos;

public record NhatKyResponse(
    long Id,
    string? NguoiThucHien,
    string HanhDong,
    string TenBang,
    string BanGhiId,
    string? GiaTriTruoc,
    string? GiaTriSau,
    DateTime NgayTao);
