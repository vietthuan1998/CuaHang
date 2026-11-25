namespace CuaHang.Api.Entities;

public sealed class ThanhToanWebhook
{
    public long Id { get; set; }
    public string MaThanhToanNgoai { get; set; } = string.Empty;
    public int DonHangId { get; set; }
    public DateTime NgayNhanUtc { get; set; } = DateTime.UtcNow;
}
