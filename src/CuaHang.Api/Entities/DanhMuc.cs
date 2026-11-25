using System;

namespace CuaHang.Api.Entities;

public class DanhMuc
{
  public int Id { get; set; }
  public string Ten { get; set; } = string.Empty;
  public DateTime NgayTao { get; set; } = DateTime.UtcNow;

}
