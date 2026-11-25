namespace CuaHang.Api.Entities;

public sealed class DongBoLichSu
{
    public long Id { get; set; }
    public DateTime KyBatDauUtc { get; set; }
    public string TrangThai { get; set; } = "DangChay";
    public int? MaTrangThaiNgoai { get; set; }
    public DateTime NgayCapNhatUtc { get; set; } = DateTime.UtcNow;
}
