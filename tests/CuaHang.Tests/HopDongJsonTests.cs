using CuaHang.Api.Common;
using CuaHang.Api.Dtos;
using Newtonsoft.Json;

namespace CuaHang.Tests;

public class HopDongJsonTests
{
    [Fact]
    public void DTO_DungCamelCase_BoNull_GiuDecimalVaUtc()
    {
        var response = new DonHangResponse(7, 2, null, 1, 950000.00m, 950000.00m,
            CuaHang.Api.Entities.TrangThaiDon.Moi,
            new DateTime(2026, 11, 12, 1, 2, 3, DateTimeKind.Utc), null);

        var json = JsonConvert.SerializeObject(response, CauHinhJson.TaoSerializerSettings());

        Assert.Contains("\"thanhTien\":950000.00", json);
        Assert.Contains("\"ngayTao\":\"2026-11-12T01:02:03.000Z\"", json);
        Assert.DoesNotContain("sanPhamId", json);
        Assert.DoesNotContain("ngayCapNhat", json);
    }

}
