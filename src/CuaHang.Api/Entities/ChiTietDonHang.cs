namespace CuaHang.Api.Entities;

public sealed class ChiTietDonHang
{
    public int Id { get; set; }
    public int DonHangId { get; set; }
    public int? SanPhamId { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }

    public DonHang? DonHang { get; set; }
    public SanPham? SanPham { get; set; }
}
