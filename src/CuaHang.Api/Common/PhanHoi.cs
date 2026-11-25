using System;

namespace CuaHang.Api.Common;

public record LoiTruong(string Truong, string ThongBao);
public static class PhanHoi
{
  public static PhanHoi<T> Ok<T>(T duLieu)
    => new() { ThanhCong = true, DuLieu = duLieu };
  public static PhanHoi<object> Loi(string loi, IReadOnlyList<LoiTruong>? chiTiet = null)
    => new() { ThanhCong = false, Loi = loi, ChiTiet = chiTiet };

}
public class PhanHoi<T>
{
  public bool ThanhCong { get; set; }
  public T? DuLieu { get; set; }
  public string? Loi { get; set; }
  public IReadOnlyList<LoiTruong>? ChiTiet { get; set; }
}
