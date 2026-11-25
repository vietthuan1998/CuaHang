namespace CuaHang.Api.Entities;

public sealed class HopThuDi
{
    public long Id { get; set; }
    public string LoaiSuKien { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "ChoGui";
    public int SoLanThu { get; set; }
    public DateTime GuiSauUtc { get; set; } = DateTime.UtcNow;
    public DateTime NgayTaoUtc { get; set; } = DateTime.UtcNow;
}
