namespace CuaHang.Api.Reporting;

public sealed class BaoCaoSanPhamThang
{
    public int SanPhamId { get; set; }
    public string Ten { get; set; } = string.Empty;
    public DateTime? Thang { get; set; }
    public decimal DoanhSo { get; set; }
    public int SoDonHang { get; set; }
}
