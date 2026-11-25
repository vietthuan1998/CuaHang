using System;

namespace CuaHang.Api.Entities;

public class KhachHang
{
  public int Id { get; set; }
  public string Ten { get; set; } = string.Empty;
  public string SoDienThoai { get; set; } = string.Empty;
  public string? Email { get; set; }
  public DateTime NgayTao { get; set; } = DateTime.UtcNow;

}
