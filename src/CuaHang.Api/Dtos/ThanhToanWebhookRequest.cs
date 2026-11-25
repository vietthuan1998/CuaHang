namespace CuaHang.Api.Dtos;

public sealed record ThanhToanWebhookRequest(
    string? MaThanhToanNgoai,
    int? DonHangId,
    string? TrangThai);
