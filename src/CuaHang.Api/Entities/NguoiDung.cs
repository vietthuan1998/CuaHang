namespace CuaHang.Api.Entities;

public sealed class NguoiDung
{
    public int Id { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhauBam { get; set; } = string.Empty;
    public string VaiTro { get; set; } = string.Empty;
    public bool HoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
}
