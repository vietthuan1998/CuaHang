using System;

namespace CuaHang.Api.Entities;

public class DonHang
{
  public int Id { get; set; }
  public int KhachHangId { get; set; }
  public int? SanPhamId { get; set; }
  public int SoLuong { get; set; }
  public decimal DonGia { get; set; }
  public decimal ThanhTien { get; set; }
  public TrangThaiDon TrangThai { get; set; } = TrangThaiDon.Moi;
  public DateTime NgayTao { get; set; } = DateTime.UtcNow;
  public DateTime? NgayCapNhat { get; set; }
  public string? NguoiTao { get; set; }
  public string? NguoiCapNhat { get; set; }
  public string? IdempotencyKey { get; set; }

  public SanPham? SanPham { get; set; }
  public KhachHang? KhachHang { get; set; }
  public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
}
