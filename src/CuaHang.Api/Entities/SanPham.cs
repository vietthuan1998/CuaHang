using System;

namespace CuaHang.Api.Entities;

public class SanPham
{
  public int Id { get; set; }
  public int DanhMucId { get; set; }
  public string Ten { get; set; } = string.Empty;
  public decimal DonGia { get; set; }
  public int TonKho { get; set; }
  public string? GhiChu { get; set; }
  public bool DaXoa { get; set; }
  //RowVersion
  public DateTime NgayTao { get; set; } = DateTime.UtcNow;
  public DateTime? NgayCapNhat { get; set; }
  public string? NguoiTao { get; set; }
  public string? NguoiCapNhat { get; set; }
  public byte[] RowVersion { get; set; } = [];
  public string? AnhTenTep { get; set; }
  public DanhMuc? DanhMuc { get; set; }
}
