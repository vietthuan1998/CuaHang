using CuaHang.Api.Common;
using CuaHang.Api.Dtos;

namespace CuaHang.Api.Validation;

public class SanPhamValidator
{
  private const decimal DonGiaToiDa = 9999999999999999.99m;

  public static List<LoiTruong> Kiem(SanPhamRequest sp)
  {
    var loi = new List<LoiTruong>();
    if (string.IsNullOrWhiteSpace(sp.Ten))
      loi.Add(new("ten", "Tên không được trống"));
    else if (sp.Ten.Length > 200)
      loi.Add(new("ten", "Tên quá dài, tối đa 200 ký tự"));
    if (sp.TonKho == null)
      loi.Add(new("tonKho", "Thiếu tồn kho"));
    if (sp.TonKho is < 0)
      loi.Add(new("tonKho", "Tồn kho không được âm"));
    if (sp.DonGia == null)
      loi.Add(new("donGia", "Thiếu đơn giá"));
    if (sp.DonGia is < 0)
      loi.Add(new("donGia", "Đơn giá không được âm"));
    if (sp.DonGia.HasValue)
    {
      if (sp.DonGia.Value > DonGiaToiDa)
        loi.Add(new("donGia", "Đơn giá vượt giới hạn decimal(18,2)"));

      if (decimal.Round(sp.DonGia.Value, 2, MidpointRounding.AwayFromZero) != sp.DonGia.Value)
        loi.Add(new("donGia", "Chỉ được tối đa 2 số thập phân"));
    }
    if (!string.IsNullOrWhiteSpace(sp.GhiChu) && sp.GhiChu.Length > 500)
      loi.Add(new("ghiChu", "Ghi chú quá dài tối đa 500 ký tự"));
    if (sp.DanhMucId is null or <= 0)
      loi.Add(new("danhMucId", "Danh mục không chính xác"));
    return loi;
  }

}
