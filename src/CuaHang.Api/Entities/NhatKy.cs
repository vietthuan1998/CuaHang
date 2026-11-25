
namespace CuaHang.Api.Entities;

public class NhatKy
{
  public long Id { get; set; }
  public string NguoiThucHien { get; set; } = string.Empty;
  public string HanhDong { get; set; } = string.Empty;
  public string TenBang { get; set; } = string.Empty;
  public string BanGhiId { get; set; } = string.Empty;
  public string GiaTriTruoc { get; set; } = string.Empty;
  public string GiaTriSau { get; set; } = string.Empty;
  public DateTime NgayTao { get; set; } = DateTime.UtcNow;


}
